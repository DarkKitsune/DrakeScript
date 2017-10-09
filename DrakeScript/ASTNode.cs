﻿using System;
using System.Collections.Generic;

namespace DrakeScript
{
	public struct ASTNode
	{
		public static ASTNode Invalid = new ASTNode {Type = NodeType.Invalid, Branches = new Dictionary<string, ASTNode>{}, Value = null, Location = SourceRef.Invalid};

		public enum NodeType
		{
			Invalid,
			Root,
			Str,
			Int,
			Dec,
			Nil,
			Array,
			Table,
			Ident,
			Par,
			Call,
			Index,
			Args,
			Condition,
			NewLocal,
			PlusOperator,
			MinusOperator,
			DivideOperator,
			MultiplyOperator,
			PairOperator,
			DotIndexOperator,
			ConcatOperator,
			NotOperator,
			EqualsOperator,
			NotEqualsOperator,
			GtOperator,
			GtEqOperator,
			LtOperator,
			LtEqOperator,
			SetOperator,
			PlusEqOperator,
			MinusEqOperator,
			Add,
			Subtract,
			Divide,
			Multiply,
			Concat,
			Pair,
			DotIndex,
			Not,
			Positive,
			Negative,
			Eq,
			NEq,
			Gt,
			GtEq,
			Lt,
			LtEq,
			Set,
			PlusEq,
			MinusEq,
			PReturn,
			Return,
			Yield,
			If,
			Else,
			While,
			Loop,
			Function,
		}

		public static Dictionary<NodeType, NodeInfo> NodeInfo = new Dictionary<NodeType, NodeInfo>()
		{
			{NodeType.Invalid, new NodeInfo(false)},
			{NodeType.Str, new NodeInfo(true)},
			{NodeType.Int, new NodeInfo(true)},
			{NodeType.Dec, new NodeInfo(true)},
			{NodeType.Nil, new NodeInfo(true)},
			{NodeType.Array, new NodeInfo(true)},
			{NodeType.Table, new NodeInfo(true)},
			{NodeType.Ident, new NodeInfo(true)},
			{NodeType.Par, new NodeInfo(true)},
			{NodeType.Call, new NodeInfo(true)},
			{NodeType.Index, new NodeInfo(true)},
			{NodeType.Args, new NodeInfo(false)},
			{NodeType.Condition, new NodeInfo(false)},
			{NodeType.NewLocal, new NodeInfo(true)},
			{NodeType.PlusOperator, new NodeInfo(false, true, true, NodeType.Add, 4, NodeType.Positive, 2)},
			{NodeType.MinusOperator, new NodeInfo(false, true, true, NodeType.Subtract, 4, NodeType.Negative, 2)},
			{NodeType.DivideOperator, new NodeInfo(false, true, false, NodeType.Divide, 3)},
			{NodeType.MultiplyOperator, new NodeInfo(false, true, false, NodeType.Multiply, 3)},
			{NodeType.ConcatOperator, new NodeInfo(false, true, false, NodeType.Concat, 4)},
			{NodeType.PairOperator, new NodeInfo(false, true, false, NodeType.Pair, 15)},
			{NodeType.DotIndexOperator, new NodeInfo(false, true, false, NodeType.DotIndex, 1)},
			{NodeType.NotOperator, new NodeInfo(false, false, true, NodeType.Not, 2)},
			{NodeType.EqualsOperator, new NodeInfo(false, true, false, NodeType.Eq, 7)},
			{NodeType.NotEqualsOperator, new NodeInfo(false, true, false, NodeType.NEq, 7)},
			{NodeType.GtOperator, new NodeInfo(false, true, false, NodeType.Gt, 6)},
			{NodeType.GtEqOperator, new NodeInfo(false, true, false, NodeType.GtEq, 6)},
			{NodeType.LtOperator, new NodeInfo(false, true, false, NodeType.Lt, 6)},
			{NodeType.LtEqOperator, new NodeInfo(false, true, false, NodeType.LtEq, 6)},
			{NodeType.SetOperator, new NodeInfo(false, true, false, NodeType.Set, 14)},
			{NodeType.PlusEqOperator, new NodeInfo(false, true, false, NodeType.PlusEq, 14)},
			{NodeType.MinusEqOperator, new NodeInfo(false, true, false, NodeType.MinusEq, 14)},
			{NodeType.Add, new NodeInfo(true)},
			{NodeType.Subtract, new NodeInfo(true)},
			{NodeType.Divide, new NodeInfo(true)},
			{NodeType.Multiply, new NodeInfo(true)},
			{NodeType.Concat, new NodeInfo(true)},
			{NodeType.Pair, new NodeInfo(true)},
			{NodeType.DotIndex, new NodeInfo(true)},
			{NodeType.Not, new NodeInfo(true)},
			{NodeType.Positive, new NodeInfo(true)},
			{NodeType.Negative, new NodeInfo(true)},
			{NodeType.Eq, new NodeInfo(true)},
			{NodeType.NEq, new NodeInfo(true)},
			{NodeType.Gt, new NodeInfo(true)},
			{NodeType.GtEq, new NodeInfo(true)},
			{NodeType.Lt, new NodeInfo(true)},
			{NodeType.LtEq, new NodeInfo(true)},
			{NodeType.Set, new NodeInfo(false)},
			{NodeType.PlusEq, new NodeInfo(false)},
			{NodeType.MinusEq, new NodeInfo(false)},
			{NodeType.Return, new NodeInfo(false)},
			{NodeType.Yield, new NodeInfo(false)},
			{NodeType.If, new NodeInfo(false)},
			{NodeType.Else, new NodeInfo(false)},
			{NodeType.While, new NodeInfo(false)},
			{NodeType.Loop, new NodeInfo(false)},
			{NodeType.Function, new NodeInfo(true)},
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
				else if (Value is ASTNode)
					return String.Format("{0}:{1}{2}", Type, ((ASTNode)Value).ToString(1), childrenStrBuilder.ToString());
				else
					return String.Format("{0}:{1}{2}", Type, Value, childrenStrBuilder.ToString());
	
			}
			return String.Format("{0}", childrenStrBuilder.ToString());
		}

		public override string ToString()
		{
			return String.Format("tree[\n{0}]\n", ToString(1, true));
		}
	}
}

