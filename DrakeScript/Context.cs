using System;
using System.Collections.Generic;

namespace DrakeScript
{
	public class Context
	{
		public Table Globals = new Table();
		public Dictionary<Type, Dictionary<string, Function>> Methods = new Dictionary<Type, Dictionary<string, Function>>();
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
			if (name == null)
				throw new ArgumentNullException();
			var path = name.Split('.');
			var table = Globals;
			for (var i = 0; i < path.Length - 1; i++)
			{
				var part = path[i];
				Value tval;
				if (!table.TryGetValue(part, out tval))
					throw new KeyNotFoundException("key \"" + part + "\" in global path \"" + name + "\" does not exist");
				if (tval.Type != Value.ValueType.Table)
					throw new Exception("key \"" + part + "\" in global path \"" + name + "\" is not a table");
				table = tval.TableDirect;
			}
			return table[path[path.Length - 1]];
		}
		public void SetGlobal(string name, Value value)
		{
			if (name == null)
				throw new ArgumentNullException();
			var path = name.Split('.');
			var table = Globals;
			for (var i = 0; i < path.Length - 1; i++)
			{
				var part = path[i];
				Value tval;
				if (!table.TryGetValue(part, out tval))
				{
					table[part] = table = new Table();
					continue;
				}
				if (tval.Type != Value.ValueType.Table)
					throw new Exception("key \"" + part + "\" in global path \"" + name + "\" is not a table");
				table = tval.TableDirect;
			}
			table[path[path.Length - 1]] = value;
		}
		public void SetGlobal(string name, byte value)
		{
			SetGlobal(name, Value.Create((double)value));
		}
		public void SetGlobal(string name, sbyte value)
		{
			SetGlobal(name, Value.Create((double)value));
		}
		public void SetGlobal(string name, short value)
		{
			SetGlobal(name, Value.Create((double)value));
		}
		public void SetGlobal(string name, int value)
		{
			SetGlobal(name, Value.Create((double)value));
		}
		public void SetGlobal(string name, long value)
		{
			SetGlobal(name, Value.Create((double)value));
		}
		public void SetGlobal(string name, ushort value)
		{
			SetGlobal(name, Value.Create((double)value));
		}
		public void SetGlobal(string name, uint value)
		{
			SetGlobal(name, Value.Create((double)value));
		}
		public void SetGlobal(string name, ulong value)
		{
			SetGlobal(name, Value.Create((double)value));
		}
		public void SetGlobal(string name, float value)
		{
			SetGlobal(name, Value.Create(value));
		}
		public void SetGlobal(string name, double value)
		{
			SetGlobal(name, Value.Create(value));
		}
		public void SetGlobal(string name, string value)
		{
			SetGlobal(name, Value.Create(value));
		}
		public void SetGlobal(string name, Function func)
		{
			SetGlobal(name, Value.Create(func));
		}
		public void SetGlobal(string name, List<Value> value)
		{
			SetGlobal(name, Value.Create(value));
		}
		public void SetGlobal(string name, Table value)
		{
			SetGlobal(name, Value.Create(value));
		}
		public void SetGlobal(string name, Object value)
		{
			SetGlobal(name, Value.Create(value));
		}

		public Function GetMethod(Type type, string name)
		{
			Dictionary<string, Function> mtable;
			if (!Methods.TryGetValue(type, out mtable))
				return null;
			Function ret;
			if (!mtable.TryGetValue(name, out ret))
				return null;
			return ret;
		}
		public void AddMethod(Type type, string name, Function func)
		{
			Dictionary<string, Function> mtable;
			if (!Methods.TryGetValue(type, out mtable))
			{
				Methods.Add(type, mtable = new Dictionary<string, Function>());
			}
			mtable[name] = func;
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

