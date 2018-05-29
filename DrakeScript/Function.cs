using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace DrakeScript
{
	public class Function
	{
		public bool ScriptFunction {get; private set;}
		public Instruction[] Code {get; internal set;}
		public String[] Args {get; internal set;}
		public String[] Locals {get; internal set;}
		public Func<Interpreter, SourceRef, Value[], int, Value> Method;
		public Context Context;
		public Version Version;
		public SourceRef Location;
        public Function ParentFunction;


        public Function(SourceRef location, Context context, String[] args, Function parentFunc)
		{
			Location = location;
			Version = typeof(Context).Assembly.GetName().Version;
			Context = context;
			ScriptFunction = true;
			Args = args;
            ParentFunction = parentFunc;
		}

		public Function(Context context, Func<Interpreter, SourceRef, Value[], int, Value> method, int paramCount)
		{
			Location = new SourceRef(new Source(method.ToString(), ""), 0, 0);
			Context = context;
			Method = method;
			Args = new string[paramCount];
			Locals = new string[] {};
		}

		public Function(SourceRef location, Context context, Func<Interpreter, SourceRef, Value[], int, Value> method, int paramCount)
		{
			Location = location;
			Context = context;
			Method = method;
			Args = new string[paramCount];
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
				if (interpreter.ArgListCount < Args.Length)
					throw new NotEnoughArgumentsException(Args.Length, interpreter.ArgListCount, interpreter.CallLocation);
				return Method(interpreter, interpreter.CallLocation, interpreter.ArgList, interpreter.ArgListCount);
			}
		}
		internal void InvokePushInsteadOfReturn(Interpreter interpreter, Value[] parentLocals)
		{
			if (ScriptFunction)
			{
				interpreter.Interpret(this, parentLocals);
			}
			else
			{
				//Console.WriteLine("Calling " + Method.GetMethodInfo().Name + " with args " + String.Join(", ", interpreter.ArgList));
				if (interpreter.ArgListCount < Args.Length)
					throw new NotEnoughArgumentsException(Args.Length, interpreter.ArgListCount, interpreter.CallLocation);
				interpreter.Stack.Push(Method(interpreter, interpreter.CallLocation, interpreter.ArgList, interpreter.ArgListCount));
			}
		}
		public Value Invoke(Interpreter interpreter, params Value[] args)
		{
			if (ScriptFunction)
			{
				if (!interpreter.PauseStatus.Paused)
				{
					interpreter.ArgList = args;
					interpreter.ArgListCount = args.Length;
				}
				interpreter.Interpret(this);
				return interpreter.Stack.Pop();
			}
			else
			{
				if (args.Length < Args.Length)
					throw new NotEnoughArgumentsException(Args.Length, args.Length, interpreter.CallLocation);
				return Method(interpreter, interpreter.CallLocation, args, args.Length);
			}
		}
		public Value Invoke(
			params Value[] args
		)
		{
			var location = SourceRef.Invalid;//new SourceRef(new Source(sourceFilePath, ""), sourceLineNumber, 0);
			var interpreter = new Interpreter(Context);
			interpreter.ArgList = args;
			interpreter.ArgListCount = args.Length;

			if (ScriptFunction)
			{
				interpreter.CallLocation = location;
				interpreter.Interpret(this);
				return interpreter.Stack.Pop();
			}
			else
			{
				if (args.Length < Args.Length)
					throw new NotEnoughArgumentsException(Args.Length, args.Length, location);
				return Method(interpreter, location, args, args.Length);
			}
		}
		public Value Invoke(
			Dictionary<string, Value> dynamicConstants,
			params Value[] args
		)
		{
			var location = SourceRef.Invalid;//new SourceRef(new Source(sourceFilePath, ""), sourceLineNumber, 0);
			var interpreter = new Interpreter(Context);
			interpreter.ArgList = args;
			interpreter.ArgListCount = args.Length;
			if (ScriptFunction)
			{
				interpreter.CallLocation = location;
				interpreter.DynamicLocalConstants = dynamicConstants;
				interpreter.Interpret(this);
				return interpreter.Stack.Pop();
			}
			else
			{
				if (args.Length < Args.Length)
					throw new NotEnoughArgumentsException(Args.Length, args.Length, location);
				return Method(interpreter, location, args, args.Length);
			}
		}


		public byte[] GetBytecode()
		{
			using (var memoryStream = new MemoryStream())
			{
				using (var writer = new BinaryWriter(memoryStream))
				{
					writer.Write(Location.GetBytes());
					writer.Write(Args.Length);
					writer.Write(Locals.Length);
					writer.Write(Code.Length);
					foreach (var inst in Code)
					{
						writer.Write(inst.GetBytes(Context));
					}
				}
				return memoryStream.ToArray();
			}
		}

		public void WriteByteCodeFile(BinaryWriter writer)
		{
			var version = typeof(Function).Assembly.GetName().Version;
			writer.Write(version.Major);
			writer.Write(version.Minor);
			writer.Write(version.Build);
			writer.Write(GetBytecode());
		}

		internal static Function FromReader(Context context, BinaryReader reader)
		{
			var location = SourceRef.FromReader(reader);
			var args = new string[reader.ReadInt32()];
			var locals = new string[reader.ReadInt32()];
			var code = new Instruction[reader.ReadInt32()];
			for (var i = 0; i < code.Length; i++)
			{
				code[i] = Instruction.FromReader(context, reader, location.Source);
			}
			var func = new Function(location, context, args, null);
            func.Code = code;
            func.Locals = locals;
            return func;
		}

		public override string ToString()
		{
			return string.Format("Function at {0}", Location.ToString());
		}
	}
}

