﻿using System;
using System.IO;
using System.Collections.Generic;

namespace DrakeScript
{
	public struct Value
	{
		public const int MinSize = sizeof(ValueType) + sizeof(double) + sizeof(int) + 4;

		static string DefaultString = "";
		public static Value Nil = new Value {Type = ValueType.Nil, Number = 0.0, String = DefaultString, Object = null};
		public static Value Zero = new Value {Type = ValueType.Number, Number = 0.0, String = DefaultString, Object = null};
		public static Value One = new Value {Type = ValueType.Number, Number = 1.0, String = DefaultString, Object = null};

		public enum ValueType : short
		{
			Nil = 0,
			Number = 1,
			String = 2,
			Function = 3,
			Array = 4,
			Int = 5,
			Table = 6,
			Coroutine = 7,
			Object = 8
		}

		public ValueType Type;
		public double Number;
		public int IntNumber;
		public object Object;

		public bool Bool
		{
			get
			{
				return Number != 0.0;
			}
			set
			{
				Number = (value ? 1.0 : 0.0);
			}
		}

		public string String
		{
			get
			{
				if (Type == ValueType.String)
					return (string)Object;
				return DefaultString;
			}
			set
			{
				Object = value;
			}
		}

		public string StringDirect
		{
			get
			{
				return (string)Object;
			}
			set
			{
				Object = value;
			}
		}

		public Function Function
		{
			get
			{
				if (Object is Function)
					return (Function)Object;
				return null;
			}
			set
			{
				Object = value;
			}
		}

        public Function FunctionDirect
        {
            get
            {
                return (Function)Object;
            }
            set
            {
                Object = value;
            }
        }

        public Coroutine Coroutine
		{
			get
			{
				if (Object is Coroutine)
					return (Coroutine)Object;
				return null;
			}
			set
			{
				Object = value;
			}
		}

        public Coroutine CoroutineDirect
        {
            get
            {
                return (Coroutine)Object;
            }
            set
            {
                Object = value;
            }
        }

        public List<Value> Array
		{
			get
			{
				if (Object is List<Value>)
					return (List<Value>)Object;
				return null;
			}
			set
			{
				Object = value;
			}
		}

        public List<Value> ArrayDirect
        {
            get
            {
                return (List<Value>)Object;
            }
            set
            {
                Object = value;
            }
        }

        public Table Table
		{
			get
			{
				if (Object is Table)
					return (Table)Object;
				return null;
			}
			set
			{
				Object = value;
			}
		}

        public Table TableDirect
        {
            get
            {
                return (Table)Object;
            }
            set
            {
                Object = value;
            }
        }

        public object DynamicValue
		{
			get
			{
				switch (Type)
				{
					case (ValueType.Nil):
						return null;
					case (ValueType.Number):
						return Number;
					case (ValueType.Int):
						return IntNumber;
					case (ValueType.String):
						return StringDirect;
					default:
						return Object;
				}
			}
		}

		public bool Is<T>()
		{
			return Object is T;
		}

		public bool Is(ValueType t)
		{
			return Type == t;
		}

		public T ObjectAs<T>()
		{
			if (Object is T)
				return (T)Object;
			return default(T);
		}

		public bool IsNil
		{
			get
			{
				return Type == ValueType.Nil;
			}
		}

		public Type ActualType
		{
			get
			{
				switch (Type)
				{
					case (ValueType.Array):
						return typeof(List<Value>);
					case (ValueType.Coroutine):
						return typeof(Coroutine);
					case (ValueType.Function):
						return typeof(Function);
					case (ValueType.Int):
					case (ValueType.Number):
						return typeof(double);
					case (ValueType.Nil):
						return null;
					case (ValueType.Object):
						return Object.GetType();
					case (ValueType.String):
						return typeof(string);
					case (ValueType.Table):
						return typeof(Table);
					default:
						return DynamicValue.GetType();
				}
			}
		}

		public static Value CreateInt(int v)
		{
			var val = new Value();
			val.Type = ValueType.Int;
			val.IntNumber = v;
			val.Object = null;
			return val;
		}

