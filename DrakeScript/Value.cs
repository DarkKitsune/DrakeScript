using System;

namespace DrakeScript
{
	public struct Value
	{
		static string DefaultString = "";
		public static Value Nil = new Value {Type = ValueType.Nil, Number = 0.0, String = DefaultString, Reference = null};

		public enum ValueType : byte
		{
			Nil,
			Number,
			String,
			Function
		}

		public ValueType Type;
		public double Number;
		public string String;
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

		public Function Function
		{
			get
			{
				if (Reference is Function)
					return (Function)Reference;
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
			val.String = DefaultString;
			val.Reference = null;
			return val;
		}

		public static Value Create(string v)
		{
			var val = new Value();
			val.Type = ValueType.String;
			val.Number = 0.0;
			val.String = v;
			val.Reference = null;
			return val;
		}

		public static Value Create(bool v)
		{
			var val = new Value();
			val.Type = ValueType.Number;
			val.Number = (v ? 1.0 : 0.0);
			val.String = DefaultString;
			val.Reference = null;
			return val;
		}

		public static Value Create(Function v)
		{
			var val = new Value();
			val.Type = ValueType.Function;
			val.Number = 0.0;
			val.String = DefaultString;
			val.Reference = v;
			return val;
		}

		public static Value Create()
		{
			var val = new Value();
			val.Type = ValueType.Number;
			val.Number = 0.0;
			val.String = DefaultString;
			val.Reference = null;
			return val;
		}
	}
}

