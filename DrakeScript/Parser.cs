﻿using System;
using System.Collections.Generic;

using PowerDS;

namespace DrakeScript
{
	public class Parser
	{
		List<Token> Tokens;
		int Position;
		FastStackGrowable<ASTNode> Stack;

		public ASTNode Parse(List<Token> tokens)
		{
			var tree = new ASTNode(ASTNode.NodeType.Root, SourceRef.Invalid, _Parse(tokens));
			return tree;
		}

		List<ASTNode> _Parse(List<Token> tokens, bool useCommaForSemicolon = false)
		{
			var root = new List<ASTNode>();

			Tokens = tokens;
			Position = 0;
			Stack = new FastStackGrowable<ASTNode>();

			while (!EndOf())
			{
				var current = At();

				Parser newParser;
				int advanceAmount;
				ASTNode top = ASTNode.Invalid;
				ASTNode node;
				List<ASTNode> parsed;
				if (Stack.Count > 0)
					top = Stack.Peek(0);
				switch (current.Type)
				{
					case (Token.TokenType.Str):
						Stack.Push(new ASTNode(ASTNode.NodeType.Str, current.Location, current.Value));
						Advance(1);
						break;
					case (Token.TokenType.Int):
						Stack.Push(new ASTNode(ASTNode.NodeType.Int, current.Location, current.Value));
						Advance(1);
						break;
					case (Token.TokenType.Dec):
						Stack.Push(new ASTNode(ASTNode.NodeType.Dec, current.Location, current.Value));
						Advance(1);
						break;
					case (Token.TokenType.Ident):
						switch ((string)current.Value)
						{
							case ("is"):
								Stack.Push(new ASTNode(ASTNode.NodeType.IsOperator, current.Location));
								Advance(1);
								break;
							case ("then"):
								Stack.Push(new ASTNode(ASTNode.NodeType.ThenOperator, current.Location));
								Advance(1);
								break;
							case ("otherwise"):
								Stack.Push(new ASTNode(ASTNode.NodeType.OtherwiseOperator, current.Location));
								Advance(1);
								break;
							case ("contains"):
								Stack.Push(new ASTNode(ASTNode.NodeType.ContainsOperator, current.Location));
								Advance(1);
								break;
							case ("lengthof"):
								Stack.Push(new ASTNode(ASTNode.NodeType.LengthOperator, current.Location));
								Advance(1);
								break;
							case ("true"):
								Stack.Push(new ASTNode(ASTNode.NodeType.Int, current.Location, 1.0));
								Advance(1);
								break;
							case ("false"):
								Stack.Push(new ASTNode(ASTNode.NodeType.Int, current.Location, 0.0));
								Advance(1);
								break;
							case ("nil"):
								Stack.Push(new ASTNode(ASTNode.NodeType.Nil, current.Location));
								Advance(1);
								break;
							case ("return"):
								newParser = new Parser();
								parsed = newParser._Parse(GetUntil(Token.TokenType.Semicolon, 0, out advanceAmount));
								root.Add(new ASTNode(ASTNode.NodeType.Return, current.Location, parsed.GetSafe(0)));
								Advance(advanceAmount);
								break;
							case ("yield"):
								newParser = new Parser();
								parsed = newParser._Parse(GetUntil(Token.TokenType.Semicolon, 0, out advanceAmount));
								root.Add(new ASTNode(ASTNode.NodeType.Yield, current.Location, parsed.GetSafe(0)));
								Advance(advanceAmount);
								break;
							case ("break"):
								root.Add(new ASTNode(ASTNode.NodeType.Break, current.Location));
								Advance(1);
								break;
							case ("if"):
                                Advance(1);
                                if (At().Type != Token.TokenType.ParOpen)
                                    throw new ExpectedTokenException("(", At().Location);
                                newParser = new Parser();
								parsed = newParser._Parse(GetBetweenInclusive(Token.TokenType.ParClose, 0, out advanceAmount));
								var ifPar = parsed.GetSafe(0);
								if (ifPar.Type != ASTNode.NodeType.Par)
								{
									throw new ExpectedNodeException(ASTNode.NodeType.Par, ifPar.Type, current.Location);
								}
								if (parsed.Count > 1)
								{
									throw new ExpectedTokenException("{", parsed[1].Location);
								}
								parsed = newParser._Parse(GetBetween(Token.TokenType.BraClose, advanceAmount, out advanceAmount));
								node = new ASTNode(ASTNode.NodeType.If, current.Location, parsed);
								node.Branches["condition"] = new ASTNode(ASTNode.NodeType.Condition, ifPar.Location, ifPar.Value);
								root.Add(node);
								Advance(advanceAmount);
								break;
							case ("else"):
								if (root.Count == 0)
								{
									throw new ExpectedNodeException(ASTNode.NodeType.If, ASTNode.NodeType.Invalid, current.Location);
								}
								if (root[root.Count - 1].Type != ASTNode.NodeType.If)
								{
									throw new ExpectedNodeException(ASTNode.NodeType.If, root[root.Count - 1].Type, root[root.Count - 1].Location);
								}
								newParser = new Parser();
								parsed = newParser._Parse(GetUntil(Token.TokenType.BraOpen, 0, out advanceAmount));
								if (parsed.Count > 0)
								{
									throw new ExpectedTokenException("{", parsed[0].Location);
								}
								parsed = newParser._Parse(GetBetween(Token.TokenType.BraClose, advanceAmount - 1, out advanceAmount));
								root[root.Count - 1].Branches["else"] = new ASTNode(ASTNode.NodeType.Else, current.Location, parsed);
								Advance(advanceAmount);
								break;
							case ("while"):
                                Advance(1);
                                if (At().Type != Token.TokenType.ParOpen)
                                    throw new ExpectedTokenException("(", At().Location);
                                newParser = new Parser();
                                parsed = newParser._Parse(GetBetweenInclusive(Token.TokenType.ParClose, 0, out advanceAmount));
                                var whilePar = parsed.GetSafe(0);
								if (whilePar.Type != ASTNode.NodeType.Par)
								{
									throw new ExpectedNodeException(ASTNode.NodeType.Par, whilePar.Type, current.Location);
								}
								parsed = newParser._Parse(GetBetween(Token.TokenType.BraClose, advanceAmount, out advanceAmount));
								node = new ASTNode(ASTNode.NodeType.While, current.Location, parsed);
								node.Branches["condition"] = new ASTNode(ASTNode.NodeType.Condition, whilePar.Location, whilePar.Value);
								root.Add(node);
								Advance(advanceAmount);
								break;
							case ("loop"):
                                Advance(1);
                                if (At().Type != Token.TokenType.ParOpen)
                                    throw new ExpectedTokenException("(", At().Location);
                                newParser = new Parser();
                                parsed = newParser._Parse(GetBetweenInclusive(Token.TokenType.ParClose, 0, out advanceAmount));
                                var loopPar = parsed.GetSafe(0);
								if (loopPar.Type != ASTNode.NodeType.Par)
								{
									throw new ExpectedNodeException(ASTNode.NodeType.Par, loopPar.Type, current.Location);
								}
								parsed = newParser._Parse(GetBetween(Token.TokenType.BraClose, advanceAmount, out advanceAmount));
								node = new ASTNode(ASTNode.NodeType.Loop, current.Location, parsed);
								node.Branches["condition"] = new ASTNode(ASTNode.NodeType.Condition, loopPar.Location, loopPar.Value);
								root.Add(node);
								Advance(advanceAmount);
								break;
                            case ("foreach"):
                                Advance(1);
                                if (At().Type != Token.TokenType.ParOpen)
                                    throw new ExpectedTokenException("(", At().Location);
                                newParser = new Parser();
                                parsed = newParser._Parse(GetBetweenInclusive(Token.TokenType.ParClose, 0, out advanceAmount));
                                loopPar = parsed.GetSafe(0);
                                if (loopPar.Type != ASTNode.NodeType.Par)
                                {
                                    throw new ExpectedNodeException(ASTNode.NodeType.Par, loopPar.Type, current.Location);
                                }
                                parsed = newParser._Parse(GetBetween(Token.TokenType.BraClose, advanceAmount, out advanceAmount));
                                node = new ASTNode(ASTNode.NodeType.Foreach, current.Location, parsed);
                                node.Branches["condition"] = new ASTNode(ASTNode.NodeType.Condition, loopPar.Location, loopPar.Value);
                                root.Add(node);
                                Advance(advanceAmount);
                                break;
                            case ("function"):
								newParser = new Parser();
								parsed = newParser._Parse(GetUntil(Token.TokenType.BraOpen, 0, out advanceAmount));
								var functionPar = parsed.GetSafe(0);
								var functionName = "";
								if (functionPar.Type == ASTNode.NodeType.Call)
								{
									if (functionPar.Branches["function"].Type != ASTNode.NodeType.Ident)
									{
										throw new ExpectedNodeException(ASTNode.NodeType.Ident, functionPar.Branches["function"].Type, functionPar.Branches["function"].Location);
									}
									functionName = (string)functionPar.Branches["function"].Value;
									functionPar = functionPar.Branches["args"];
								}
								else
								{
									if (functionPar.Type != ASTNode.NodeType.Par)
									{
										throw new ExpectedNodeException(ASTNode.NodeType.Par, functionPar.Type, current.Location);
									}
								}
								parsed = newParser._Parse(GetBetween(Token.TokenType.BraClose, advanceAmount - 1, out advanceAmount));
								var newRoot = new ASTNode(ASTNode.NodeType.Root, SourceRef.Invalid, parsed);
								node = new ASTNode(ASTNode.NodeType.Function, current.Location, newRoot);
								node.Branches["args"] = new ASTNode(ASTNode.NodeType.Args, functionPar.Location, functionPar.Value);
								node.Branches["additionalArgs"] = new ASTNode(ASTNode.NodeType.Int, functionPar.Location, 0);
								node.Branches["functionName"] = new ASTNode(ASTNode.NodeType.Ident, current.Location, functionName);
								if (functionName.Length > 0)
								{
									if (top.Type == ASTNode.NodeType.Ident && (string)top.Value == "let")
									{
										Stack.Pop();
										root.Add(new ASTNode(ASTNode.NodeType.NewLocal, top.Location, functionName));
									}
									root.Add(node);
								}
								else
								{
									Stack.Push(node);
								}
								Advance(advanceAmount);
								break;
							default:
								if (top.Type == ASTNode.NodeType.Ident && (string)top.Value == "let")
								{
									Stack.Pop();
									Stack.Push(new ASTNode(ASTNode.NodeType.NewLocal, current.Location, current.Value));
								}
								else
									Stack.Push(new ASTNode(ASTNode.NodeType.Ident, current.Location, current.Value));
								Advance(1);
								break;
						}
						break;
					case (Token.TokenType.Plus):
						Stack.Push(new ASTNode(ASTNode.NodeType.PlusOperator, current.Location));
						Advance(1);
						break;
					case (Token.TokenType.Minus):
						Stack.Push(new ASTNode(ASTNode.NodeType.MinusOperator, current.Location));
						Advance(1);
						break;
					case (Token.TokenType.Divide):
						Stack.Push(new ASTNode(ASTNode.NodeType.DivideOperator, current.Location));
						Advance(1);
						break;
					case (Token.TokenType.Multiply):
						Stack.Push(new ASTNode(ASTNode.NodeType.MultiplyOperator, current.Location));
						Advance(1);
						break;
					case (Token.TokenType.Modulo):
						Stack.Push(new ASTNode(ASTNode.NodeType.ModuloOperator, current.Location));
						Advance(1);
						break;
					case (Token.TokenType.Power):
						Stack.Push(new ASTNode(ASTNode.NodeType.PowerOperator, current.Location));
						Advance(1);
						break;
					case (Token.TokenType.Concat):
						Stack.Push(new ASTNode(ASTNode.NodeType.ConcatOperator, current.Location));
						Advance(1);
						break;
					case (Token.TokenType.Colon):
						Stack.Push(new ASTNode(ASTNode.NodeType.PairOperator, current.Location));
						Advance(1);
						break;
                    case (Token.TokenType.Arrow):
                        Stack.Push(new ASTNode(ASTNode.NodeType.TablePairOperator, current.Location));
                        Advance(1);
                        break;
                    case (Token.TokenType.Period):
						Stack.Push(new ASTNode(ASTNode.NodeType.DotIndexOperator, current.Location));
						Advance(1);
						break;
					case (Token.TokenType.Not):
						Stack.Push(new ASTNode(ASTNode.NodeType.NotOperator, current.Location));
						Advance(1);
						break;
                    case (Token.TokenType.DollarSign):
                        Stack.Push(new ASTNode(ASTNode.NodeType.ThreadOperator, current.Location));
                        Advance(1);
                        break;
                    case (Token.TokenType.Eq):
						Stack.Push(new ASTNode(ASTNode.NodeType.EqualsOperator, current.Location));
						Advance(1);
						break;
                    case (Token.TokenType.SEq):
                        Stack.Push(new ASTNode(ASTNode.NodeType.SeqEqualsOperator, current.Location));
                        Advance(1);
                        break;
                    case (Token.TokenType.NEq):
						Stack.Push(new ASTNode(ASTNode.NodeType.NotEqualsOperator, current.Location));
						Advance(1);
						break;
					case (Token.TokenType.Gt):
						Stack.Push(new ASTNode(ASTNode.NodeType.GtOperator, current.Location));
						Advance(1);
						break;
					case (Token.TokenType.GtEq):
						Stack.Push(new ASTNode(ASTNode.NodeType.GtEqOperator, current.Location));
						Advance(1);
						break;
					case (Token.TokenType.Lt):
						Stack.Push(new ASTNode(ASTNode.NodeType.LtOperator, current.Location));
						Advance(1);
						break;
					case (Token.TokenType.LtEq):
						Stack.Push(new ASTNode(ASTNode.NodeType.LtEqOperator, current.Location));
						Advance(1);
						break;
					case (Token.TokenType.Or):
						Stack.Push(new ASTNode(ASTNode.NodeType.OrOperator, current.Location));
						Advance(1);
						break;
					case (Token.TokenType.And):
						Stack.Push(new ASTNode(ASTNode.NodeType.AndOperator, current.Location));
						Advance(1);
						break;
					case (Token.TokenType.Set):
						Stack.Push(new ASTNode(ASTNode.NodeType.SetOperator, current.Location));
						Advance(1);
						break;
					case (Token.TokenType.PlusEq):
						Stack.Push(new ASTNode(ASTNode.NodeType.PlusEqOperator, current.Location));
						Advance(1);
						break;
					case (Token.TokenType.MinusEq):
						Stack.Push(new ASTNode(ASTNode.NodeType.MinusEqOperator, current.Location));
						Advance(1);
						break;
					case (Token.TokenType.ParOpen):
						newParser = new Parser();
						parsed = newParser._Parse(GetBetween(Token.TokenType.ParClose, 0, out advanceAmount), true);
						if (ASTNode.NodeInfo[top.Type].HasValue)
						{
							var newNode = new ASTNode(ASTNode.NodeType.Call, top.Location);
							newNode.Branches.Add("function", Stack.Pop());
							newNode.Branches.Add("args", new ASTNode(ASTNode.NodeType.Args, current.Location, parsed));
							newNode.Branches.Add("additionalArgs", new ASTNode(ASTNode.NodeType.Int, current.Location, 0));
							Stack.Push(newNode);
							Advance(advanceAmount);
						}
						else
						{
							Stack.Push(new ASTNode(ASTNode.NodeType.Par, current.Location, parsed));
							Advance(advanceAmount);
						}
						break;
					case (Token.TokenType.SqBraOpen):
						newParser = new Parser();
						parsed = newParser._Parse(GetBetween(Token.TokenType.SqBraClose, 0, out advanceAmount), true);
						if (ASTNode.NodeInfo[top.Type].HasValue)
						{
							if (parsed.Count > 1)
							{
								throw new ExpectedTokenException("]", parsed[1].Location);
							}
							var newNode = new ASTNode(ASTNode.NodeType.Index, top.Location);
							newNode.Branches.Add("arrayOrTable", Stack.Pop());
							newNode.Branches.Add("index", parsed[0]);
							Stack.Push(newNode);
							Advance(advanceAmount);
						}
						else
						{
							Stack.Push(new ASTNode(ASTNode.NodeType.Array, current.Location, parsed));
							Advance(advanceAmount);
						}
						break;
					case (Token.TokenType.BraOpen):
						newParser = new Parser();
						parsed = newParser._Parse(GetBetween(Token.TokenType.BraClose, 0, out advanceAmount), true);
						foreach (var p in parsed)
						{
							if (p.Type != ASTNode.NodeType.TablePair)
								throw new ExpectedTokenException("... => ...", p.Location);
						}
						Stack.Push(new ASTNode(ASTNode.NodeType.Table, current.Location, parsed));
						Advance(advanceAmount);
						break;
					case (Token.TokenType.Semicolon):
						if (useCommaForSemicolon)
						{
							throw new UnexpectedTokenException(";", current.Location);
						}
						else
						{
							DoOperators();
							if (Stack.Count > 1)
							{
								if (useCommaForSemicolon)
									throw new MissingCommaException(Stack.GetRaw(1).Location);
								else
									throw new MissingSemicolonException(Stack.GetRaw(1).Location);
							}
							if (Stack.Count == 1)
							{
								root.Add(Stack.Pop());
							}
							Advance(1);
						}
						break;
					case (Token.TokenType.Comma):
						if (!useCommaForSemicolon)
						{
							throw new UnexpectedTokenException(",", current.Location);
						}
						else
						{
							DoOperators();
							if (Stack.Count > 1)
							{

								if (useCommaForSemicolon)
									throw new MissingCommaException(Stack.GetRaw(1).Location);
								else
									throw new MissingSemicolonException(Stack.GetRaw(1).Location);
							}
							if (Stack.Count == 1)
							{
								root.Add(Stack.Pop());
							}
							Advance(1);
						}
						break;
					default:
						throw new UnrecognizedTokenTypeException(current.Type, current.Location);
				}
			}
			DoOperators();
			if (Stack.Count > 1)
			{
				if (useCommaForSemicolon)
					throw new MissingCommaException(Stack.GetRaw(1).Location);
				else
					throw new MissingSemicolonException(Stack.GetRaw(1).Location);
			}
			if (Stack.Count == 1)
			{
				root.Add(Stack.Pop());
			}

			return root;
		}


