﻿using System;
using System.Collections.Generic;
using PowerDS;

namespace DrakeScript
{
	public class Interpreter
	{
		public Context Context;
		public FastStackGrowable<Scope> ScopeStack = new FastStackGrowable<Scope>(4096);
		public FastStackGrowable<Value> Stack = new FastStackGrowable<Value>(1024);
		public SourceRef CallLocation = SourceRef.Invalid;
		public List<Value> ArgList = new List<Value>(16);

		public Interpreter(Context context)
		{
			Context = context;
		}

		public void Interpret(Function func)
		{
			ScopeStack.Push(Scope.Create());

			var callLocation = CallLocation;
			var code = func.Code;
			if (func.Args.Length > ArgList.Count)
				throw new NotEnoughArgumentsException(func.Args.Length, ArgList.Count, callLocation);
			var args = ArgList.ToArray();

			int ia, ib;
			Value va, vb;
			for (var pos = 0; pos < code.Length; pos++)
			{
				var instruction = code[pos];
				try
				{
					switch (instruction.Type)
					{
						case (Instruction.InstructionType.NewLoc):
							ScopeStack.Peek(0).Locals[instruction.Arg.String] = Value.Nil;
							break;
						case (Instruction.InstructionType.PushVar):
							Stack.Push(GetVar(instruction.Arg.String));
							break;
						case (Instruction.InstructionType.PushArg):
							Stack.Push(args[(int)instruction.Arg.Number]);
							break;
						case (Instruction.InstructionType.PushNum):
						case (Instruction.InstructionType.PushStr):
						case (Instruction.InstructionType.PushFunc):
							Stack.Push(instruction.Arg);
							break;
						case (Instruction.InstructionType.Pop):
							Stack.Pop();
							break;
						case (Instruction.InstructionType.PopVar):
							SetVar(instruction.Arg.String, Stack.Pop());
							break;
						case (Instruction.InstructionType.PopArgs):
							ArgList.Clear();
							ia = (int)instruction.Arg.Number;
							for (var i = 0; i < ia; i++)
							{
								ArgList.Add(Value.Nil);
							}
							for (var i = ia - 1; i >= 0; i--)
							{
								ArgList[i] = Stack.Pop();
							}
							break;
						case (Instruction.InstructionType.Call):
							var callFunc = Stack.Pop();
							if (callFunc.Type != Value.ValueType.Function)
								throw new CannotCallNilException(instruction.Location);
							CallLocation = instruction.Location;
							Stack.Push(callFunc.Function.Invoke(this));
							break;
						case (Instruction.InstructionType.Add):
							vb = Stack.Pop();
							va = Stack.Pop();
							va.Number += vb.Number;

							Stack.Push(va);
							break;
						case (Instruction.InstructionType.Sub):
							vb = Stack.Pop();
							va = Stack.Pop();
							va.Number -= vb.Number;

							Stack.Push(va);
							break;
						case (Instruction.InstructionType.Div):
							vb = Stack.Pop();
							va = Stack.Pop();
							va.Number /= vb.Number;

							Stack.Push(va);
							break;
						case (Instruction.InstructionType.Mul):
							vb = Stack.Pop();
							va = Stack.Pop();
							va.Number *= vb.Number;

							Stack.Push(va);
							break;

						case (Instruction.InstructionType.Not):
							va = Stack.Pop();
							va.Bool = !va.Bool;

							Stack.Push(va);
							break;

						case (Instruction.InstructionType.Neg):
							va = Stack.Pop();
							va.Number = -va.Number;

							Stack.Push(va);
							break;

						case (Instruction.InstructionType.Eq):
							vb = Stack.Pop();
							va = Stack.Pop();
							va.Number = (va.Number == vb.Number ? 1.0 : 0.0);

							Stack.Push(va);
							break;

						case (Instruction.InstructionType.NEq):
							vb = Stack.Pop();
							va = Stack.Pop();
							va.Number = (va.Number != vb.Number ? 1.0 : 0.0);

							Stack.Push(va);
							break;

						case (Instruction.InstructionType.Gt):
							vb = Stack.Pop();
							va = Stack.Pop();
							va.Number = (va.Number > vb.Number ? 1.0 : 0.0);

							Stack.Push(va);
							break;

						case (Instruction.InstructionType.GtEq):
							vb = Stack.Pop();
							va = Stack.Pop();
							va.Number = (va.Number >= vb.Number ? 1.0 : 0.0);

							Stack.Push(va);
							break;

						case (Instruction.InstructionType.Lt):
							vb = Stack.Pop();
							va = Stack.Pop();
							va.Number = (va.Number < vb.Number ? 1.0 : 0.0);

							Stack.Push(va);
							break;

						case (Instruction.InstructionType.LtEq):
							vb = Stack.Pop();
							va = Stack.Pop();
							va.Number = (va.Number <= vb.Number ? 1.0 : 0.0);

							Stack.Push(va);
							break;

						case (Instruction.InstructionType.IncVarBy):
							IncVar(instruction.Arg.String, Stack.Pop().Number);
							break;

						case (Instruction.InstructionType.DecVarBy):
							IncVar(instruction.Arg.String, -Stack.Pop().Number);
							break;


						case (Instruction.InstructionType.Dec):
							va = Stack.Peek(0);
							va.Number--;
							Stack.Poke(0, va);
							break;

						case (Instruction.InstructionType.Dup):
							Stack.Push(Stack.Peek(0));
							break;

						case (Instruction.InstructionType.JumpEZ):
							va = Stack.Pop();
							if (!va.Bool)
							{
								pos += (int)instruction.Arg.Number;
							}
							break;

						case (Instruction.InstructionType.Jump):
							pos += (int)instruction.Arg.Number;
							break;

						case (Instruction.InstructionType.EnterScope):
							ScopeStack.Push(Scope.Create());
							break;

						case (Instruction.InstructionType.ResetScope):
							ScopeStack.Peek(0).Reset();
							break;

						case (Instruction.InstructionType.LeaveScope):
							ScopeStack.Pop();
							break;
					}
				}
				catch (Exception e)
				{
					throw new InterpreterException(e.ToString(), instruction.Location);
				}
			}

			ScopeStack.Pop();
		}

		public Value GetVar(string name)
		{
			Value v;
			for (var i = ScopeStack.Count - 1; i >= 0; i--)
			{
				var scope = ScopeStack.GetRaw(i);
				if (scope.Locals.TryGetValue(name, out v))
				{
					return v;
				}
			}
			if (Context.Globals.TryGetValue(name, out v))
			{
				return v;
			}
			return Value.Nil;
		}

		public void SetVar(string name, Value v)
		{
			for (var i = ScopeStack.Count - 1; i >= 0; i--)
			{
				var scope = ScopeStack.GetRaw(i);
				if (scope.Locals.ContainsKey(name))
				{
					scope.Locals[name] = v;
					return;
				}
			}
			Context.Globals[name] = v;
		}

		public void IncVar(string name, double v)
		{
			Value val;
			for (var i = ScopeStack.Count - 1; i >= 0; i--)
			{
				var scope = ScopeStack.GetRaw(i);
				if (scope.Locals.TryGetValue(name, out val))
				{
					val.Number += v;
					scope.Locals[name] = val;
					return;
				}
			}
			if (!Context.Globals.TryGetValue(name, out val))
			{
				Context.Globals[name] = Value.Create(v);
			}
			else
			{
				val.Number += v;
				Context.Globals[name] = val;
			}
		}
	}
}

