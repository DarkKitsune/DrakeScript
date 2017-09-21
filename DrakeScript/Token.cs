using System;
using System.Collections.Generic;

namespace DrakeScript
{
	public struct Token
	{
		public static Token Invalid = new Token {Valid = false, Location = SourceRef.Invalid, Type = TokenType.Invalid, Value = null};

		public enum TokenType
		{
			Invalid, Str, Int, Dec, Ident, ParOpen, ParClose, BraOpen, BraClose, SqBraOpen, SqBraClose, Semicolon, Comma, Period, Plus,
			Minus, Divide, Multiply, Not, Eq, NEq, Gt, GtEq, Lt, LtEq, Set, PlusEq, MinusEq, Tilde, Colon
		}

		public bool Valid {get; private set;}
		public SourceRef Location {get; private set;}
		public TokenType Type {get; private set;}
		public object Value {get; private set;}

		public Token(SourceRef location, TokenType type, string valueString = null)
		{
			Valid = true;
			Location = location;
			Type = type;

			Value = null;

			if (valueString != null)
			{
				switch (type)
				{
					case (TokenType.Int):
						SetValueInt(valueString);
						break;
					case (TokenType.Dec):
						SetValueDec(valueString);
						break;
					default:
						SetValueStr(valueString);
						break;
				}
				if (Value == null)
				{
					throw new UnrecognizedTokenException(valueString, location);
				}
			}
		}

		public void SetValueInt(string str)
		{
			long a;
			if (long.TryParse(str, out a))
			{
				Value = a;
			}
		}

		public void SetValueDec(string str)
		{
			double a;
			if (double.TryParse(str, out a))
			{
				Value = a;
			}
		}

		public void SetValueStr(string str)
		{
			Value = str;
		}



		public override string ToString()
		{
			return string.Format("[Token: Valid={0}, Location={1}, Type={2}, Value={3}]", Valid, Location, Type, Value);
		}
	}
}

