﻿using System;
using System.Collections.Generic;
using System.Linq;
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

            Dictionary<object, Tuple<int, Value[]>> iterators = null;
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

            func.LastRunLocals = locals;

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
							Stack.Push(GetGlobalVar(instruction.Arg.StringDirect));
							break;
						case (Instruction.InstructionType.PushVarLocal):
							Stack.Push(GetLocalVar((int)instruction.Arg.Number, func, locals));
							break;
						case (Instruction.InstructionType.PushArg):
							Stack.Push(args[(int)instruction.Arg.Number]);
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
							ia = (int)instruction.Arg.Number;
							aa = new Value[ia];
							for (var i = ia - 1; i >= 0; i--)
							{
								aa[i] = Stack.Pop();
							}
							Stack.Push(Value.Create(new List<Value>(aa)));
							break;
                        case (Instruction.InstructionType.PushArrayI):
                            var la = instruction.Arg.ArrayDirect;
                            var lb = new List<Value>(la.Count);
                            for (var i = 0; i < la.Count; i++)
                            {
                                lb.Add(la[i]);
                            }
                            Stack.Push(Value.Create(lb));
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
							SetGlobalVar(instruction.Arg.StringDirect, Stack.Pop());
							break;
						case (Instruction.InstructionType.PopVarLocal):
                            SetLocalVar((int)instruction.Arg.Number, Stack.Pop(), func, locals);
							break;
                        case (Instruction.InstructionType.PopArg):
                            args[(int)instruction.Arg.Number] = Stack.Pop();
                            break;
                        case (Instruction.InstructionType.Call):
							ia = (int)instruction.Arg.Number;
							if (ArgList.Length < ia)
								Array.Resize(ref ArgList, ia);
							for (var i = ia - 1; i >= 0; i--)
							{
								ArgList[i] = Stack.Pop();
							}
							var callFunc = Stack.Pop();
							ArgListCount = ia;
							switch (callFunc.Type)
							{
								case (Value.ValueType.Function):
                                    CallLocation = instruction.Location;
                                    callFunc.FunctionDirect.InvokePushInsteadOfReturn(this);
                                    break;
								case (Value.ValueType.Coroutine):
									CallLocation = instruction.Location;
                                    Stack.Push(callFunc.CoroutineDirect.Resume(ArgList, ArgListCount));
                                    break;
								default:
                                    ArgListCount += 1;
                                    var method = GetMethod(callFunc, "Call");
                                    if (method != null)
                                    {
                                        CallLocation = instruction.Location;
                                        if (ArgList.Length < ia + 1)
                                            Array.Resize(ref ArgList, ia + 1);
                                        for (var i = ia; i > 0; i--)
                                        {
                                            ArgList[i] = ArgList[i - 1];
                                        }
                                        ArgList[0] = callFunc;
                                        method.InvokePushInsteadOfReturn(this);
                                    }
                                    else
                                        throw new CannotCallTypeException(callFunc.Type, instruction.Location);
                                    break;
							}
							break;
                        case (Instruction.InstructionType.NewThread):
                            ia = (int)instruction.Arg.Number;
                            if (ArgList.Length < ia)
                                Array.Resize(ref ArgList, ia);
                            for (var i = ia - 1; i >= 0; i--)
                            {
                                ArgList[i] = Stack.Pop();
                            }
                            callFunc = Stack.Pop();
                            ArgListCount = ia;
                            switch (callFunc.Type)
                            {
                                case (Value.ValueType.Function):
                                    var threadInt = new Interpreter(Context);
                                    threadInt.ArgList = (Value[])ArgList.Clone();
                                    threadInt.ArgListCount = ArgListCount;
                                    threadInt.CallLocation = instruction.Location;
                                    var nthread = new System.Threading.Thread(
                                        () =>
                                        {
                                            callFunc.FunctionDirect.Invoke(threadInt);
                                        }
                                    );
                                    nthread.Start();
                                    Stack.Push(Value.Create(nthread));
                                    break;
                                default:
                                    ArgListCount += 1;
                                    var method = GetMethod(callFunc, "Call");
                                    if (method != null)
                                    {
                                        if (ArgList.Length < ia + 1)
                                            Array.Resize(ref ArgList, ia + 1);
                                        for (var i = ia; i > 0; i--)
                                        {
                                            ArgList[i] = ArgList[i - 1];
                                        }
                                        ArgList[0] = callFunc;
                                        threadInt = new Interpreter(Context);
                                        threadInt.ArgList = (Value[])ArgList.Clone();
                                        threadInt.ArgListCount = ArgListCount;
                                        threadInt.CallLocation = instruction.Location;
                                        nthread = new System.Threading.Thread(
                                            () =>
                                            {
                                                method.Invoke(threadInt);
                                            }
                                        );
                                        nthread.Start();
                                        Stack.Push(Value.Create(nthread));
                                    }
                                    else
                                        throw new CannotThreadTypeException(callFunc.Type, instruction.Location);
                                    break;
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
									if (ia < 0 || ia >= va.ArrayDirect.Count)
										throw new InvalidIndexValueException("Array", ia, instruction.Location);
									Stack.Push(va.ArrayDirect[ia]);
									break;
								case (Value.ValueType.String):
									if (vb.Type != Value.ValueType.Number)
										throw new InvalidIndexTypeException("String", vb.Type, instruction.Location);
									ia = (int)vb.Number;
									if (ia < 0 || ia >= va.StringDirect.Length)
										throw new InvalidIndexValueException("String", ia, instruction.Location);
									Stack.Push(va.StringDirect[ia]);
									break;
								case (Value.ValueType.Table):
									Value outValue;
									if (va.TableDirect.TryGetValue(vb, out outValue))
										Stack.Push(outValue);
									else
										Stack.Push(Value.Nil);
									break;
                                case (Value.ValueType.Object):
                                    if (va.Is<Type>())
                                    {
                                        if (vb.Type != Value.ValueType.String)
                                            throw new InvalidIndexTypeException("Type", vb.Type, instruction.Location);
                                        var type = va.ObjectAs<Type>();
                                        var methodName = vb.StringDirect;
                                        Dictionary<string, Function> methodDict;
                                        if (!Context.Methods.TryGetValue(type, out methodDict))
                                            Stack.Push(Value.Nil);
                                        else
                                            Stack.Push(methodDict[methodName]);
                                    }
                                    else
                                    {
                                        if (va.Is<IIndexable>())
                                            Stack.Push(((IIndexable)va.Object).GetValue(vb, instruction.Location));
                                        else
                                            throw new CannotIndexTypeException(va.ActualType, instruction.Location);
                                    }
                                    break;
                                default:
									throw new CannotIndexTypeException(va.Type, instruction.Location);
							}
							break;
                        case (Instruction.InstructionType.PushIndexInt):
                            va = Stack.Pop();

                            switch (va.Type)
                            {
                                case (Value.ValueType.Array):
                                    ia = (int)instruction.Arg.Number;
                                    if (ia < 0 || ia >= va.ArrayDirect.Count)
                                        throw new InvalidIndexValueException("Array", ia, instruction.Location);
                                    Stack.Push(va.ArrayDirect[ia]);
                                    break;
                                case (Value.ValueType.String):
                                    ia = (int)instruction.Arg.Number;
                                    if (ia < 0 || ia >= va.StringDirect.Length)
                                        throw new InvalidIndexValueException("String", ia, instruction.Location);
                                    Stack.Push(va.StringDirect[ia]);
                                    break;
                                case (Value.ValueType.Table):
                                    Value outValue;
                                    if (va.TableDirect.TryGetValue((double)(int)instruction.Arg.Number, out outValue))
                                        Stack.Push(outValue);
                                    else
                                        Stack.Push(Value.Nil);
                                    break;
                                case (Value.ValueType.Object):
                                    if (va.Is<Type>())
                                    {
                                        throw new InvalidIndexTypeException("Type", Value.ValueType.Number, instruction.Location);
                                    }
                                    else
                                    {
                                        if (va.Is<IIndexable>())
                                            Stack.Push(((IIndexable)va.Object).GetValue((double)(int)instruction.Arg.Number, instruction.Location));
                                        else
                                            throw new CannotIndexTypeException(va.ActualType, instruction.Location);
                                    }
                                    break;
                                default:
                                    throw new CannotIndexTypeException(va.Type, instruction.Location);
                            }
                            break;
                        case (Instruction.InstructionType.PushIndexStr):
                            va = Stack.Pop();

                            switch (va.Type)
                            {
                                case (Value.ValueType.Array):
                                    throw new InvalidIndexTypeException("Array", Value.ValueType.String, instruction.Location);
                                case (Value.ValueType.String):
                                    throw new InvalidIndexTypeException("String", Value.ValueType.String, instruction.Location);
                                case (Value.ValueType.Table):
                                    Value outValue;
                                    if (va.TableDirect.TryGetValue(instruction.Arg.StringDirect, out outValue))
                                        Stack.Push(outValue);
                                    else
                                        Stack.Push(Value.Nil);
                                    break;
                                case (Value.ValueType.Object):
                                    if (va.Is<Type>())
                                    {
                                        var type = va.ObjectAs<Type>();
                                        var methodName = instruction.Arg.StringDirect;
                                        Dictionary<string, Function> methodDict;
                                        if (!Context.Methods.TryGetValue(type, out methodDict))
                                            Stack.Push(Value.Nil);
                                        else
                                            Stack.Push(methodDict[methodName]);
                                    }
                                    else
                                    {
                                        if (va.Is<IIndexable>())
                                            Stack.Push(((IIndexable)va.Object).GetValue(instruction.Arg.StringDirect, instruction.Location));
                                        else
                                            throw new CannotIndexTypeException(va.ActualType, instruction.Location);
                                    }
                                    break;
                                default:
                                    throw new CannotIndexTypeException(va.Type, instruction.Location);
                            }
                            break;
                        case (Instruction.InstructionType.PushMethod):
                            va = Stack.Pop();

                            {
                                var method = GetMethod(va, instruction.Arg.StringDirect);
                                if (method != null)
                                {
                                    Stack.Push(method);
                                    break;
                                }
                                if (va.Type == Value.ValueType.Object)
                                    throw new NoMethodForTypeException(va.ActualType, instruction.Arg.StringDirect, instruction.Location);
                                throw new NoMethodForTypeException(va.Type, instruction.Arg.StringDirect, instruction.Location);
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
									var tempArray = va.ArrayDirect;
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
									va.TableDirect[vb] = vc;
									break;
								case (Value.ValueType.Object):
									if (va.Is<Type>())
									{
										if (vb.Type != Value.ValueType.String)
											throw new InvalidIndexTypeException("Type", vb.Type, instruction.Location);
										if (vc.Type != Value.ValueType.Function)
											throw new UnexpectedTypeException(vc.Type, instruction.Location);
										var type = va.ObjectAs<Type>();
										var methodName = vb.StringDirect;
										Dictionary<string, Function> methodDict;
										if (!Context.Methods.TryGetValue(type, out methodDict))
										{
											methodDict = new Dictionary<string, Function>();
											Context.Methods.Add(type, methodDict);
										}
										methodDict[methodName] = vc.FunctionDirect;
									}
                                    else
                                    {
                                        if (va.Is<IIndexable>())
                                            ((IIndexable)va.Object).SetValue(vb, vc, instruction.Location);
                                        else
                                            throw new CannotIndexTypeException(va.ActualType, instruction.Location);
                                    }
									break;
								default:
									throw new CannotIndexTypeException(va.Type, instruction.Location);
							}
							break;
						case (Instruction.InstructionType.Add):
							vb = Stack.Pop();
							va = Stack.Pop();
                            if (va.Type == Value.ValueType.Number)
                            {
                                va.Number += vb.Number;
                                Stack.Push(va);
                            }
                            else
                            {
                                var vaType = va.ActualType;
                                var method = GetMethod(va, "Add");
                                if (method != null)
                                    Stack.Push(method.Invoke(va, vb));
                                else
                                {
                                    if (va.Type == Value.ValueType.Object)
                                        throw new UnexpectedLeftTypeException(vaType, instruction.Location);
                                    throw new UnexpectedLeftTypeException(va.Type, instruction.Location);
                                }
                            }
							break;
						case (Instruction.InstructionType.Sub):
							vb = Stack.Pop();
							va = Stack.Pop();
                            if (va.Type == Value.ValueType.Number)
                            {
                                va.Number -= vb.Number;
                                Stack.Push(va);
                            }
                            else
                            {
                                var vaType = va.ActualType;
                                var method = GetMethod(va, "Subtract");
                                if (method != null)
                                    Stack.Push(method.Invoke(va, vb));
                                else
                                {
                                    if (va.Type == Value.ValueType.Object)
                                        throw new UnexpectedLeftTypeException(vaType, instruction.Location);
                                    throw new UnexpectedLeftTypeException(va.Type, instruction.Location);
                                }
                            }
                            break;
						case (Instruction.InstructionType.Div):
							vb = Stack.Pop();
							va = Stack.Pop();
                            if (va.Type == Value.ValueType.Number)
                            {
                                va.Number /= vb.Number;
                                Stack.Push(va);
                            }
                            else
                            {
                                var vaType = va.ActualType;
                                var method = GetMethod(va, "Divide");
                                if (method != null)
                                    Stack.Push(method.Invoke(va, vb));
                                else
                                {
                                    if (va.Type == Value.ValueType.Object)
                                        throw new UnexpectedLeftTypeException(vaType, instruction.Location);
                                    throw new UnexpectedLeftTypeException(va.Type, instruction.Location);
                                }
                            }
                            break;
						case (Instruction.InstructionType.Mul):
							vb = Stack.Pop();
							va = Stack.Pop();
                            if (va.Type == Value.ValueType.Number)
                            {
                                va.Number *= vb.Number;
                                Stack.Push(va);
                            }
                            else
                            {
                                var vaType = va.ActualType;
                                var method = GetMethod(va, "Multiply");
                                if (method != null)
                                    Stack.Push(method.Invoke(va, vb));
                                else
                                {
                                    if (va.Type == Value.ValueType.Object)
                                        throw new UnexpectedLeftTypeException(vaType, instruction.Location);
                                    throw new UnexpectedLeftTypeException(va.Type, instruction.Location);
                                }
                            }
                            break;

						case (Instruction.InstructionType.Mod):
							vb = Stack.Pop();
							va = Stack.Pop();
                            if (va.Type == Value.ValueType.Number)
                            {
                                va.Number %= vb.Number;
                                Stack.Push(va);
                            }
                            else
                            {
                                var vaType = va.ActualType;
                                var method = GetMethod(va, "Modulo");
                                if (method != null)
                                    Stack.Push(method.Invoke(va, vb));
                                else
                                {
                                    if (va.Type == Value.ValueType.Object)
                                        throw new UnexpectedLeftTypeException(vaType, instruction.Location);
                                    throw new UnexpectedLeftTypeException(va.Type, instruction.Location);
                                }
                            }
                            break;

						case (Instruction.InstructionType.Pow):
							vb = Stack.Pop();
							va = Stack.Pop();
                            if (va.Type == Value.ValueType.Number)
                            {
                                va.Number = Math.Pow(va.Number, vb.Number);
                                Stack.Push(va);
                            }
                            else
                            {
                                var vaType = va.ActualType;
                                var method = GetMethod(va, "Power");
                                if (method != null)
                                    Stack.Push(method.Invoke(va, vb));
                                else
                                {
                                    if (va.Type == Value.ValueType.Object)
                                        throw new UnexpectedLeftTypeException(vaType, instruction.Location);
                                    throw new UnexpectedLeftTypeException(va.Type, instruction.Location);
                                }
                            }
							break;

						case (Instruction.InstructionType.Cat):
							vb = Stack.Pop();
							va = Stack.Pop();

							switch (va.Type)
							{
								case (Value.ValueType.String):
									va.StringDirect += vb.ToString(Context);
									break;
								case (Value.ValueType.Array):
									if (vb.Type == Value.ValueType.Array)
									{
										var ret = new List<Value>(va.ArrayDirect.Count + vb.ArrayDirect.Count);
										ret.AddRange(va.ArrayDirect);
										ret.AddRange(vb.ArrayDirect);
										va.ArrayDirect = ret;
									}
									else
										throw new UnexpectedRightTypeException(vb.Type, instruction.Location);
									break;
								case (Value.ValueType.Table):
									if (vb.Type == Value.ValueType.Table)
									{
										var ret = new Table(va.TableDirect.InternalDictionary);
										foreach (var kvp in vb.TableDirect.InternalDictionary)
										{
											ret[kvp.Key] = kvp.Value;
										}
										va.TableDirect = ret;
									}
									else
										throw new UnexpectedRightTypeException(vb.Type, instruction.Location);
									break;
								default:
                                    var vaType = va.ActualType;
                                    var method = GetMethod(va, "Concat");
                                    if (method != null)
                                        va = method.Invoke(va, vb);
                                    else
                                    {
                                        va = Value.Create(va.ToString(Context) + vb.ToString(Context));
                                    }
									break;
							}

							Stack.Push(va);
							break;

						case (Instruction.InstructionType.Not):
							va = Stack.Pop();
                            if (va.Type == Value.ValueType.Number)
                            {
                                va.Bool = !va.Bool;
                                Stack.Push(va);
                            }
                            else
                            {
                                var vaType = va.ActualType;
                                var method = GetMethod(va, "Not");
                                if (method != null)
                                    Stack.Push(method.Invoke(va));
                                else
                                {
                                    if (va.Type == Value.ValueType.Object)
                                        throw new UnexpectedLeftTypeException(vaType, instruction.Location);
                                    throw new UnexpectedLeftTypeException(va.Type, instruction.Location);
                                }
                            }
							break;

						case (Instruction.InstructionType.Neg):
							va = Stack.Pop();
                            if (va.Type == Value.ValueType.Number)
                            {
                                va.Number = -va.Number;
                                Stack.Push(va);
                            }
                            else
                            {
                                var vaType = va.ActualType;
                                var method = GetMethod(va, "Negative");
                                if (method != null)
                                    Stack.Push(method.Invoke(va));
                                else
                                {
                                    if (va.Type == Value.ValueType.Object)
                                        throw new UnexpectedLeftTypeException(vaType, instruction.Location);
                                    throw new UnexpectedLeftTypeException(va.Type, instruction.Location);
                                }
                            }
                            break;

						case (Instruction.InstructionType.Eq):
							vb = Stack.Pop();
							va = Stack.Pop();
                            {
                                var vaType = va.ActualType;
                                var method = GetMethod(va, "Equals");
                                if (method != null)
                                    Stack.Push(method.Invoke(va, vb));
                                else
                                {
                                    Stack.Push((va.Equals(vb) ? 1.0 : 0.0));
                                }
                            }
                            break;

                        case (Instruction.InstructionType.SEq):
                            vb = Stack.Pop();
                            va = Stack.Pop();

                            if (vb.Type != va.Type)
                            {
                                Stack.Push(0.0);
                                break;
                            }
                            if (va.Type == Value.ValueType.Table)
                            {
                                if (va.TableDirect.Count != vb.TableDirect.Count)
                                {
                                    Stack.Push(0.0);
                                    break;
                                }
                                ia = 0;
                                foreach (var kvp in va.TableDirect.InternalDictionary)
                                {
                                    if (!vb.TableDirect.InternalDictionary.TryGetValue(kvp.Key, out vc))
                                    {
                                        Stack.Push(0.0);
                                        ia = -1;
                                        break;
                                    }
                                    if ((vc.Object == null && kvp.Value.Object != null) || !vc.Equals(kvp.Value))
                                    {
                                        Stack.Push(0.0);
                                        ia = -1;
                                        break;
                                    }
                                }
                                if (ia == -1)
                                    break;
                                Stack.Push(1.0);
                                break;
                            }
                            if (va.Type == Value.ValueType.Array)
                            {
                                if (va.ArrayDirect.Count != vb.ArrayDirect.Count)
                                {
                                    Stack.Push(0.0);
                                    break;
                                }
                                ia = va.ArrayDirect.Count;
                                for (var i = 0; i < ia; i++)
                                {
                                    if ((va.ArrayDirect[i].Object == null && vb.ArrayDirect[i].Object != null) || !va.ArrayDirect[i].Equals(vb.ArrayDirect[i]))
                                    {
                                        Stack.Push(0.0);
                                        ia = -1;
                                        break;
                                    }
                                }
                                if (ia == -1)
                                    break;
                                Stack.Push(1.0);
                                break;
                            }
                            if (va.Object is IEnumerable<KeyValuePair<object, Value>>)
                            {
                                Stack.Push(((IEnumerable<KeyValuePair<object, Value>>)va.Object).SequenceEqual(((IEnumerable<KeyValuePair<object, Value>>)vb.Object)) ? 1.0 : 0.0);
                                break;
                            }
                            if (va.Object is IEnumerable<Value>)
                            {
                                Stack.Push(((IEnumerable<Value>)va.Object).SequenceEqual(((IEnumerable<Value>)vb.Object)) ? 1.0 : 0.0);
                                break;
                            }

                            {
                                var vaType = va.ActualType;
                                var method = GetMethod(va, "SequenceEquals");
                                if (method != null)
                                    Stack.Push(method.Invoke(va, vb));
                                else
                                {
                                    Stack.Push(0.0);
                                }
                            }
                            break;

                        case (Instruction.InstructionType.EqNil):
							va = Stack.Pop();

							Stack.Push(va.IsNil);
							break;

						case (Instruction.InstructionType.NEq):
							vb = Stack.Pop();
							va = Stack.Pop();

                            {
                                var vaType = va.ActualType;
                                var method = GetMethod(va, "Equals");
                                if (method != null)
                                    Stack.Push(method.Invoke(va, vb).Number.EqualsZeroSafe());
                                else
                                {
                                    Stack.Push((va.Equals(vb) ? 0.0 : 1.0));
                                }
                            }
                            break;

						case (Instruction.InstructionType.Gt):
							vb = Stack.Pop();
							va = Stack.Pop();
                            if (va.Type == Value.ValueType.Number)
                            {
                                Stack.Push((va > vb ? 1.0 : 0.0));
                            }
                            else
                            {
                                var vaType = va.ActualType;
                                var method = GetMethod(va, "GreaterThan");
                                if (method != null)
                                    Stack.Push(method.Invoke(va, vb));
                                else
                                {
                                    if (va.Type == Value.ValueType.Object)
                                        throw new UnexpectedLeftTypeException(va.ActualType, instruction.Location);
                                    throw new UnexpectedLeftTypeException(va.Type, instruction.Location);
                                }
                            }
                            break;

						case (Instruction.InstructionType.GtEq):
							vb = Stack.Pop();
							va = Stack.Pop();
                            if (va.Type == Value.ValueType.Number)
                            {
                                Stack.Push((va >= vb ? 1.0 : 0.0));
                            }
                            else
                            {
                                var method = GetMethod(va, "GreaterThan");
                                if (method != null)
                                    Stack.Push(method.Invoke(vb, va).Number.EqualsZeroSafe());
                                else
                                {
                                    if (va.Type == Value.ValueType.Object)
                                        throw new UnexpectedLeftTypeException(va.ActualType, instruction.Location);
                                    throw new UnexpectedLeftTypeException(va.Type, instruction.Location);
                                }
                            }
                            break;

						case (Instruction.InstructionType.Lt):
							vb = Stack.Pop();
							va = Stack.Pop();
                            if (va.Type == Value.ValueType.Number)
                            {
                                Stack.Push((va < vb ? 1.0 : 0.0));
                            }
                            else
                            {
                                var method = GetMethod(va, "GreaterThan");
                                if (method != null)
                                    Stack.Push(method.Invoke(vb, va));
                                else
                                {
                                    if (va.Type == Value.ValueType.Object)
                                        throw new UnexpectedLeftTypeException(va.ActualType, instruction.Location);
                                    throw new UnexpectedLeftTypeException(va.Type, instruction.Location);
                                }
                            }
                            break;

						case (Instruction.InstructionType.LtEq):
							vb = Stack.Pop();
							va = Stack.Pop();
                            if (va.Type == Value.ValueType.Number)
                            {
                                Stack.Push((va <= vb ? 1.0 : 0.0));
                            }
                            else
                            {
                                var method = GetMethod(va, "GreaterThan");
                                if (method != null)
                                    Stack.Push(method.Invoke(va, vb).Number.EqualsZeroSafe());
                                else
                                {
                                    if (va.Type == Value.ValueType.Object)
                                        throw new UnexpectedLeftTypeException(va.ActualType, instruction.Location);
                                    throw new UnexpectedLeftTypeException(va.Type, instruction.Location);
                                }
                            }
                            break;

						case (Instruction.InstructionType.IncVarGlobal):
							IncGlobalVar(instruction.Arg.StringDirect, 1);
							break;

						case (Instruction.InstructionType.IncVarLocal):
							ia = (int)instruction.Arg.Number;
							va = GetLocalVar(ia, func, locals);
							va.Number++;
                            SetLocalVar(ia, va, func, locals);
							break;

						case (Instruction.InstructionType.DecVarGlobal):
							IncGlobalVar(instruction.Arg.StringDirect, -1);
							break;

						case (Instruction.InstructionType.DecVarLocal):
							ia = (int)instruction.Arg.Number;
							va = GetLocalVar(ia, func, locals);
							va.Number--;
                            SetLocalVar(ia, va, func, locals);
							break;

						case (Instruction.InstructionType.IncVarByGlobal):
							IncGlobalVar(instruction.Arg.StringDirect, Stack.Pop().Number);
							break;

						case (Instruction.InstructionType.IncVarByLocal):
							ia = (int)instruction.Arg.Number;
							va = GetLocalVar(ia, func, locals);
							va.Number += Stack.Pop().Number;
                            SetLocalVar(ia, va, func, locals);
							break;

						case (Instruction.InstructionType.DecVarByGlobal):
							IncGlobalVar(instruction.Arg.StringDirect, -Stack.Pop().Number);
							break;

						case (Instruction.InstructionType.DecVarByLocal):
							ia = (int)instruction.Arg.Number;
							va = GetLocalVar(ia, func, locals);
							va.Number -= Stack.Pop().Number;
                            SetLocalVar(ia, va, func, locals);
							break;

                        case (Instruction.InstructionType.Inc):
                            va = Stack.Peek(0);
                            va.Number++;
                            Stack.Poke(0, va);
                            break;

                        case (Instruction.InstructionType.Dec):
							va = Stack.Peek(0);
							va.Number--;
							Stack.Poke(0, va);
							break;

						case (Instruction.InstructionType.Dup):
							Stack.Push(Stack.Peek(0));
							break;

						case (Instruction.InstructionType.Swap):
							vb = Stack.Pop();
							va = Stack.Pop();
							Stack.Push(vb);
							Stack.Push(va);
							break;

						case (Instruction.InstructionType.JumpEZ):
							va = Stack.Pop();
							if (va.Number == 0.0)
							{
								pos += (int)instruction.Arg.Number;
							}
							break;

						case (Instruction.InstructionType.JumpNZ):
							va = Stack.Pop();
							if (va.Number != 0.0)
							{
								pos += (int)instruction.Arg.Number;
							}
							break;

						case (Instruction.InstructionType.Jump):
							pos += (int)instruction.Arg.Number;
							break;

						case (Instruction.InstructionType.Return):
							exited = true;
							Yielded = false;
							break;

                        case (Instruction.InstructionType.ReturnNil):
                            Stack.Push(Value.Nil);
                            exited = true;
                            Yielded = false;
                            break;

                        case (Instruction.InstructionType.Return0):
                            Stack.Push(Value.Zero);
                            exited = true;
                            Yielded = false;
                            break;

                        case (Instruction.InstructionType.Return1):
                            Stack.Push(Value.One);
                            exited = true;
                            Yielded = false;
                            break;

                        case (Instruction.InstructionType.Yield):
							exited = true;
							Yielded = true;
							break;

						case (Instruction.InstructionType.Is):
							vb = Stack.Pop();
							va = Stack.Pop();
                            {
                                var method = GetMethod(va, "Is");
                                if (method != null)
                                    Stack.Push(method.Invoke(va, vb));
                                else
                                {
                                    if (vb.Is<Type>())
                                        Stack.Push(va.ActualType == vb.ObjectAs<Type>());
                                    else
                                    {
                                        if (vb.Type == Value.ValueType.Object)
                                            throw new UnexpectedRightTypeException(vb.ActualType, instruction.Location);
                                        throw new UnexpectedRightTypeException(vb.Type, instruction.Location);
                                    }
                                }
                            }
                            
							break;

						case (Instruction.InstructionType.Contains):
							vb = Stack.Pop();
							va = Stack.Pop();
							bool found = false;
							switch (va.Type)
							{
								case (Value.ValueType.Array):
									foreach (var v in va.ArrayDirect)
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
									foreach (var v in va.TableDirect.InternalDictionary.Keys)
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

						case (Instruction.InstructionType.Length):
							va = Stack.Pop();
							switch (va.Type)
							{
								case (Value.ValueType.Array):
									Stack.Push(va.ArrayDirect.Count);
									break;
								case (Value.ValueType.Table):
									Stack.Push(va.TableDirect.Count);
									break;
								case (Value.ValueType.String):
									Stack.Push(va.StringDirect.Length);
									break;
								default:
									throw new CannotIndexTypeException(va.Type, instruction.Location);
							}
							break;

                        case (Instruction.InstructionType.StartIter):
                            va = Stack.Peek(0);
                            if (iterators == null)
                                iterators = new Dictionary<object, Tuple<int, Value[]>>();
                            switch (va.Type)
                            {
                                case (Value.ValueType.Array):
                                    iterators[va.ArrayDirect] = new Tuple<int, Value[]>(0, null);
                                    break;
                                case (Value.ValueType.Table):
                                    iterators[va.TableDirect] = new Tuple<int, Value[]>(0, va.TableDirect.Keys.ToArray());
                                    break;
                                case (Value.ValueType.String):
                                    iterators[va.StringDirect] = new Tuple<int, Value[]>(0, null);
                                    break;
                                default:
                                    throw new CannotIndexTypeException(va.Type, instruction.Location);
                            }
                            break;

                        case (Instruction.InstructionType.NextPair):
                            va = Stack.Peek(0);
                            switch (va.Type)
                            {
                                case (Value.ValueType.Array):
                                    var tup = iterators[va.ArrayDirect];
                                    ia = tup.Item1;
                                    if (ia < va.ArrayDirect.Count)
                                    {
                                        iterators[va.ArrayDirect] = new Tuple<int, Value[]>(ia + 1, null);
                                        Stack.Push(va.ArrayDirect[ia]);
                                        Stack.Push(ia);
                                    }
                                    Stack.Push(va.ArrayDirect.Count - ia);
                                    break;
                                case (Value.ValueType.Table):
                                    tup = iterators[va.TableDirect];
                                    ia = tup.Item1;
                                    if (ia < va.TableDirect.Count)
                                    {
                                        iterators[va.TableDirect] = new Tuple<int, Value[]>(ia + 1, iterators[va.TableDirect].Item2);
                                        var key = tup.Item2[ia];
                                        Stack.Push(va.TableDirect[key]);
                                        Stack.Push(key);
                                    }
                                    Stack.Push(va.TableDirect.Count - ia);
                                    break;
                                case (Value.ValueType.String):
                                    tup = iterators[va.StringDirect];
                                    ia = tup.Item1;
                                    if (ia < va.StringDirect.Length)
                                    {
                                        iterators[va.StringDirect] = new Tuple<int, Value[]>(ia + 1, null);
                                        Stack.Push(new string(va.StringDirect[ia], 1));
                                        Stack.Push(ia);
                                    }
                                    Stack.Push(va.StringDirect.Length - ia);
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

        public Value GetLocalVar(int ind, Function func, Value[] locals)
        {
            var level = ind / CodeGenerator.LocalsPerFunction - 1;
            ind = ind % CodeGenerator.LocalsPerFunction;

            if (level >= 0)
            {
                var llocals = func.Parents[level].LastRunLocals;
                if (llocals == null)
                    return Value.Nil;
                return llocals[ind];
            }
            return locals[ind];
        }

        public void SetLocalVar(int ind, Value value, Function func, Value[] locals)
        {
            var level = ind / CodeGenerator.LocalsPerFunction - 1;
            ind = ind % CodeGenerator.LocalsPerFunction;

            if (level >= 0)
            {
                var llocals = func.Parents[level].LastRunLocals;
                if (llocals == null)
                    return;
                llocals[ind] = value;
                return;
            }
            locals[ind] = value;
        }

        Function GetMethod(Value value, string methodName)
        {
            var dtype = value.Type;
            if (dtype == Value.ValueType.Table)
            {
                Value potentialMethod;
                if (value.TableDirect.TryGetValue(methodName, out potentialMethod) && potentialMethod.Type == Value.ValueType.Function)
                    return potentialMethod.FunctionDirect;
            }
            var type = value.ActualType;
            Dictionary<string, Function> typeMethods;
            if (type != null)
                if (Context.Methods.TryGetValue(type, out typeMethods))
                {
                    Function method;
                    if (typeMethods.TryGetValue(methodName, out method))
                    {
                        return method;
                    }
                }
            return null;
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

