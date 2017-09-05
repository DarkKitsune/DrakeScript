using System;
using System.Collections.Generic;

namespace DrakeScript
{
	public class Function
	{
		public bool ScriptFunction {get; private set;}
		public Instruction[] Code {get; private set;}
		public Func<List<Value>, Value> Method;

		public Function(Instruction[] code)
		{
			ScriptFunction = true;
			Code = code;
		}

		public Function(Func<List<Value>, Value> method)
		{
			Method = method;
		}

		public Value Invoke(Interpreter interpreter)
		{
			if (ScriptFunction)
			{
				interpreter.Interpret(Code);
				if (interpreter.Stack.Count > 0)
					return interpreter.Stack.Peek(0);
				return Value.Nil;
			}
			else
			{
				return Method(interpreter.ArgList);
			}
		}
		public Value Invoke(List<Value> args)
		{
			if (ScriptFunction)
			{
				throw new NotSupportedException("Script function cannot be invoked with this method");
			}
			else
			{
				return Method(args);
			}
		}
	}
}