		public static Value Create(double v)
		{
			var val = new Value();
			val.Type = ValueType.Number;
			val.Number = v;
			val.Object = null;
			return val;
		}
		public static implicit operator double(Value v)
		{
			return v.Number;
		}
		public static implicit operator Value(double v)
		{
			return Value.Create(v);
		}

		public static Value Create(int v)
		{
			var val = new Value();
			val.Type = ValueType.Number;
			val.Number = v;
			val.Object = null;
			return val;
		}
		public static implicit operator int(Value v)
		{
			return (int)v.Number;
		}
		public static implicit operator Value(int v)
		{
			return Value.Create(v);
		}

		public static Value Create(string v)
		{
			var val = new Value();
			val.Type = ValueType.String;
			val.Object = v;
			return val;
		}
		public static implicit operator string(Value v)
		{
			if (v.Type == ValueType.String)
				return v.StringDirect;
			return v.ToString();
		}
		public static implicit operator Value(string v)
		{
			return Value.Create(v);
		}
		public static implicit operator Value(char v)
		{
			return Value.Create(new String(v, 1));
		}

		public static Value Create(bool v)
		{
			var val = new Value();
			val.Type = ValueType.Number;
			val.Number = (v ? 1.0 : 0.0);
			val.Object = null;
			return val;
		}
		public static implicit operator bool(Value v)
		{
			return v.Bool;
		}
		public static implicit operator Value(bool v)
		{
			return Value.Create(v);
		}

		public static Value Create(Function v)
		{
			var val = new Value();
			val.Type = ValueType.Function;
			val.Object = v;
			return val;
		}
		public static implicit operator Function(Value v)
		{
			return v.Function;
		}
		public static implicit operator Value(Function v)
		{
			return Value.Create(v);
		}

		public static Value Create(Coroutine v)
		{
			var val = new Value();
			val.Type = ValueType.Coroutine;
			val.Object = v;
			return val;
		}
		public static implicit operator Coroutine(Value v)
		{
			return v.Coroutine;
		}
		public static implicit operator Value(Coroutine v)
		{
			return Value.Create(v);
		}

		public static Value Create(List<Value> v)
		{
			var val = new Value();
			val.Type = ValueType.Array;
			val.Object = v;
			return val;
		}
		public static implicit operator List<Value>(Value v)
		{
			return v.Array;
		}
		public static implicit operator Value(List<Value> v)
		{
			return Value.Create(v);
		}

		public static Value Create(Table v)
		{
			var val = new Value();
			val.Type = ValueType.Table;
			val.Object = v;
			return val;
		}
		public static implicit operator Table(Value v)
		{
			return v.Table;
		}
		public static implicit operator Value(Table v)
		{
			return Value.Create(v);
		}

		public static Value Create(object obj)
		{
			var val = new Value();
			val.Type = ValueType.Object;
			val.Object = obj;
			return val;
		}

		public static Value Create()
		{
			var val = new Value();
			val.Type = ValueType.Number;
			val.Number = 0.0;
			val.Object = null;
			return val;
		}


