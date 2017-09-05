using System;
using System.Collections.Generic;
using System.Text;

namespace DrakeScript
{
	public static class Extensions
	{
		public static string ToStringFormatted(this List<Token> tokens)
		{
			return string.Format("[\n    {0}\n]", String.Join(",\n    ", tokens));
		}

		public static string ToStringFormatted(this Instruction[] instructions)
		{
			return string.Format("[\n    {0}\n]", String.Join(",\n    ", instructions));
		}

		public static bool IsEscaped(this string str, int pos)
		{
			if (pos - 1 < 0 || pos - 1 >= str.Length)
				return false;

			var c = str[pos - 1];
			if (c != '\\')
			{
				return false;
			}

			if (pos - 2 < 0 || pos - 2 >= str.Length)
				return true;

			var offset = -2;
			c = str[pos + offset];
			int num = 1;
			while (c == '\\')
			{
				num++;
				offset--;
				if (pos + offset < 0)
					break;
				c = str[pos + offset];
			}

			return num % 2 == 1;
		}

		public static string ApplyEscapes(this string str)
		{
			var sb = new StringBuilder();
			for (var i = str.Length - 1; i >= 0; i--)
			{
				if (!str.IsEscaped(i))
				{
					sb.Insert(0, str[i]);
				}
				else
				{
					switch (str[i])
					{
						case ('n'):
							sb.Insert(0, '\n');
							break;
						case ('r'):
							sb.Insert(0, '\r');
							break;
						default:
							sb.Insert(0, str[i]);
							break;
					}
					i--;
				}
			}
			return sb.ToString();
		}

		public static ASTNode GetSafe(this List<ASTNode> list, int n)
		{
			if (n < 0 || n >= list.Count)
				return ASTNode.Invalid;
			return list[n];
		}
	}
}

