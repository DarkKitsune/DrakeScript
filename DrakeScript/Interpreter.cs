using System;
using System.Collections.Generic;
using PowerDS;

namespace DrakeScript
{
	public class Interpreter
	{
		public Context Context;
		public FastStackGrowable<Scope> ScopeStack = new FastStackGrowable<Scope>(4096);
		public FastStackGrowable<Value> Stack = new FastStackGrowable<Value>(1024);
		public List<Value> ArgList = new List<Value>(128);

		public Interpreter(Context context)
		{
			Context = context;
		}

		public void Interpret(Instruction[] code)
		{
			ScopeStack.Push(Scope.Create());

			int ia, ib;

			for (var pos = 0; pos < code.Length; pos++)
			{
				var instruction = code[pos];
				switch (instruction.Type)
				{
					case (Instruction.InstructionType.NewLoc):
						ScopeStack.Peek(0).Locals[instruction.Arg.String] = Value.Nil;
						break;
					case (Instruction.InstructionType.PushVar):
						Stack.Push(GetVar(instruction.Arg.String));
						break;
					case (Instruction.InstructionType.PushNum):
					case (Instruction.InstructionType.PushStr):
						Stack.Push(instruction.Arg);
						break;
					case (Instruction.InstructionType.PopVar):
						SetVar(instruction.Arg.String, Stack.Pop());
						break;
					case (Instruction.InstructionType.PopArgs):
						ArgList.Clear();
						ia = (int)instruction.Arg.Number;
						for (var i = 0; i < ia; i++)
						{
							ArgList.Add(Stack.Pop());
						}
						break;
					case (Instruction.InstructionType.Call):
						Stack.Pop().Function.Invoke(this);
						break;
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
	}
}