		public Value VerifyType(
			ValueType type,
			[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",  
			[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
		)
		{
			if (Type != type)
				throw new UnexpectedTypeException(Type, type, new SourceRef(new Source(sourceFilePath, ""), sourceLineNumber, 0));
			return this;
		}

		public Value VerifyType(ValueType type, SourceRef location)
		{
			if (Type != type)
				throw new UnexpectedTypeException(Type, type, location);
			return this;
		}

		public Value VerifyType<T>(
			[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",  
			[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
		)
		{
			if (Object == null || !(Object is T))
				throw new UnexpectedTypeException(
					DynamicValue?.GetType(),
					typeof(T),
					new SourceRef(
						new Source(sourceFilePath, ""),
						sourceLineNumber,
						0
					)
				);
			return this;
		}

		public Value VerifyType<T>(SourceRef location)
		{
			if (Object == null || !(Object is T))
				throw new UnexpectedTypeException(DynamicValue?.GetType(), typeof(T), location);
			return this;
		}

        public bool BoolOr(bool alt)
        {
            if (Type == ValueType.Number)
                return Bool;
            return alt;
        }
        public double NumberOr(double alt)
        {
            if (Type == ValueType.Number)
                return Number;
            return alt;
        }
        public string StringOr(string alt)
        {
            if (Type == ValueType.String)
                return StringDirect;
            return alt;
        }
        public Function FunctionOr(Function alt)
        {
            if (Type == ValueType.Function)
                return FunctionDirect;
            return alt;
        }
        public Coroutine CoroutineOr(Coroutine alt)
        {
            if (Type == ValueType.Coroutine)
                return CoroutineDirect;
            return alt;
        }
        public List<Value> ArrayOr(List<Value> alt)
        {
            if (Type == ValueType.Array)
                return ArrayDirect;
            return alt;
        }
        public Table TableOr(Table alt)
        {
            if (Type == ValueType.Table)
                return TableDirect;
            return alt;
        }
        public object ObjectOr(object alt)
        {
            if (Type == ValueType.Object)
                return Object;
            return alt;
        }
        public T ObjectOr<T>(T alt)
        {
            if (Type == ValueType.Object && Object is T)
                return (T)Object;
            return alt;
        }

        public override string ToString()
		{
			switch (Type)
			{
				case (ValueType.Nil):
					return "nil";
				case (ValueType.Array):
					return String.Format("[{0}]", String.Join(", ", ArrayDirect));
			}
			return (DynamicValue != null ? DynamicValue.ToString() : "nil");
		}

		public bool Equals(Value value)
		{
			if (value.Type != Type)
				return false;
			switch (Type)
			{
				case (ValueType.Nil):
					return true;
				case (ValueType.Number):
					return Number == value.Number;
				case (ValueType.String):
					return StringDirect == value.StringDirect;
				default:
					var dyn = DynamicValue;
					if (dyn == null)
					{
						if (value.DynamicValue == null)
							return true;
						return false;
					}
					return dyn.Equals(value.DynamicValue);
			}
		}

		public override bool Equals(object obj)
		{
			return DynamicValue.Equals(obj);
		}

		public override int GetHashCode()
		{
			return DynamicValue.GetHashCode();
		}


		public byte[] GetBytes(Context context)
		{
			using (var memoryStream = new MemoryStream())
			{
				using (var writer = new BinaryWriter(memoryStream))
				{
					writer.Write((short)Type);
					switch (Type)
					{
						case (ValueType.String):
							writer.Write(StringDirect.Length);
							writer.Write(System.Text.Encoding.ASCII.GetBytes(StringDirect));
							break;
						case (ValueType.Function):
							writer.Write(FunctionDirect.GetBytecode());
							break;
						case (ValueType.Number):
							writer.Write(Number);
							break;
						case (ValueType.Int):
							writer.Write(IntNumber);
							break;
                        case (ValueType.Array):
                            writer.Write(ArrayDirect.Count);
                            foreach (var e in ArrayDirect)
                                writer.Write(e.GetBytes(context));
                            break;
						case (ValueType.Object):
							var otype = Object.GetType();
							int id;
							if (!context.IDFromType.TryGetValue(otype, out id))
							{
								writer.Write(-1);
								break;
							}
							writer.Write(id);
							context.ToBytesConv[otype].Invoke(writer, this);
							break;
					}
				}
				return memoryStream.ToArray();
			}
		}

		public static Value FromReader(Context context, BinaryReader reader)
		{
			var type = (ValueType)reader.ReadInt16();
			switch (type)
			{
				case (ValueType.String):
					var strLength = reader.ReadInt32();
					var str = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(strLength));
					return Value.Create(str);
				case (ValueType.Function):
					return Value.Create(Function.FromReader(context, reader));
				case (ValueType.Number):
					return Value.Create(reader.ReadDouble());
				case (ValueType.Int):
					return Value.CreateInt(reader.ReadInt32());
                case (ValueType.Array):
                    var count = reader.ReadInt32();
                    var array = new List<Value>(count);
                    for (var i = 0; i < count; i++)
                        array.Add(Value.FromReader(context, reader));
                    return Value.Create(array);
				case (ValueType.Object):
					var id = reader.ReadInt32();
					Type otype;
					if (!context.TypeFromID.TryGetValue(id, out otype))
						return Value.Nil;
					return context.FromBytesConv[otype].Invoke(reader);
				default:
					return Value.Nil;
			}
		}
	}
}

