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
		public String[] Locals {get; private set;}
		public Func<List<Value>, Value> Method;

		public Function(Instruction[] code, String[] args, String[] locals)
		{
			ScriptFunction = true;
			Code = code;
			Args = args;
			Locals = locals;
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
			Locals = new string[] {};
		}

		public Value Invoke(Interpreter interpreter)
		{
			if (ScriptFunction)
			{
				interpreter.Interpret(this);
				return interpreter.Stack.Pop();
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