		bool EndOf()
		{
			return Position >= Tokens.Count;
		}

		void Advance(int num)
		{
			Position += num;
		}

		Token At(int num = 0)
		{
			if (Position + num < 0 || Position + num >= Tokens.Count)
				return Token.Invalid;
			return Tokens[Position + num];
		}

		SourceRef Location(int offset = 0)
		{
			if (Position + offset < 0 || Position + offset >= Tokens.Count)
				return SourceRef.Invalid;
			return Tokens[Position + offset].Location;
		}

		List<ASTNode> TempStackList = new List<ASTNode>();
		void DoOperators()
		{
			TempStackList.Clear();
			foreach (ASTNode node in Stack)
			{
				if (node.Type != ASTNode.NodeType.Invalid)
					TempStackList.Add(node);
			}

			for (var precedence = 1; precedence <= ASTNode.MaxPrecedence && Stack.Count >= 2; precedence++)
			{
				for (var pos = 0; pos < TempStackList.Count - 1;)
				{
					var converted = false;
					
					var op = TempStackList[pos];
					var opInfo = ASTNode.NodeInfo[op.Type];

					if (opInfo.InfixOperatorPrecedence == precedence && pos > 0 && opInfo.InfixOperator)
					{
						var left = TempStackList[pos - 1];
						var leftInfo = ASTNode.NodeInfo[left.Type];
						var right = TempStackList[pos + 1];
						var rightInfo = ASTNode.NodeInfo[right.Type];

						if (leftInfo.HasValue && rightInfo.HasValue)
						{
							converted = true;
							pos--;
							TempStackList.RemoveAt(pos);
							TempStackList.RemoveAt(pos);
							TempStackList.RemoveAt(pos);

							if (opInfo.InfixOperatorType == ASTNode.NodeType.Invalid)
								throw new Exception(op.Type + " should not have invalid infix operator type");
							var node = new ASTNode(opInfo.InfixOperatorType, op.Location);
							node.Branches.Add("left", left);
							node.Branches.Add("right", right);
							TempStackList.Insert(pos, node);
						}
					}
					if (!converted && opInfo.PrefixOperatorPrecedence == precedence && opInfo.PrefixOperator)
					{
						var cancel = false;
						if (pos > 0 && opInfo.InfixOperator && opInfo.PrefixOperatorPrecedence < opInfo.InfixOperatorPrecedence)
						{
							var left = TempStackList[pos - 1];
							if (ASTNode.NodeInfo[left.Type].HasValue)
								cancel = true;
						}
						if (!cancel)
						{
							var right = TempStackList[pos + 1];
							var rightInfo = ASTNode.NodeInfo[right.Type];

							if (rightInfo.HasValue)
							{
								converted = true;
								TempStackList.RemoveAt(pos);
								TempStackList.RemoveAt(pos);

								if (opInfo.PrefixOperatorType == ASTNode.NodeType.Invalid)
									throw new Exception(op.Type + " should not have invalid prefix operator type");
								var node = new ASTNode(opInfo.PrefixOperatorType, op.Location);
								node.Branches.Add("right", right);
								TempStackList.Insert(pos, node);
							}
						}
					}

					if (!converted)
						pos++;
				}
			}

			Stack.Clear();
			foreach (var node in TempStackList)
			{
				if (node.Type != ASTNode.NodeType.Invalid)
				{
					NodeInfo info;
					if (!ASTNode.NodeInfo.TryGetValue(node.Type, out info))
					{
						throw new NoInfoForNodeException(node.Type, node.Location);
					}
					if (info.InfixOperator || info.PrefixOperator)
						throw new InvalidOperandsException(node.Type, node.Location);
					Stack.Push(node);
				}
			}
		}

