using System;

namespace DrakeScript
{
	public class CompileException : Exception
	{
		public SourceRef Location;

		public CompileException(string message, SourceRef location) : base(message)
		{
			Location = location;
		}

		public override string ToString()
		{
			return string.Format("{0}: {1}\n{2}", Location, Message, StackTrace);
		}
	}

	public class UnrecognizedTokenException : CompileException
	{
		//string TokenString;
		public UnrecognizedTokenException(string tokenString, SourceRef location) :
			base("Unrecognized token \"" + tokenString + "\"", location)
		{
			//TokenString = tokenString;
		}
	}

	public class UnexpectedTokenException : CompileException
	{
		//string TokenString;
		public UnexpectedTokenException(string tokenString, SourceRef location) :
			base("Unexpected token \"" + tokenString + "\"", location)
		{
			//TokenString = tokenString;
		}
	}

	public class ExpectedTokenException : CompileException
	{
		public ExpectedTokenException(string tokenString, SourceRef location) :
		base("Expected token \"" + tokenString + "\"", location)
		{

		}
	}

	public class UnrecognizedTokenTypeException : CompileException
	{
		//Token.TokenType Type;
		public UnrecognizedTokenTypeException(Token.TokenType type, SourceRef location) :
			base("Unrecognized token \"" + type + "\"", location)
		{
			//Type = type;
		}
	}

	public class StringNotTerminatedException : CompileException
	{
		public StringNotTerminatedException(SourceRef location) :
			base("String not terminated", location)
		{

		}
	}

	public class UnmatchedTokenException : CompileException
	{
		public UnmatchedTokenException(Token.TokenType type, SourceRef location) :
			base("Unmatched token \"" + type + "\"", location)
		{

		}
	}

	public class MalformedNumberException : CompileException
	{
		public MalformedNumberException(SourceRef location) :
			base("Malformed number", location)
		{

		}
	}

	public class MissingSemicolonException : CompileException
	{
		public MissingSemicolonException(SourceRef location) :
			base("Missing semicolon", location)
		{

		}
	}

	public class MissingCommaException : CompileException
	{
		public MissingCommaException(SourceRef location) :
			base("Missing comma", location)
		{

		}
	}

	public class InvalidOperandsException : CompileException
	{
		public InvalidOperandsException(ASTNode.NodeType type, SourceRef location) :
			base("Invalid operand(s) for operator type \"" + type + "\"", location)
		{

		}
	}

	public class NoInfoForNodeException : CompileException
	{
		public NoInfoForNodeException(ASTNode.NodeType type, SourceRef location) :
		base("No information available for node \"" + type + "\"", location)
		{

		}
	}

	public class NoCodeGenerationForNodeException : CompileException
	{
		public NoCodeGenerationForNodeException(ASTNode.NodeType type, SourceRef location) :
			base("No code generation available for node type \"" + type + "\"", location)
		{

		}
	}

	public class ExpectedNodeException : CompileException
	{
		public ExpectedNodeException(ASTNode.NodeType expected, ASTNode.NodeType got, SourceRef location) :
		base("Expected \"" + expected + "\" but got \""+ got + "\"", location)
		{

		}
	}

	public class InvalidConditionException : CompileException
	{
		public InvalidConditionException(string statement, SourceRef location) :
			base("Invalid condition(s) for \"" + statement + "\" statement", location)
		{

		}
	}
}

