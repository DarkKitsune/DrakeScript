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
		public Func<Value[], int, Value> Method;

		public Function(Instruction[] code, String[] args, String[] locals)
		{
			ScriptFunction = true;
			Code = code;
			Args = args;
			Locals = locals;
		}

		public Function(Func<Value[], int, Value> method)
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
				return Method(interpreter.ArgList, interpreter.ArgListCount);
			}
		}
		internal void InvokePushInsteadOfReturn(Interpreter interpreter)
		{
			if (ScriptFunction)
			{
				interpreter.Interpret(this);
			}
			else
			{
				interpreter.Stack.Push(Method(interpreter.ArgList, interpreter.ArgListCount));
			}
		}
		public Value Invoke(params Value[] args)
		{
			if (ScriptFunction)
			{
				throw new NotSupportedException("Script function cannot be invoked with this method");
			}
			else
			{
				return Method(args, args.Length);
			}
		}
	}
}

