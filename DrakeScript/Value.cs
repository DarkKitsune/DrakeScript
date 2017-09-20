using System;
using System.Collections.Generic;

namespace DrakeScript
{
	public struct Value
	{
		public const int MinSize = sizeof(ValueType) + sizeof(double) + 4;

		static string DefaultString = "";
		public static Value Nil = new Value {Type = ValueType.Nil, Number = 0.0, String = DefaultString, Reference = null};

		public enum ValueType : byte
		{
			Nil,
			Number,
			String,
			Function,
			Array,
			Object
		}

		public ValueType Type;
		public double Number;
		object Reference;

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


		public static Value Create(double v)
		{
			var val = new Value();
			val.Type = ValueType.Number;
			val.Number = v;
			val.Reference = null;
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

		public static Value Create(string v)
		{
			var val = new Value();
			val.Type = ValueType.String;
			val.Number = 0.0;
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
			val.Type = ValueType.Number;
			val.Number = (v ? 1.0 : 0.0);
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
			val.Number = 0.0;
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
			val.Number = 0.0;
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

		/*public static Value Create(object v)
		{
			if (v == null)
				return Value.Nil;
			var val = new Value();
			val.Type = ValueType.Object;
			val.Number = 0.0;
			val.Reference = v;
			return val;
		}*/

		public static Value Create()
		{
			var val = new Value();
			val.Type = ValueType.Number;
			val.Number = 0.0;
			val.Reference = null;
			return val;
		}


		public override string ToString()
		{
			if (DynamicValue == null)
				return "nil";
			if (DynamicValue is List<Value>)
				return String.Format("[{0}]", String.Join(", ", (List<Value>)DynamicValue));
			return DynamicValue.ToString();
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
	}
}

