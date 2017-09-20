using System;
using System.Collections.Generic;
using System.Linq;

namespace DrakeScript
{
	public class Analyzer
	{
		public ASTNode Analyze(ASTNode node)
		{
			var keys = node.Branches.Keys.ToArray();
			for (var i = 0; i < keys.Length; i++)
			{
				node.Branches[keys[i]] = Analyze(node.Branches[keys[i]]);
			}

			if (node.Value is List<ASTNode>)
			{
				var list = (List<ASTNode>)node.Value;
				for (var i = 0; i < list.Count; i++)
				{
					list[i] = Analyze(list[i]);
				}
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
			}

			return node;
		}
	}
}

