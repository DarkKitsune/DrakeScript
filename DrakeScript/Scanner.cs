using System;
using System.Linq;
using System.Collections.Generic;

namespace DrakeScript
{
	public class Scanner
	{
		public Dictionary<string, List<Token>> Macros = new Dictionary<string, List<Token>>();

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
				var wasComment = false;
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
					case ('['):
						token = new Token(Location(), Token.TokenType.SqBraOpen);
						Advance(1);
						break;
					case (']'):
						token = new Token(Location(), Token.TokenType.SqBraClose);
						Advance(1);
						break;
					case (';'):
						token = new Token(Location(), Token.TokenType.Semicolon);
						Advance(1);
						break;
					case (':'):
						token = new Token(Location(), Token.TokenType.Colon);
						Advance(1);
						break;
					case (','):
						token = new Token(Location(), Token.TokenType.Comma);
						Advance(1);
						break;
					case ('.'):
						token = new Token(Location(), Token.TokenType.Period);
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
						if (At(1) == '/')
						{
							wasComment = true;
							AdvanceUntilLineBreak();
							Advance(1);
							break;
						}
						if (At(1) == '*')
						{
							wasComment = true;
							AdvanceUntilEndComment();
							Advance(2);
							break;
						}
						token = new Token(Location(), Token.TokenType.Divide);
						Advance(1);
						break;
					case ('*'):
						token = new Token(Location(), Token.TokenType.Multiply);
						Advance(1);
						break;
					case ('%'):
						token = new Token(Location(), Token.TokenType.Modulo);
						Advance(1);
						break;
					case ('^'):
						token = new Token(Location(), Token.TokenType.Power);
						Advance(1);
						break;
					case ('='):
						if (At(1) == '=')
						{
                            if (At(2) == '=')
                            {
                                token = new Token(Location(), Token.TokenType.SEq);
                                Advance(3);
                            }
                            else
                            {
                                token = new Token(Location(), Token.TokenType.Eq);
                                Advance(2);
                            }
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
						else
						{
							token = new Token(Location(), Token.TokenType.Not);
							Advance(1);
						}
						break;
					case ('>'):
						if (At(1) == '=')
						{
							token = new Token(Location(), Token.TokenType.GtEq);
							Advance(2);
						}
						else
						{
							token = new Token(Location(), Token.TokenType.Gt);
							Advance(1);
						}
						break;
					case ('<'):
						if (At(1) == '=')
						{
							token = new Token(Location(), Token.TokenType.LtEq);
							Advance(2);
						}
						else
						{
							token = new Token(Location(), Token.TokenType.Lt);
							Advance(1);
						}
						break;
					case ('|'):
						if (At(1) == '|')
						{
							token = new Token(Location(), Token.TokenType.Or);
							Advance(2);
						}
						break;
					case ('&'):
						if (At(1) == '&')
						{
							token = new Token(Location(), Token.TokenType.And);
							Advance(2);
						}
						break;
					case ('~'):
						token = new Token(Location(), Token.TokenType.Tilde);
						Advance(1);
						break;
					case ('#'):
						var loc = Location();
						wasComment = true;
						var directiveString = GetUntilLineBreak().Trim();
						var dsb = new System.Text.StringBuilder();
						for (var dsbi = 0; dsbi < directiveString.Length; dsbi++)
						{
							if (!(directiveString[dsbi] == ' ' && dsbi < directiveString.Length - 1 && directiveString[dsbi + 1] == ' '))
								dsb.Append(directiveString[dsbi]);
						}
						var directiveParts = dsb.ToString().Split(' ');
						if (directiveParts.Length < 1)
							break;
						var directive = directiveParts[0];
						switch (directive)
						{
							case ("define"):
								if (directiveParts.Length < 2)
									break;
								var macro = directiveParts[1];
								var arg = "";
								if (directiveParts.Length >= 3)
									arg = String.Join(" ", directiveParts.Skip(2));
								var dsource = new Source("(Macro '" + macro + "' at " + loc + ")", arg);
								Macros[macro] = (new Scanner()).Scan(dsource);
								break;
						}
						Advance(1);
						break;
					default:
						foreach (var kvp in Macros)
						{
							if (Matches(kvp.Key))
							{
								wasComment = true;

								var mtokens = new List<Token>(kvp.Value.Count);
								foreach (var mtoken in kvp.Value)
								{
									var mloc = Location();
									var nmtoken = new Token(new SourceRef(new Source("Expansion of " + mtoken.Location.Source.Name + " in " + mloc.Source.Name, ""), mloc.Line, mloc.Column), mtoken.Type, mtoken.Value, 0);
									mtokens.Add(nmtoken);
								}
								tokenList.AddRange(mtokens);

								Advance(kvp.Key.Length - 1);
								break;
							}
						}
						if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_')
						{
							token = ScanIdentifier();
						}
						else if (c >= '0' && c <= '9')
						{
							token = ScanNumber();
						}
						break;
				}

				if (!wasComment)
				{
					if (!token.Valid)
						throw new UnrecognizedTokenException(new string(c, 1), Location());

					tokenList.Add(token);
				}
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

		void AdvanceUntilLineBreak()
		{
			Position++;
			while (At(0) != '\n' && Position < Source.Code.Length)
				Position += 1;
		}

		string GetUntilLineBreak()
		{
			char chr;
			var sb = new System.Text.StringBuilder();
			Position++;
			while ((chr = At(0)) != '\n' && Position < Source.Code.Length)
			{
				sb.Append(chr);
				Position += 1;
			}
			return sb.ToString();
		}

		void AdvanceUntilEndComment()
		{
			Position++;
			while (!(At(0) == '*' && At(1) == '/') && Position < Source.Code.Length)
				Position += 1;
		}

		char At(int num = 0)
		{
			if (Position + num < 0 || Position + num >= Source.Code.Length)
				return '\0';
			return Source.Code[Position + num];
		}

		bool Matches(string str, int num = 0)
		{
			for (var i = 0; i < str.Length; i++)
			{
				if (str[i] != At(num + i))
				{
					return false;
				}
			}
			return true;
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
					(c >= 'A' && c <= 'Z') ||
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

