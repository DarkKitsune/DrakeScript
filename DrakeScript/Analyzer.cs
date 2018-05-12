using System;
using System.Collections.Generic;
using System.Linq;

namespace DrakeScript
{
	public class Analyzer
	{
		public ASTNode Analyze(ASTNode node, ASTNode parent = default(ASTNode))
		{
			if (node.Type == ASTNode.NodeType.Invalid)
				return node;
			var keys = node.Branches.Keys.ToArray();
			for (var i = 0; i < keys.Length; i++)
			{
				node.Branches[keys[i]] = Analyze(node.Branches[keys[i]], node);
			}

			if (node.Value is List<ASTNode>)
			{
				var list = (List<ASTNode>)node.Value;
				for (var i = 0; i < list.Count; i++)
				{
					list[i] = Analyze(list[i], node);
				}
			}
			else if (node.Value is ASTNode)
			{
				node.Value = Analyze((ASTNode)node.Value, node);
			}

			ASTNode newNode, left, right;
			switch (node.Type)
			{
				case (ASTNode.NodeType.Negative):
					newNode = node.Branches["right"];
					if (newNode.Value is long)
					{
						newNode.Value = -(long)newNode.Value;
						node = newNode;
					}
					else if (newNode.Value is double)
					{
						newNode.Value = -(double)newNode.Value;
						node = newNode;
					}
					break;
				case (ASTNode.NodeType.Add):
					left = node.Branches["left"];
					if (left.Value is long)
					{
						right = node.Branches["right"];
						if (right.Value is long)
						{
							node = left;
							node.Value = (long)left.Value + (long)right.Value;
						}
						else if (right.Value is double)
						{
							node = left;
							node.Value = (double)(long)left.Value + (double)right.Value;
						}
					}
					else if (left.Value is double)
					{
						right = node.Branches["right"];
						if (right.Value is long)
						{
							node = left;
							node.Value = (double)left.Value + (double)(long)right.Value;
						}
						else if (right.Value is double)
						{
							node = left;
							node.Value = (double)left.Value + (double)right.Value;
						}
					}
					break;
				case (ASTNode.NodeType.Subtract):
					left = node.Branches["left"];
					if (left.Value is long)
					{
						right = node.Branches["right"];
						if (right.Value is long)
						{
							node = left;
							node.Value = (long)left.Value - (long)right.Value;
						}
						else if (right.Value is double)
						{
							node = left;
							node.Value = (double)(long)left.Value - (double)right.Value;
						}
					}
					else if (left.Value is double)
					{
						right = node.Branches["right"];
						if (right.Value is long)
						{
							node = left;
							node.Value = (double)left.Value - (double)(long)right.Value;
						}
						else if (right.Value is double)
						{
							node = left;
							node.Value = (double)left.Value - (double)right.Value;
						}
					}
					break;
				case (ASTNode.NodeType.Divide):
					left = node.Branches["left"];
					if (left.Value is long)
					{
						right = node.Branches["right"];
						if (right.Value is long)
						{
							node = left;
							node.Value = (long)left.Value / (long)right.Value;
						}
						else if (right.Value is double)
						{
							node = left;
							node.Value = (double)(long)left.Value / (double)right.Value;
						}
					}
					else if (left.Value is double)
					{
						right = node.Branches["right"];
						if (right.Value is long)
						{
							node = left;
							node.Value = (double)left.Value / (double)(long)right.Value;
						}
						else if (right.Value is double)
						{
							node = left;
							node.Value = (double)left.Value / (double)right.Value;
						}
					}
					break;
				case (ASTNode.NodeType.Multiply):
					left = node.Branches["left"];
					if (left.Value is long)
					{
						right = node.Branches["right"];
						if (right.Value is long)
						{
							node = left;
							node.Value = (long)left.Value * (long)right.Value;
						}
						else if (right.Value is double)
						{
							node = left;
							node.Value = (double)(long)left.Value * (double)right.Value;
						}
					}
					else if (left.Value is double)
					{
						right = node.Branches["right"];
						if (right.Value is long)
						{
							node = left;
							node.Value = (double)left.Value * (double)(long)right.Value;
						}
						else if (right.Value is double)
						{
							node = left;
							node.Value = (double)left.Value * (double)right.Value;
						}
					}
					break;
				case (ASTNode.NodeType.Modulo):
					left = node.Branches["left"];
					if (left.Value is long)
					{
						right = node.Branches["right"];
						if (right.Value is long)
						{
							node = left;
							node.Value = (long)left.Value % (long)right.Value;
						}
						else if (right.Value is double)
						{
							node = left;
							node.Value = (double)(long)left.Value % (double)right.Value;
						}
					}
					else if (left.Value is double)
					{
						right = node.Branches["right"];
						if (right.Value is long)
						{
							node = left;
							node.Value = (double)left.Value % (double)(long)right.Value;
						}
						else if (right.Value is double)
						{
							node = left;
							node.Value = (double)left.Value % (double)right.Value;
						}
					}
					break;
				case (ASTNode.NodeType.Power):
					left = node.Branches["left"];
					if (left.Value is long)
					{
						right = node.Branches["right"];
						if (right.Value is long)
						{
							node = left;
							node.Value = (long)Math.Pow((double)(long)left.Value, (double)(long)right.Value);
						}
						else if (right.Value is double)
						{
							node = left;
							node.Value = (long)Math.Pow((double)(long)left.Value, (double)right.Value);
						}
					}
					else if (left.Value is double)
					{
						right = node.Branches["right"];
						if (right.Value is long)
						{
							node = left;
							node.Value = Math.Pow((double)left.Value, (double)(long)right.Value);
						}
						else if (right.Value is double)
						{
							node = left;
							node.Value = Math.Pow((double)left.Value, (double)right.Value);
						}
					}
					break;
				case (ASTNode.NodeType.Set):
					left = node.Branches["left"];
					if (left.Type == ASTNode.NodeType.Ident)
					{
						node.Branches["left"] = new ASTNode(ASTNode.NodeType.Str, left.Location, left.Value);
					}
					else if (left.Type == ASTNode.NodeType.Index)
					{
						//leave alone
					}
					else if (left.Type == ASTNode.NodeType.NewLocal)
					{
						//leave alone
					}
					else
						throw new ExpectedNodeException(ASTNode.NodeType.Ident, left.Type, left.Location);
					break;
				case (ASTNode.NodeType.PlusEq):
					left = node.Branches["left"];
					if (left.Type == ASTNode.NodeType.Ident)
					{
						node.Branches["left"] = new ASTNode(ASTNode.NodeType.Str, left.Location, left.Value);
					}
					else
						throw new ExpectedNodeException(ASTNode.NodeType.Ident, left.Type, left.Location);
					break;
				case (ASTNode.NodeType.MinusEq):
					left = node.Branches["left"];
					if (left.Type == ASTNode.NodeType.Ident)
					{
						node.Branches["left"] = new ASTNode(ASTNode.NodeType.Str, left.Location, left.Value);
					}
					else
						throw new ExpectedNodeException(ASTNode.NodeType.Ident, left.Type, left.Location);
					break;
				case (ASTNode.NodeType.Pair):
					if (parent.Type == ASTNode.NodeType.Table)
						break;
					left = node.Branches["left"];
					right = node.Branches["right"];
                    if (right.Type == ASTNode.NodeType.Call && right.Branches["function"].Type == ASTNode.NodeType.Ident)
					{
						node = new ASTNode(ASTNode.NodeType.MethodCall, node.Location, right.Value);
						node.Branches.Add("arrayOrTable", left);
						node.Branches.Add("index", new ASTNode(ASTNode.NodeType.Str, right.Location, right.Branches["function"].Value));
						node.Branches.Add("args", right.Branches["args"]);
						node.Branches.Add("additionalArgs", new ASTNode(ASTNode.NodeType.Int, node.Location, 1));
					}
					else
						throw new ExpectedNodeException(ASTNode.NodeType.Ident, right.Type, right.Location);
					break;
                case (ASTNode.NodeType.Thread):
                    right = node.Branches["right"];
                    if (right.Type == ASTNode.NodeType.Call)
                    {
                        node.Branches["function"] = right.Branches["function"];
                        node.Branches["args"] = right.Branches["args"];
                        node.Branches.Remove("right");
                    }
                    else
                        throw new ExpectedNodeException(ASTNode.NodeType.Call, right.Type, right.Location);
                    break;
                case (ASTNode.NodeType.DotIndex):
					left = node.Branches["left"];
					right = node.Branches["right"];
					if (right.Type == ASTNode.NodeType.Ident)
					{
						newNode = new ASTNode(ASTNode.NodeType.Index, node.Location);
						newNode.Branches.Add("arrayOrTable", left);
						newNode.Branches.Add("index", new ASTNode(ASTNode.NodeType.Str, right.Location, right.Value));
						node = newNode;
					}
					else if (right.Type == ASTNode.NodeType.Call && right.Branches["function"].Type == ASTNode.NodeType.Ident)
					{
						newNode = new ASTNode(ASTNode.NodeType.Index, node.Location);
						newNode.Branches.Add("arrayOrTable", left);
						newNode.Branches.Add("index", new ASTNode(ASTNode.NodeType.Str, right.Location, right.Branches["function"].Value));
						node = new ASTNode(ASTNode.NodeType.Call, node.Location, right.Value);
						node.Branches.Add("function", newNode);
						node.Branches.Add("args", right.Branches["args"]);
						node.Branches.Add("additionalArgs", right.Branches["additionalArgs"]);
					}
					else if (right.Type == ASTNode.NodeType.Index && right.Branches["arrayOrTable"].Type == ASTNode.NodeType.Ident)
					{
						newNode = new ASTNode(ASTNode.NodeType.Index, node.Location);
						newNode.Branches.Add("arrayOrTable", left);
						newNode.Branches.Add("index", new ASTNode(ASTNode.NodeType.Str, right.Location, right.Branches["arrayOrTable"].Value));
						node = new ASTNode(ASTNode.NodeType.Index, node.Location, right.Value);
						node.Branches.Add("arrayOrTable", newNode);
						node.Branches.Add("index", right.Branches["index"]);
					}
					else
						throw new ExpectedNodeException(ASTNode.NodeType.Ident, right.Type, right.Location);
					break;
			}

			return node;
		}
	}
}

