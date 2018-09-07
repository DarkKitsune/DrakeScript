using System;
using System.Collections.Generic;
using System.Linq;

namespace DrakeScript
{
	public class Table
	{
		public Dictionary<Value, Value> InternalDictionary {get; private set;} = new Dictionary<Value, Value>();

		public int Count
		{
			get
			{
				return InternalDictionary.Count;
			}
		}


		public Value this[Value key]
		{
			get
			{
                if (key.Type == Value.ValueType.Nil)
                    return Value.Nil;
				Value outValue;
				if (InternalDictionary.TryGetValue(key, out outValue))
					return outValue;
				return Value.Nil;
			}
			set
			{
                if (key.Type == Value.ValueType.Nil)
                    return;
                InternalDictionary[key] = value;
			}
		}

		public bool TryGetValue(Value key, out Value val)
		{
			return InternalDictionary.TryGetValue(key, out val);
		}


		public Dictionary<Value, Value>.KeyCollection Keys
		{
			get
			{
                return InternalDictionary.Keys;
			}
		}

		public Value[] Values
		{
			get
			{
				return InternalDictionary.Values.ToArray();
			}
		}


		public Table(Dictionary<Value, Value> baseDict)
		{
			foreach (var kvp in baseDict)
			{
				InternalDictionary[kvp.Key] = kvp.Value;
			}
		}
		public Table()
		{
			
		}


		public override string ToString()
		{
			var sb = new System.Text.StringBuilder();
			sb.Append('{');
			var first = true;
			foreach (var key in Keys)
			{
				if (!first)
					sb.Append(", ");
				else
					first = false;
				sb.Append(key is string ? "\"" + key + "\"" : key.ToString());
				sb.Append(':');
				sb.Append(InternalDictionary[key].ToString());
			}
			sb.Append('}');
			return sb.ToString();
		}
	}
}

