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
		bool IsCoroutine = false;
		public InterpreterPause PauseStatus = InterpreterPause.Finished;
		public Dictionary<string, Value> DynamicLocalConstants = new Dictionary<string, Value>();
		public bool Yielded = false;

		public Interpreter(Context context, bool isCoroutine = false)
		{
			IsCoroutine = isCoroutine;
			Context = context;
		}

		public void Interpret(Function func)
		{
			//CurrentScope = Scope.Create();
			//ScopeStack.Push(CurrentScope);

			var code = func.Code;
			var callLocation = CallLocation;

			Value[] args;
			Value[] locals;
			int pos;
			if (PauseStatus.Paused)
			{
				pos = PauseStatus.Location;
				args = PauseStatus.Args;
				locals = PauseStatus.Locals;
			}
			else
			{
				if (func.Args.Length > ArgListCount)
					throw new NotEnoughArgumentsException(func.Args.Length, ArgListCount, callLocation);
				
				args = new Value[ArgListCount];
				for (var i = 0; i < ArgListCount; i++)
					args[i] = ArgList[i];

				locals = null;
				if (func.Locals.Length > 0)
					locals = new Value[func.Locals.Length];
				pos = 0;
			}

			PauseStatus = InterpreterPause.Finished;

			int ia;
			Value va, vb, vc;
			Value[] aa;
			bool exited = false;
			Yielded = false;
			for (; pos < code.Length && !exited && !Yielded; pos++)
			{
				var instruction = code[pos];
				//Console.WriteLine(pos + " " + instruction);
				try
				{
					switch (instruction.Type)
					{
						case (Instruction.InstructionType.PushVarGlobal):
							Stack.Push(GetGlobalVar(instruction.Arg.String));
							break;
						case (Instruction.InstructionType.PushVarLocal):
							Stack.Push(locals[instruction.Arg.IntNumber]);
							break;
						case (Instruction.InstructionType.PushArg):
							Stack.Push(args[instruction.Arg.IntNumber]);
							break;
						case (Instruction.InstructionType.PushNum):
						case (Instruction.InstructionType.PushStr):
						case (Instruction.InstructionType.PushFunc):
							Stack.Push(instruction.Arg);
							break;
						case (Instruction.InstructionType.Push0):
							Stack.Push(Value.Zero);
							break;
						case (Instruction.InstructionType.Push1):
							Stack.Push(Value.One);
							break;
						case (Instruction.InstructionType.PushArray):
							ia = instruction.Arg.IntNumber;
							aa = new Value[ia];
							for (var i = ia - 1; i >= 0; i--)
							{
								aa[i] = Stack.Pop();
							}
							Stack.Push(Value.Create(new List<Value>(aa)));
							break;
						case (Instruction.InstructionType.PushTable):
							Stack.Push(Value.Create(new Table()));
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
							locals[instruction.Arg.IntNumber] = Stack.Pop();
							break;
						case (Instruction.InstructionType.Call):
							var callFunc = Stack.Pop();
							ia = instruction.Arg.IntNumber;
							if (ArgList.Length < ia)
								Array.Resize(ref ArgList, ia);
							for (var i = ia - 1; i >= 0; i--)
							{
								ArgList[i] = Stack.Pop();
							}
							ArgListCount = ia;
							switch (callFunc.Type)
							{
								case (Value.ValueType.Function):
									CallLocation = instruction.Location;
									callFunc.Function.InvokePushInsteadOfReturn(this);
									break;
								case (Value.ValueType.Coroutine):
									CallLocation = instruction.Location;
									Stack.Push(callFunc.Coroutine.Resume(ArgList, ArgListCount));
									break;
								default:
									throw new CannotCallNilException(instruction.Location);
							}
							break;
						case (Instruction.InstructionType.PushIndex):
							vb = Stack.Pop();
							va = Stack.Pop();
							switch (va.Type)
							{
								case (Value.ValueType.Array):
									if (vb.Type != Value.ValueType.Number)
										throw new InvalidIndexTypeException("Array", vb.Type, instruction.Location);
									ia = (int)vb.Number;
									if (ia < 0 || ia >= va.Array.Count)
										throw new InvalidIndexValueException("Array", ia, instruction.Location);
									Stack.Push(va.Array[ia]);
									break;
								case (Value.ValueType.String):
									if (vb.Type != Value.ValueType.Number)
										throw new InvalidIndexTypeException("String", vb.Type, instruction.Location);
									ia = (int)vb.Number;
									if (ia < 0 || ia >= va.String.Length)
										throw new InvalidIndexValueException("String", ia, instruction.Location);
									Stack.Push(va.String[ia]);
									break;
								case (Value.ValueType.Table):
									Value outValue;
									if (va.Table.TryGetValue(vb.DynamicValue, out outValue))
										Stack.Push(outValue);
									else
										Stack.Push(Value.Nil);
									break;
								default:
									throw new CannotIndexTypeException(va.Type, instruction.Location);
							}
							break;
						case (Instruction.InstructionType.PopIndex):
							vc = Stack.Pop();
							vb = Stack.Pop();
							va = Stack.Pop();
							switch (va.Type)
							{
								case (Value.ValueType.Array):
									if (vb.Type != Value.ValueType.Number)
										throw new InvalidIndexTypeException("Array", vb.Type, instruction.Location);
									ia = (int)vb.Number;
									if (ia < 0)
										throw new BelowZeroIndexValueException("Array", ia, instruction.Location);
									var tempArray = va.Array;
									if (tempArray.Count <= ia)
									{
										for (var i = tempArray.Count; i < ia; i++)
											tempArray.Add(Value.Nil);
										tempArray.Add(vc);
									}
									else
										tempArray[ia] = vc;
									break;
								case (Value.ValueType.Table):
									va.Table[vb.DynamicValue] = vc;
									break;
								default:
									throw new CannotIndexTypeException(va.Type, instruction.Location);
							}
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

						case (Instruction.InstructionType.Mod):
							vb = Stack.Pop();
							va = Stack.Pop();
							va.Number %= vb.Number;

							Stack.Push(va);
							break;

						case (Instruction.InstructionType.Pow):
							vb = Stack.Pop();
							va = Stack.Pop();
							va.Number = Math.Pow(va.Number, vb.Number);

							Stack.Push(va);
							break;

						case (Instruction.InstructionType.Cat):
							vb = Stack.Pop();
							va = Stack.Pop();

							switch (va.Type)
							{
								case (Value.ValueType.String):
									va.String += vb.ToString();
									break;
								case (Value.ValueType.Array):
									if (vb.Type == Value.ValueType.Array)
									{
										var ret = new List<Value>(va.Array.Count + vb.Array.Count);
										ret.AddRange(va.Array);
										ret.AddRange(vb.Array);
										va.Array = ret;
									}
									else
										throw new UnexpectedRightTypeException(vb.Type, instruction.Location);
									break;
								case (Value.ValueType.Table):
									if (vb.Type == Value.ValueType.Table)
									{
										var ret = new Table(va.Table.InternalDictionary);
										foreach (var kvp in vb.Table.InternalDictionary)
										{
											ret[kvp.Key] = kvp.Value;
										}
										va.Table = ret;
									}
									else
										throw new UnexpectedRightTypeException(vb.Type, instruction.Location);
									break;
								default:
									va = Value.Create(va.ToString() + vb.ToString());
									break;
							}

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

							Stack.Push((va.Equals(vb) ? 1.0 : 0.0));
							break;

						case (Instruction.InstructionType.EqNil):
							va = Stack.Pop();

							Stack.Push(va.IsNil);
							break;

						case (Instruction.InstructionType.NEq):
							vb = Stack.Pop();
							va = Stack.Pop();

							Stack.Push((!va.Equals(vb) ? 1.0 : 0.0));
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
							ia = instruction.Arg.IntNumber;
							va = locals[ia];
							va.Number++;
							locals[ia] = va;
							break;

						case (Instruction.InstructionType.DecVarGlobal):
							IncGlobalVar(instruction.Arg.String, -1);
							break;

						case (Instruction.InstructionType.DecVarLocal):
							ia = instruction.Arg.IntNumber;
							va = locals[ia];
							va.Number--;
							locals[ia] = va;
							break;

						case (Instruction.InstructionType.IncVarByGlobal):
							IncGlobalVar(instruction.Arg.String, Stack.Pop().Number);
							break;

						case (Instruction.InstructionType.IncVarByLocal):
							ia = instruction.Arg.IntNumber;
							va = locals[ia];
							va.Number += Stack.Pop().Number;
							locals[ia] = va;
							break;

						case (Instruction.InstructionType.DecVarByGlobal):
							IncGlobalVar(instruction.Arg.String, -Stack.Pop().Number);
							break;

						case (Instruction.InstructionType.DecVarByLocal):
							ia = instruction.Arg.IntNumber;
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
								pos += instruction.Arg.IntNumber;
							}
							break;

						case (Instruction.InstructionType.JumpNZ):
							va = Stack.Pop();
							if (va.Number != 0.0)
							{
								pos += instruction.Arg.IntNumber;
							}
							break;

						case (Instruction.InstructionType.Jump):
							pos += instruction.Arg.IntNumber;
							break;

						case (Instruction.InstructionType.Return):
							//ScopeStack.Peek(1).Stack.Push(Stack.Pop());
							exited = true;
							Yielded = false;
							break;

						case (Instruction.InstructionType.Yield):
							//ScopeStack.Peek(1).Stack.Push(Stack.Pop());
							exited = true;
							Yielded = true;
							break;

						case (Instruction.InstructionType.Is):
							vb = Stack.Pop();
							va = Stack.Pop();
							Stack.Push(va.ActualType == vb.ObjectAs<Type>());
							break;

						case (Instruction.InstructionType.Contains):
							vb = Stack.Pop();
							va = Stack.Pop();
							bool found = false;
							switch (va.Type)
							{
								case (Value.ValueType.Array):
									foreach (var v in va.Array)
									{
										if (v.Equals(vb))
										{
											found = true;
											break;
										}
									}
									Stack.Push(found);
									break;
								case (Value.ValueType.Table):
									foreach (var v in va.Table.InternalDictionary.Keys)
									{
										if (v.Equals(vb.DynamicValue))
										{
											found = true;
											break;
										}
									}
									Stack.Push(found);
									break;
								default:
									throw new CannotIndexTypeException(va.Type, instruction.Location);
							}
							break;

						default:
							throw new NoCaseForInstructionException(instruction.Type, instruction.Location);
					}

					/*Console.Write("Stack: ");
					foreach (Value item in Stack)
					{
						Console.Write(item.ToString());
						Console.Write(", ");
					}
					Console.WriteLine();*/
				}
				catch (Exception e)
				{
					throw new InterpreterException(e.ToString(), instruction.Location);
				}
			}

			if (Yielded)
				PauseStatus = new InterpreterPause(pos, locals, args);

			/*ScopeStack.Pop();
			if (ScopeStack.Count > 0)
				CurrentScope = ScopeStack.Peek(0);
			else
				CurrentScope.Reset();*/
		}

		public Value GetGlobalVar(string name)
		{
			Value v;
			if (DynamicLocalConstants != null && DynamicLocalConstants.TryGetValue(name, out v))
			{
				return v;
			}
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


		public void Yield()
		{
			if (Yielded)
				return;
			Stack.Push(Value.Nil);
			Yielded = true;
		}
	}
}

