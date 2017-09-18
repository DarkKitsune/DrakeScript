using System;

namespace DrakeScript
{
	public struct Instruction
	{
		public static Instruction Nop = new Instruction {Type = InstructionType.Nop, Arg = Value.Nil, Location = SourceRef.Invalid};

		public enum InstructionType : byte
		{
			Nop,
			PushVarGlobal,
			PushVarLocal,
			PushNum,
			PushStr,
			PushFunc,
			PushNil,
			PushArg,
			Pop,
			PopVarGlobal,
			PopVarLocal,
			PopArgs,
			Call,
			Add,
			Sub,
			Div,
			Mul,
			Cat,
			Not,
			Neg,
			Eq,
			NEq,
			Gt,
			GtEq,
			Lt,
			LtEq,
			IncVarGlobal,
			IncVarLocal,
			DecVarGlobal,
			DecVarLocal,
			IncVarByGlobal,
			IncVarByLocal,
			DecVarByGlobal,
			DecVarByLocal,
			Dec,
			Dup,
			Return,
			JumpEZ,
			Jump
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
			return string.Format("[{0}({1})]", Type, Arg);
		}
	}
}

