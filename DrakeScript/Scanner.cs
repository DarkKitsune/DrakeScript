using System;
using System.Collections.Generic;

namespace DrakeScript
{
	public class Scanner
	{
		public List<Token> Scan(Source source)
		{
			var tokenList = new List<Token>();
			Source = source;
			Position = 0;

			while (!EndOf())
			{
				var c = At();

				if (c <= ' ')
				{
					Advance(1);
					continue;
				}

				Token token = Token.Invalid;

				switch (c)
				{
					case ('"'):
						token = ScanString();
						break;
					case ('('):
						token = new Token(Location(), Token.TokenType.ParOpen);
						Advance(1);
						break;
					case (')'):
						token = new Token(Location(), Token.TokenType.ParClose);
						Advance(1);
						break;
					case ('{'):
						token = new Token(Location(), Token.TokenType.BraOpen);
						Advance(1);
						break;
					case ('}'):
						token = new Token(Location(), Token.TokenType.BraClose);
						Advance(1);
						break;
					case (';'):
						token = new Token(Location(), Token.TokenType.Semicolon);
						Advance(1);
						break;
					case (','):
						token = new Token(Location(), Token.TokenType.Comma);
						Advance(1);
						break;
					case ('+'):
						if (At(1) == '=')
						{
							token = new Token(Location(), Token.TokenType.PlusEq);
							Advance(2);
						}
						else
						{
							token = new Token(Location(), Token.TokenType.Plus);
							Advance(1);
						}
						break;
					case ('-'):
						if (At(1) == '=')
						{
							token = new Token(Location(), Token.TokenType.MinusEq);
							Advance(2);
						}
						else
						{
							token = new Token(Location(), Token.TokenType.Minus);
							Advance(1);
						}
						break;
					case ('/'):
						token = new Token(Location(), Token.TokenType.Divide);
						Advance(1);
						break;
					case ('*'):
						token = new Token(Location(), Token.TokenType.Multiply);
						Advance(1);
						break;
					case ('='):
						if (At(1) == '=')
						{
							token = new Token(Location(), Token.TokenType.Eq);
							Advance(2);
						}
						else
						{
							token = new Token(Location(), Token.TokenType.Set);
							Advance(1);
						}
						break;
					case ('!'):
						if (At(1) == '=')
						{
							token = new Token(Location(), Token.TokenType.NEq);
							Advance(2);
						}
						/*else
						{
							token = new Token(Location(), Token.TokenType.Not);
							Advance(1);
						}*/
						break;
					default:
						if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'A') || c == '_')
						{
							token = ScanIdentifier();
						}
						else if (c >= '0' && c <= '9')
						{
							token = ScanNumber();
						}
						break;
				}

				if (!token.Valid)
					throw new UnrecognizedTokenException(new string(c, 1), Location());
				
				tokenList.Add(token);
			}

			return tokenList;
		}
			
		Source Source;
		int Position;

		bool EndOf()
		{
			return Position >= Source.Code.Length;
		}

		void Advance(int num)
		{
			Position += num;
		}

		char At(int num = 0)
		{
			if (Position + num < 0 || Position + num >= Source.Code.Length)
				return '\0';
			return Source.Code[Position + num];
		}

		bool IsEscaped(int offset = 0)
		{
			var c = At(offset - 1);
			if (c != '\\')
			{
				return false;
			}

			var offsetoff = -2;
			c = At(offset + offsetoff);
			int num = 1;
			while (c == '\\')
			{
				num++;
				c = At(offset + (--offsetoff));
			}

			return num % 2 == 1;
		}

		bool IsInQuotes(int offset = 0)
		{
			var offsetoff = -1;
			var c = At(offset + offsetoff);
			int num = 0;
			while (c != '\0')
			{
				if (c == '"' || (IsInQuotes(offset + offsetoff) && IsEscaped(offset + offsetoff)))
					num++;
				c = At(offset + (--offsetoff));
			}

			return num % 2 == 1;
		}

		SourceRef Location(int offset = 0)
		{
			if (Position + offset < 0 || Position + offset >= Source.SourceRefs.Length)
				return SourceRef.Invalid;
			return Source.SourceRefs[Position + offset];
		}




		Token ScanString()
		{
			var offset = 1;
			var c = At(offset);
			while (true)
			{
				if (c == '\0')
				{
					throw new StringNotTerminatedException(Location());
				}
				if (c == '"' && !IsEscaped(offset))
				{
					var token = new Token(
						Location(), Token.TokenType.Str,
						Source.Code.Substring(Position + 1, offset - 1).ApplyEscapes()
					);
					Advance(offset + 1);
					return token;
				}
				c = At(++offset);
			}
		}


		Token ScanIdentifier()
		{
			var offset = 1;
			var c = At(offset);
			while (
				c != '\0' &&
				(
					(c >= 'a' && c <= 'z') ||
					(c >= 'A' && c <= 'A') ||
					c == '_' ||
					(c >= '0' && c <= '9')
				)
			)
			{
				c = At(++offset);
			}

			var token = new Token(Location(), Token.TokenType.Ident, Source.Code.Substring(Position, offset));
			Advance(offset);
			return token;
		}

		Token ScanNumber()
		{
			var dotNum = 0;
			var offset = 1;
			var c = At(offset);
			while (
				c != '\0' &&
				(
					(c >= '0' && c <= '9') ||
					c == '.'
				)
			)
			{
				if (c == '.')
				{
					dotNum++;
					if (dotNum > 1)
					{
						throw new MalformedNumberException(Location());
					}
				}
				c = At(++offset);
			}
			Token token;
			if (dotNum > 0)
				token = new Token(Location(), Token.TokenType.Dec, Source.Code.Substring(Position, offset));
			else
				token = new Token(Location(), Token.TokenType.Int, Source.Code.Substring(Position, offset));
			Advance(offset);
			return token;
		}
	}
}

