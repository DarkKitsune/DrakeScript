using System;

namespace DrakeScript
{
	public class InterpreterException : Exception
	{
		public SourceRef Location;

		public InterpreterException(string message, SourceRef location) : base(message)
		{
			Location = location;
		}

		public override string ToString()
		{
			return string.Format("{0}: {1}\n{2}", Location, Message, StackTrace);
		}
	}

	public class UnexpectedLeftOperandException : InterpreterException
	{
		public UnexpectedLeftOperandException(
			ValueType got, ValueType expected, SourceRef location
		) : base("Illegal operation", location)
		{
			
		}
	}

	public class CannotCallNilException : InterpreterException
	{
		public CannotCallNilException(
			SourceRef location
		) : base("Function does not exist", location)
		{

		}
	}
}

