﻿using System;
using System.Collections.Generic;
using PowerDS;

namespace DrakeScript
{
	public class Interpreter
	{
		public Context Context;
		//public FastStackGrowable<Scope> ScopeStack = new FastStackGrowable<Scope>(64);
		public SourceRef CallLocation = SourceRef.Invalid;
		public Value[] ArgList = new Value[0];
		public int ArgListCount = 0;
		//public Scope CurrentScope = Scope.Create();
		public FastStackGrowable<Value> Stack = new FastStackGrowable<Value>(1);

		public Interpreter(Context context)
		{
			Context = context;
		}

		public void Interpret(Function func)
		{
			//CurrentScope = Scope.Create();
			//ScopeStack.Push(CurrentScope);

			var callLocation = CallLocation;
			var code = func.Code;
			if (func.Args.Length > ArgListCount)
				throw new NotEnoughArgumentsException(func.Args.Length, ArgListCount, callLocation);
			var args = new Value[ArgListCount];
			for (var i = 0; i < ArgListCount; i++)
				args[i] = ArgList[i];
			Value[] locals = null;
			if (func.Locals.Length > 0)
				locals = new Value[func.Locals.Length];

			int ia;
			Value va, vb;
			bool exited = false;
			for (var pos = 0; pos < code.Length && !exited; pos++)
			{
				var instruction = code[pos];
				try
				{
					switch (instruction.Type)
					{
						case (Instruction.InstructionType.PushVarGlobal):
							Stack.Push(GetGlobalVar(instruction.Arg.String));
							break;
						case (Instruction.InstructionType.PushVarLocal):
							Stack.Push(locals[(int)instruction.Arg.Number]);
							break;
						case (Instruction.InstructionType.PushArg):
							Stack.Push(args[(int)instruction.Arg.Number]);
							break;
						case (Instruction.InstructionType.PushNum):
						case (Instruction.InstructionType.PushStr):
						case (Instruction.InstructionType.PushFunc):
							Stack.Push(instruction.Arg);
							break;
						case (Instruction.InstructionType.PushNil):
							Stack.Push(Value.Nil);
							break;
						case (Instruction.InstructionType.Pop):
							Stack.Pop();
							break;
						case (Instruction.InstructionType.PopVarGlobal):
							SetGlobalVar(instruction.Arg.String, Stack.Pop());
							break;
						case (Instruction.InstructionType.PopVarLocal):
							locals[(int)instruction.Arg.Number] = Stack.Pop();
							break;
						case (Instruction.InstructionType.PopArgs):
							ia = (int)instruction.Arg.Number;
							if (ArgList.Length < ia)
								Array.Resize(ref ArgList, ia);
							for (var i = ia - 1; i >= 0; i--)
							{
								ArgList[i] = Stack.Pop();
							}
							ArgListCount = ia;
							break;
						case (Instruction.InstructionType.Call):
							var callFunc = Stack.Pop();
							if (callFunc.Type != Value.ValueType.Function)
								throw new CannotCallNilException(instruction.Location);
							CallLocation = instruction.Location;
							callFunc.Function.InvokePushInsteadOfReturn(this);
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

						case (Instruction.InstructionType.IncVarGlobal):
							IncGlobalVar(instruction.Arg.String, 1);
							break;

						case (Instruction.InstructionType.IncVarLocal):
							ia = (int)instruction.Arg.Number;
							va = locals[ia];
							va.Number++;
							locals[ia] = va;
							break;

						case (Instruction.InstructionType.DecVarGlobal):
							IncGlobalVar(instruction.Arg.String, -1);
							break;

						case (Instruction.InstructionType.DecVarLocal):
							ia = (int)instruction.Arg.Number;
							va = locals[ia];
							va.Number--;
							locals[ia] = va;
							break;

						case (Instruction.InstructionType.IncVarByGlobal):
							IncGlobalVar(instruction.Arg.String, Stack.Pop().Number);
							break;

						case (Instruction.InstructionType.IncVarByLocal):
							ia = (int)instruction.Arg.Number;
							va = locals[ia];
							va.Number += Stack.Pop().Number;
							locals[ia] = va;
							break;

						case (Instruction.InstructionType.DecVarByGlobal):
							IncGlobalVar(instruction.Arg.String, -Stack.Pop().Number);
							break;

						case (Instruction.InstructionType.DecVarByLocal):
							ia = (int)instruction.Arg.Number;
							va = locals[ia];
							va.Number -= Stack.Pop().Number;
							locals[ia] = va;
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
							if (va.Number == 0.0)
							{
								pos += (int)instruction.Arg.Number;
							}
							break;

						case (Instruction.InstructionType.Jump):
							pos += (int)instruction.Arg.Number;
							break;

						case (Instruction.InstructionType.Return):
							//ScopeStack.Peek(1).Stack.Push(Stack.Pop());
							exited = true;
							break;

						default:
							throw new NoCaseForInstructionException(instruction.Type, instruction.Location);
					}
				}
				catch (Exception e)
				{
					throw new InterpreterException(e.ToString(), instruction.Location);
				}
			}

			/*ScopeStack.Pop();
			if (ScopeStack.Count > 0)
				CurrentScope = ScopeStack.Peek(0);
			else
				CurrentScope.Reset();*/
		}

		public Value GetGlobalVar(string name)
		{
			Value v;
			if (Context.Globals.TryGetValue(name, out v))
			{
				return v;
			}
			return Value.Nil;
		}

		public void SetGlobalVar(string name, Value v)
		{
			Context.Globals[name] = v;
		}

		public void IncGlobalVar(string name, double v)
		{
			Value val;
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

