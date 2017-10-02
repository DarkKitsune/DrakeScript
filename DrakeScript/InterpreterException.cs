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

	public class NotEnoughArgumentsException : InterpreterException
	{
		public NotEnoughArgumentsException(
			int expected, int got,
			SourceRef location
		) : base("Function expected " + expected + " arguments but got " + got, location)
		{

		}
	}

	public class NoCaseForInstructionException : InterpreterException
	{
		public NoCaseForInstructionException(
			Instruction.InstructionType type,
			SourceRef location
		) : base("Interpreter does not have a case for instruction " + type, location)
		{

		}
	}

	public class InvalidIndexTypeException : InterpreterException
	{
		public InvalidIndexTypeException(
			string arrayOrTableType,
			Value.ValueType type,
			SourceRef location
		) : base(arrayOrTableType + " cannot be indexed with type \"" + type + "\"", location)
		{
			
		}
	}

	public class InvalidIndexValueException : InterpreterException
	{
		public InvalidIndexValueException(
			string arrayOrTableType,
			object index,
			SourceRef location
		) : base("Index \"" + index + "\" does not exist in " + arrayOrTableType, location)
		{

		}
	}

	public class BelowZeroIndexValueException : InterpreterException
	{
		public BelowZeroIndexValueException(
			string arrayOrTableType,
			int index,
			SourceRef location
		) : base("Index \"" + index + "\" is below 0 in " + arrayOrTableType, location)
		{

		}
	}

	public class CannotIndexTypeException : InterpreterException
	{
		public CannotIndexTypeException(
			Value.ValueType type,
			SourceRef location
		) : base("Cannot index type \"" + type + "\"", location)
		{

		}
	}

	public class UnexpectedTypeException : InterpreterException
	{
		public UnexpectedTypeException(
			Value.ValueType type,
			SourceRef location
		) : base("Unexpected type \"" + type + "\"", location)
		{

		}

		public UnexpectedTypeException(
			Value.ValueType got,
			Value.ValueType expected,
			SourceRef location
		) : base("Expected type \"" + expected + "\" but got \"" + got + "\"", location)
		{

		}
	}
}

