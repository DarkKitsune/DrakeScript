using System;

namespace DrakeScript
{
	public struct Instruction
	{
		public static Instruction Nop = new Instruction {Type = InstructionType.Nop, Arg = Value.Nil, Location = SourceRef.Invalid};

		public enum InstructionType : byte
		{
			Nop,
			NewLoc,
			PushVar,
			PushNum,
			PushStr,
			Pop,
			PopVar,
			PopArgs,
			Call,
			Add,
			Sub,
			Div,
			Mul,
			Not,
			Neg,
			Eq,
			NEq,
			Gt,
			GtEq,
			Lt,
			LtEq,
			IncVarBy,
			DecVarBy,
			Dec,
			Dup,
			Return,
			JumpEZ,
			Jump,
			EnterScope,
			LeaveScope,
			ResetScope,
		}

		public InstructionType Type;
		public Value Arg;
		public SourceRef Location;

		public Instruction(SourceRef location, InstructionType type, Value arg = default(Value))
		{
			Location = location;
			Type = type;
			if (arg.IsNil)
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

