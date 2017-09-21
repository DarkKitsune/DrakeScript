using System;
using System.IO;
using System.Collections.Generic;

namespace DrakeScript
{
	public struct Value
	{
		public const int MinSize = sizeof(ValueType) + sizeof(double) + 4;

		static string DefaultString = "";
		public static Value Nil = new Value {Type = ValueType.Nil, FloatNumber = 0.0, String = DefaultString, Reference = null};

		public enum ValueType : short
		{
			Nil,
			Float,
			String,
			Function,
			Array,
			Int,
			Table
		}

		public ValueType Type;
		public double FloatNumber;
		/*public double Number
		{
			get
			{
				if (Type == ValueType.Number)
					return FloatNumber;
				return (double)IntNumber;
			}
			set
			{
				if (Type == ValueType.Number)
					FloatNumber = value;
				IntNumber = (int)value;
			}
		}*/
		public int IntNumber;
		object Reference;

		public bool Bool
		{
			get
			{
				return FloatNumber != 0.0;
			}
			set
			{
				FloatNumber = (value ? 1.0 : 0.0);
			}
		}

		public string String
		{
			get
			{
				if (Type == ValueType.String)
					return (string)Reference;
				return DefaultString;
			}
			set
			{
				Reference = value;
			}
		}

		public Function Function
		{
			get
			{
				if (Reference is Function)
					return (Function)Reference;
				return null;
			}
		}

		public List<Value> Array
		{
			get
			{
				if (Reference is List<Value>)
					return (List<Value>)Reference;
				return null;
			}
		}

		public Table Table
		{
			get
			{
				if (Reference is Table)
					return (Table)Reference;
				return null;
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
					case (ValueType.Float):
						return FloatNumber;
					case (ValueType.Int):
						return IntNumber;
					case (ValueType.String):
						return String;
					default:
						return Reference;
				}
			}
		}

		public bool IsNil
		{
			get
			{
				return Type == ValueType.Nil;
			}
		}

		public bool IsValid
		{
			get
			{
				return String != null;
			}
		}

		public static Value CreateInt(int v)
		{
			var val = new Value();
			val.Type = ValueType.Int;
			val.FloatNumber = 0.0;
			val.IntNumber = v;
			val.Reference = null;
			return val;
		}

		public static Value Create(double v)
		{
			var val = new Value();
			val.Type = ValueType.Float;
			val.FloatNumber = v;
			val.Reference = null;
			return val;
		}
		public static implicit operator double(Value v)
		{
			return v.FloatNumber;
		}
		public static implicit operator Value(double v)
		{
			return Value.Create(v);
		}

		public static Value Create(int v)
		{
			var val = new Value();
			val.Type = ValueType.Float;
			val.FloatNumber = v;
			val.Reference = null;
			return val;
		}
		public static implicit operator int(Value v)
		{
			return (int)v.FloatNumber;
		}
		public static implicit operator Value(int v)
		{
			return Value.Create(v);
		}

		public static Value Create(string v)
		{
			var val = new Value();
			val.Type = ValueType.String;
			val.FloatNumber = 0.0;
			val.Reference = v;
			return val;
		}
		public static implicit operator string(Value v)
		{
			if (v.Type == ValueType.String)
				return v.String;
			return v.ToString();
		}
		public static implicit operator Value(string v)
		{
			return Value.Create(v);
		}

		public static Value Create(bool v)
		{
			var val = new Value();
			val.Type = ValueType.Float;
			val.FloatNumber = (v ? 1.0 : 0.0);
			val.Reference = null;
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
			val.FloatNumber = 0.0;
			val.Reference = v;
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

		public static Value Create(List<Value> v)
		{
			var val = new Value();
			val.Type = ValueType.Array;
			val.FloatNumber = 0.0;
			val.Reference = v;
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
			val.FloatNumber = 0.0;
			val.Reference = v;
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

		public static Value Create()
		{
			var val = new Value();
			val.Type = ValueType.Float;
			val.FloatNumber = 0.0;
			val.Reference = null;
			return val;
		}


		public override string ToString()
		{
			switch (Type)
			{
				case (ValueType.Nil):
					return "nil";
			}
			return (DynamicValue != null ? DynamicValue.ToString() : "nil");
		}

		public bool Equals(Value value)
		{
			var dyn = DynamicValue;
			if (dyn == null)
			{
				if (value.DynamicValue == null)
					return true;
				return false;
			}
			return dyn.Equals(value.DynamicValue);
		}

		public override bool Equals(object obj)
		{
			return DynamicValue.Equals(obj);
		}

		public override int GetHashCode()
		{
			return DynamicValue.GetHashCode();
		}


		public byte[] GetBytes()
		{
			using (var memoryStream = new MemoryStream())
			{
				using (var writer = new BinaryWriter(memoryStream))
				{
					writer.Write((short)Type);
					switch (Type)
					{
						case (ValueType.String):
							writer.Write(String.Length);
							writer.Write(System.Text.Encoding.ASCII.GetBytes(String));
							break;
						case (ValueType.Function):
							writer.Write(Function.GetBytecode());
							break;
						case (ValueType.Float):
							writer.Write((float)FloatNumber);
							break;
						case (ValueType.Int):
							writer.Write(IntNumber);
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
				case (ValueType.Float):
					return Value.Create((double)reader.ReadSingle());
				case (ValueType.Int):
					return Value.Create((double)reader.ReadInt32());
				default:
					return Value.Nil;
			}
		}
	}
}

