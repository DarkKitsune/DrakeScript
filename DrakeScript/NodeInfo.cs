using System;

namespace DrakeScript
{
	public struct NodeInfo
	{
		public bool InfixOperator;
		public bool PrefixOperator;
		public bool HasValue;
		public ASTNode.NodeType InfixOperatorType;
		public ASTNode.NodeType PrefixOperatorType;
		public int InfixOperatorPrecedence;
		public int PrefixOperatorPrecedence;

		public NodeInfo(bool hasValue)
		{
			HasValue = hasValue;
			InfixOperator = false;
			PrefixOperator = false;
			InfixOperatorType = ASTNode.NodeType.Invalid;
			PrefixOperatorType = ASTNode.NodeType.Invalid;
			InfixOperatorPrecedence = -1;
			PrefixOperatorPrecedence = -1;
		}

		public NodeInfo(
			bool hasValue, bool infixOperator, bool prefixOperator, ASTNode.NodeType operatorType,
			int operatorPrecedence
		)
		{
			HasValue = hasValue;
			InfixOperator = infixOperator;
			PrefixOperator = prefixOperator;
			if (InfixOperator)
			{
				InfixOperatorType = operatorType;
				PrefixOperatorType = ASTNode.NodeType.Invalid;
				InfixOperatorPrecedence = operatorPrecedence;
				PrefixOperatorPrecedence = -1;
			}
			else if (PrefixOperator)
			{
				InfixOperatorType = ASTNode.NodeType.Invalid;
				PrefixOperatorType = operatorType;
				InfixOperatorPrecedence = -1;
				PrefixOperatorPrecedence = operatorPrecedence;
			}
			else
			{
				InfixOperatorType = ASTNode.NodeType.Invalid;
				PrefixOperatorType = ASTNode.NodeType.Invalid;
				InfixOperatorPrecedence = -1;
				PrefixOperatorPrecedence = -1;
			}
		}

		public NodeInfo(
			bool hasValue, bool infixOperator, bool prefixOperator, ASTNode.NodeType infixOperatorType,
			int infixOperatorPrecedence, ASTNode.NodeType prefixOperatorType, int prefixOperatorPrecedence
		)
		{
			HasValue = hasValue;
			InfixOperator = infixOperator;
			PrefixOperator = prefixOperator;
			InfixOperatorType = infixOperatorType;
			PrefixOperatorType = prefixOperatorType;
			InfixOperatorPrecedence = infixOperatorPrecedence;
			PrefixOperatorPrecedence = prefixOperatorPrecedence;
		}
	}
}