		List<Token> GetBetween(Token.TokenType closing, int offset, out int advanceAmount)
		{
			var aoffset = offset;
			var starting = At(aoffset);

			var t = At(++aoffset);
			var depth = 1;
			while (true)
			{
                if (!t.Valid)
				{
					advanceAmount = aoffset + 1;
					throw new UnmatchedTokenException(starting.Type, starting.Location);
				}

				if (t.Type == starting.Type)
				{
					depth++;
				}
				else if (t.Type == closing)
				{
					depth--;
					if (depth == 0)
					{
						var ret = new List<Token>();
						for (var i = offset + 1; i < aoffset; i++)
						{
							ret.Add(At(i));
						}
						advanceAmount = aoffset + 1;
						return ret;
					}
				}

				t = At(++aoffset);
			}
		}


        List<Token> GetBetweenInclusive(Token.TokenType closing, int offset, out int advanceAmount)
        {
            var aoffset = offset;
            var starting = At(aoffset);

            var t = At(++aoffset);
            var depth = 1;
            while (true)
            {
                if (!t.Valid)
                {
                    advanceAmount = aoffset + 1;
                    throw new UnmatchedTokenException(starting.Type, starting.Location);
                }

                if (t.Type == starting.Type)
                {
                    depth++;
                }
                else if (t.Type == closing)
                {
                    depth--;
                    if (depth == 0)
                    {
                        var ret = new List<Token>();
                        for (var i = offset; i <= aoffset; i++)
                        {
                            ret.Add(At(i));
                        }
                        advanceAmount = aoffset + 1;
                        return ret;
                    }
                }

                t = At(++aoffset);
            }
        }


        List<Token> GetUntil(Token.TokenType closing, int offset, out int advanceAmount)
		{
			var aoffset = offset;

			var t = At(++aoffset);
			while (true)
			{
				if (Position + aoffset >= Tokens.Count || t.Type == closing)
				{
					var ret = new List<Token>();
					for (var i = offset + 1; i < aoffset; i++)
					{
						ret.Add(At(i));
					}
					advanceAmount = aoffset + 1;
					return ret;
				}

				t = At(++aoffset);
			}
		}
	}
}

