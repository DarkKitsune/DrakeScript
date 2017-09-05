using System;
using System.Collections.Generic;

namespace DrakeScript
{
	public struct ASTNode
	{
		public static ASTNode Invalid = new ASTNode {Type = NodeType.Invalid, Branches = null, Value = null, Location = SourceRef.Invalid};

		public enum NodeType
		{
			Invalid,
			Root,
			Str,
			Int,
			Dec,
			Ident,
			Par,
			Call,
			Args,
			NewLocal,
			PlusOperator,
			MinusOperator,
			DivideOperator,
			MultiplyOperator,
			EqualsOperator,
			SetOperator,
			Add,
			Subtract,
			Divide,
			Multiply,
			Positive,
			Negative,
			Eq,
			Set,
		}

		public static Dictionary<NodeType, NodeInfo> NodeInfo = new Dictionary<NodeType, NodeInfo>()
		{
			{NodeType.Invalid, new NodeInfo(false)},
			{NodeType.Str, new NodeInfo(true)},
			{NodeType.Int, new NodeInfo(true)},
			{NodeType.Dec, new NodeInfo(true)},
			{NodeType.Ident, new NodeInfo(true)},
			{NodeType.Par, new NodeInfo(true)},
			{NodeType.Call, new NodeInfo(true)},
			{NodeType.Args, new NodeInfo(false)},
			{NodeType.NewLocal, new NodeInfo(true)},
			{NodeType.PlusOperator, new NodeInfo(false, true, true, NodeType.Add, 4, NodeType.Positive, 2)},
			{NodeType.MinusOperator, new NodeInfo(false, true, true, NodeType.Subtract, 4, NodeType.Negative, 2)},
			{NodeType.DivideOperator, new NodeInfo(false, true, false, NodeType.Divide, 3)},
			{NodeType.MultiplyOperator, new NodeInfo(false, true, false, NodeType.Multiply, 3)},
			{NodeType.EqualsOperator, new NodeInfo(false, true, false, NodeType.Eq, 7)},
			{NodeType.SetOperator, new NodeInfo(false, true, false, NodeType.Set, 14)},
			{NodeType.Add, new NodeInfo(true)},
			{NodeType.Subtract, new NodeInfo(true)},
			{NodeType.Divide, new NodeInfo(true)},
			{NodeType.Multiply, new NodeInfo(true)},
			{NodeType.Positive, new NodeInfo(true)},
			{NodeType.Negative, new NodeInfo(true)},
			{NodeType.Eq, new NodeInfo(true)},
			{NodeType.Set, new NodeInfo(false)},
		};

		public NodeType Type;
		public Dictionary<string, ASTNode> Branches;
		public object Value;
		public SourceRef Location;

		public ASTNode(NodeType type, SourceRef location, object value = null)
		{
			Type = type;
			Location = location;
			Value = value;
			Branches = new Dictionary<string, ASTNode>();
		}

		public string ToString(int indentation, bool justChildren = false)
		{
			if (Type == NodeType.Invalid)
				return "Invalid";
			var childrenStrBuilder = new System.Text.StringBuilder();
			if (!justChildren)
				childrenStrBuilder.Append('\n');
			foreach (var kv in Branches)
			{
				childrenStrBuilder.Append(new String(' ', indentation * 3));
				childrenStrBuilder.Append("\\__");
				childrenStrBuilder.Append(kv.Key + " = " + kv.Value.ToString(indentation + 1));
			}
			if (Value is List<ASTNode>)
			{
				var n = 0;
				foreach (var child in (List<ASTNode>)Value)
				{
					childrenStrBuilder.Append(new String(' ', indentation * 3));
					childrenStrBuilder.Append("\\__");
					childrenStrBuilder.Append((n++) + " = " + child.ToString(indentation + 1));
				}
			}
			if (!justChildren)
			{
				if (Value is List<ASTNode> || Value == null)
					return String.Format("{0}{1}", Type, childrenStrBuilder.ToString());
				else
					return String.Format("{0}({1})]{2}", Type, Value, childrenStrBuilder.ToString());
	
			}
			return String.Format("{0}", childrenStrBuilder.ToString());
		}

		public override string ToString()
		{
			return String.Format("tree[\n{0}]\n", ToString(1, true));
		}
	}
}

