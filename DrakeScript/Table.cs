using System;
using System.Collections.Generic;
using System.Linq;

namespace DrakeScript
{
	public class Table
	{
		public Dictionary<object, Value> InternalDictionary {get; private set;} = new Dictionary<object, Value>();

		public int Count
		{
			get
			{
				return InternalDictionary.Count;
			}
		}


		public Value this[object key]
		{
			get
			{
				Value outValue;
				if (InternalDictionary.TryGetValue(key, out outValue))
					return outValue;
				return Value.Nil;
			}
			set
			{
				InternalDictionary[key] = value;
			}
		}

		public bool TryGetValue(object key, out Value val)
		{
			return InternalDictionary.TryGetValue(key, out val);
		}


		public object[] Keys
		{
			get
			{
				return InternalDictionary.Keys.ToArray();
			}
		}

		public Value[] Values
		{
			get
			{
				return InternalDictionary.Values.ToArray();
			}
		}


		public Table(Dictionary<object, Value> baseDict)
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

