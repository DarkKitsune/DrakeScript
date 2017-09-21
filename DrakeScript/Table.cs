using System;
using System.Collections.Generic;
using System.Linq;

namespace DrakeScript
{
	public class Table
	{
		Dictionary<object, Value> Data = new Dictionary<object, Value>();

		public int Count
		{
			get
			{
				return Data.Count;
			}
		}


		public Value this[object key]
		{
			get
			{
				Value outValue;
				if (Data.TryGetValue(key, out outValue))
					return outValue;
				return Value.Nil;
			}
			set
			{
				Data[key] = value;
			}
		}

		public bool TryGetValue(object key, out Value val)
		{
			return Data.TryGetValue(key, out val);
		}


		public object[] Keys
		{
			get
			{
				return Data.Keys.ToArray();
			}
		}

		public Value[] Values
		{
			get
			{
				return Data.Values.ToArray();
			}
		}


		public Table(Dictionary<object, Value> baseDict)
		{
			foreach (var key in baseDict.Keys)
			{
				Data[key] = baseDict[key];
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
				sb.Append(key.ToString());
				sb.Append(':');
				sb.Append(Data[key].DynamicValue.ToString());
			}
			sb.Append('}');
			return sb.ToString();
		}
	}
}

