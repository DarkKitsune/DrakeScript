using System;
using System.Collections.Generic;

namespace DrakeScript
{
	public class Context
	{
		public Table Globals = new Table();
		internal Dictionary<Type, Func<System.IO.BinaryReader, Value>> FromBytesConv = new Dictionary<Type, Func<System.IO.BinaryReader, Value>>();
		internal Dictionary<Type, Action<System.IO.BinaryWriter, Value>> ToBytesConv = new Dictionary<Type, Action<System.IO.BinaryWriter, Value>>();
		internal Dictionary<int, Type> TypeFromID = new Dictionary<int, Type>();
		internal Dictionary<Type, int> IDFromType = new Dictionary<Type, int>();
		int NextTypeID;


		public Context()
		{
			CoreLibs.LibCore.Register(this);
			CoreLibs.LibCoroutine.Register(this);
			CoreLibs.LibArray.Register(this);
			CoreLibs.LibTable.Register(this);
			CoreLibs.LibMath.Register(this);
		}

		public void SetBinaryConversionMethods(Type type, Func<System.IO.BinaryReader, Value> fromBin, Action<System.IO.BinaryWriter, Value> toBin)
		{
			FromBytesConv[type] = fromBin;
			ToBytesConv[type] = toBin;
			var id = NextTypeID;
			NextTypeID++;
			TypeFromID[id] = type;
			IDFromType[type] = id;
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
			try
			{
				return generator.Generate(new SourceRef(source, -1, 0), tree);
			}
			catch (Exception e)
			{
				throw new Exception(e.ToString() + "\n===Code===\n" + tree.ToString());
			}
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

		public Function LoadBytecode(string path)
		{
			using (var reader = new System.IO.BinaryReader(System.IO.File.OpenRead(path)))
			{
				var vMajor = reader.ReadInt32();
				var vMinor = reader.ReadInt32();
				var vBuild = reader.ReadInt32();
				return Function.FromReader(this, reader);
			}
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
		public void SetGlobal(string name, Function func)
		{
			Globals[name] = Value.Create(func);
		}
		public void SetGlobal(string name, List<Value> value)
		{
			Globals[name] = Value.Create(value);
		}
		public void SetGlobal(string name, Table value)
		{
			Globals[name] = Value.Create(value);
		}
		public void SetGlobal(string name, Object value)
		{
			Globals[name] = Value.Create(value);
		}


		public Coroutine CreateCoroutine(Function func)
		{
			return new Coroutine(this, func);
		}

		public Function CreateFunction(Func<Interpreter, SourceRef, Value[], int, Value> method, int minimumParamCount)
		{
			return new Function(this, method, minimumParamCount);
		}
	}
}

