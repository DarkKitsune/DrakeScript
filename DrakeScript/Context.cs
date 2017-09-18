﻿using System;
using System.Collections.Generic;

namespace DrakeScript
{
	public class Context
	{
		public Dictionary<string, Value> Globals = new Dictionary<string, Value>();

		public Context()
		{
			CoreLibs.Core.Register(this);
		}

		public Function LoadFile(string path)
		{
			return LoadString(System.IO.Path.GetFileName(path), System.IO.File.ReadAllText(path));
		}

		public Function LoadString(string code,
			[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",  
			[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
		)
		{
			return LoadString("codestring(" + System.IO.Path.GetFileName(sourceFilePath) + ":" + sourceLineNumber + ")", code);
		}
		public Function LoadString(string sourceName, string code)
		{
			var source = new Source(sourceName, code);
			var scanner = new Scanner();
			var tokens = scanner.Scan(source);
			var parser = new Parser();
			var tree = parser.Parse(tokens);
			var analyzer = new Analyzer();
			tree = analyzer.Analyze(tree);
			var generator = new CodeGenerator(this);
			return generator.Generate(tree);
		}

		public Value DoFile(string path)
		{
			var interpreter = new Interpreter(this);
			return LoadFile(path).Invoke(interpreter);
		}

		public Value DoString(string code,
			[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",  
			[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
		)
		{
			var interpreter = new Interpreter(this);
			return LoadString("codestring(" + System.IO.Path.GetFileName(sourceFilePath) + ":" + sourceLineNumber + ")", code).Invoke(interpreter);
		}
		public Value DoString(string sourceName, string code)
		{
			var interpreter = new Interpreter(this);
			return LoadString(sourceName, code).Invoke(interpreter);
		}


		public Value GetGlobal(string name)
		{
			return Globals[name];
		}
		public void SetGlobal(string name, byte value)
		{
			Globals[name] = Value.Create((double)value);
		}
		public void SetGlobal(string name, sbyte value)
		{
			Globals[name] = Value.Create((double)value);
		}
		public void SetGlobal(string name, short value)
		{
			Globals[name] = Value.Create((double)value);
		}
		public void SetGlobal(string name, int value)
		{
			Globals[name] = Value.Create((double)value);
		}
		public void SetGlobal(string name, long value)
		{
			Globals[name] = Value.Create((double)value);
		}
		public void SetGlobal(string name, ushort value)
		{
			Globals[name] = Value.Create((double)value);
		}
		public void SetGlobal(string name, uint value)
		{
			Globals[name] = Value.Create((double)value);
		}
		public void SetGlobal(string name, ulong value)
		{
			Globals[name] = Value.Create((double)value);
		}
		public void SetGlobal(string name, float value)
		{
			Globals[name] = Value.Create(value);
		}
		public void SetGlobal(string name, double value)
		{
			Globals[name] = Value.Create(value);
		}
		public void SetGlobal(string name, string value)
		{
			Globals[name] = Value.Create(value);
		}
		public void SetGlobal(string name, Func<Value[], int, Value> value)
		{
			Globals[name] = Value.Create(new Function(this, value));
		}
	}
}

