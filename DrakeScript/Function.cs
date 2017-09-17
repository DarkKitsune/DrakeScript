using System;
using System.Reflection;
using System.Collections.Generic;

namespace DrakeScript
{
	public class Function
	{
		public bool ScriptFunction {get; private set;}
		public Instruction[] Code {get; internal set;}
		public String[] Args {get; private set;}
		public Func<List<Value>, Value> Method;

		public Function(Instruction[] code, String[] args)
		{
			ScriptFunction = true;
			Code = code;
			Args = args;
		}

		public Function(Func<List<Value>, Value> method)
		{
			Method = method;
			var param = method.GetMethodInfo().GetParameters();
			Args = new string[param.Length];
			var n = 0;
			foreach (var p in param)
			{
				Args[n++] = p.Name;
			}
		}

		public Value Invoke(Interpreter interpreter)
		{
			if (ScriptFunction)
			{
				interpreter.Interpret(this);
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

