﻿using System;
using System.IO;
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
		public Func<Context, SourceRef, Value[], int, Value> Method;
		public Context Context;
		public Version Version;
		public string File;

		public Function(string file, Context context, Instruction[] code, String[] args, String[] locals)
		{
			File = file;
			Version = typeof(Context).Assembly.GetName().Version;
			Context = context;
			ScriptFunction = true;
			Code = code;
			Args = args;
			Locals = locals;
		}

		public Function(Context context, Func<Context, SourceRef, Value[], int, Value> method, int paramCount)
		{
			File = method.ToString();
			Context = context;
			Method = method;
			Args = new string[paramCount];
			Locals = new string[] {};
		}

		public Function(string file, Context context, Func<Context, SourceRef, Value[], int, Value> method, int paramCount)
		{
			File = file;
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
				return Method(Context, interpreter.CallLocation, interpreter.ArgList, interpreter.ArgListCount);
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
				if (interpreter.ArgListCount < Args.Length)
					throw new NotEnoughArgumentsException(Args.Length, interpreter.ArgListCount, interpreter.CallLocation);
				interpreter.Stack.Push(Method(Context, interpreter.CallLocation, interpreter.ArgList, interpreter.ArgListCount));
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
				return Method(Context, interpreter.CallLocation, args, args.Length);
			}
		}
		public Value Invoke(
			[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",  
			[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0,
			params Value[] args
		)
		{
			var location = new SourceRef(new Source(sourceFilePath, ""), sourceLineNumber, 0);
			if (ScriptFunction)
			{
				var interpreter = new Interpreter(Context);
				interpreter.ArgList = args;
				interpreter.ArgListCount = args.Length;
				interpreter.CallLocation = location;
				interpreter.Interpret(this);
				return interpreter.Stack.Pop();
			}
			else
			{
				if (args.Length < Args.Length)
					throw new NotEnoughArgumentsException(Args.Length, args.Length, location);
				return Method(Context, location, args, args.Length);
			}
		}


		public byte[] GetBytecode()
		{
			using (var memoryStream = new MemoryStream())
			{
				using (var writer = new BinaryWriter(memoryStream))
				{
					var version = typeof(Function).Assembly.GetName().Version;
					writer.Write(version.Major);
					writer.Write(version.Minor);
					writer.Write(version.Build);
					writer.Write(File.Length);
					writer.Write(System.Text.Encoding.ASCII.GetBytes(File));
					writer.Write(Args.Length);
					writer.Write(Locals.Length);
					writer.Write(Code.Length);	
					foreach (var inst in Code)
					{
						writer.Write(inst.GetBytes());
					}
				}
				return memoryStream.ToArray();
			}
		}

		public static Function FromBytes(Context context, byte[] bytes)
		{
			using (var memoryStream = new MemoryStream(bytes))
			{
				using (var reader = new BinaryReader(memoryStream))
				{
					return FromReader(context, reader);
				}
			}
		}

		internal static Function FromReader(Context context, BinaryReader reader)
		{
			var vMajor = reader.ReadInt32();
			var vMinor = reader.ReadInt32();
			var vBuild = reader.ReadInt32();
			var filenameLength = reader.ReadInt32();
			var filename = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(filenameLength));
			var dummySource = new Source(filename, "");
			var args = new string[reader.ReadInt32()];
			var locals = new string[reader.ReadInt32()];
			var code = new Instruction[reader.ReadInt32()];
			for (var i = 0; i < code.Length; i++)
			{
				code[i] = Instruction.FromReader(context, reader, dummySource);
			}
			return new Function(filename, context, code, args, locals);
		}
	}
}

