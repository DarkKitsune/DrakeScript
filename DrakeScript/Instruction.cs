using System;
using System.IO;

namespace DrakeScript
{
	public struct Instruction
	{
		public static Instruction Nop = new Instruction {Type = InstructionType.Nop, Arg = Value.Nil, Location = SourceRef.Invalid};

		public enum InstructionType : short
		{
			Nop,
			PushVarGlobal,
			PushVarLocal,
			PushNum,
			PushStr,
			PushArray,
			PushFunc,
			PushNil,
			PushArg,
			Pop,
			PopVarGlobal,
			PopVarLocal,
			PopArgs,
			Call,
			PushIndex,
			PopIndex,
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


		public byte[] GetBytes()
		{
			using (var memoryStream = new MemoryStream())
			{
				using (var writer = new BinaryWriter(memoryStream))
				{
					writer.Write((short)Type);
					writer.Write(Arg.GetBytes());
					writer.Write(Location.Line);
					writer.Write(Location.Column);
				}
				return memoryStream.ToArray();
			}
		}

		public static Instruction FromReader(Context context, BinaryReader reader, Source source)
		{
			var type = (InstructionType)reader.ReadInt16();
			var arg = Value.FromReader(context, reader);
			var line = reader.ReadInt32();
			var column = reader.ReadInt32();
			return new Instruction(new SourceRef(source, line, column), type, arg);
		}
	}
}

