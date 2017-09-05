using System;

namespace DrakeScript
{
	public struct Instruction
	{
		public enum InstructionType : byte
		{
			NewLoc,
			PushVar,
			PushNum,
			PushStr,
			PopVar,
			PopArgs,
			Call,
			Add,
			Sub,
			Div,
			Mul,
			Neg,
			Eq,
			NEq,
			Return,
			JumpEZ,
			Jump,
		}

		public InstructionType Type;
		public Value Arg;
		public SourceRef Location;

		public Instruction(SourceRef location, InstructionType type, Value arg = default(Value))
		{
			Location = location;
			Type = type;
			if (arg.IsUndefined)
				Arg = Value.Nil;
			else
				Arg = arg;
		}

		public override string ToString()
		{
			return string.Format("[{0}({1})]", Type, Arg.DynamicValue);
		}
	}
}

