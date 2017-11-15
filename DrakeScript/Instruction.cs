using System;
using System.IO;
using System.Collections.Generic;

namespace DrakeScript
{
	public struct Instruction
	{
		public static Instruction Nop = new Instruction {Type = InstructionType.Nop, Arg = Value.Nil, Location = SourceRef.Invalid};

		public enum InstructionType : short
		{
			Nop = 0,
			PushVarGlobal = 1,
			PushVarLocal = 2,
			PushNum = 3,
			PushStr = 4,
			PushArray = 5,
			PushTable = 6,
			PushFunc = 7,
			PushNil = 8,
			PushArg = 9,
			Pop = 10,
			PopVarGlobal = 11,
			PopVarLocal = 12,
			Call = 13,
			PushIndex = 14,
			PopIndex = 15,
			Add = 16,
			Sub = 17,
			Div = 18,
			Mul = 19,
			Mod = 20,
			Pow = 21,
			Cat = 22,
			Not = 23,
			Neg = 24,
			Eq = 25,
			NEq = 26,
			Gt = 27,
			GtEq = 28,
			Lt = 29,
			LtEq = 30,
			IncVarGlobal = 31,
			IncVarLocal = 32,
			DecVarGlobal = 33,
			DecVarLocal = 34,
			IncVarByGlobal = 35,
			IncVarByLocal = 36,
			DecVarByGlobal = 37,
			DecVarByLocal = 38,
			Dec = 39,
			Dup = 40,
			Return = 41,
			Yield = 42,
			JumpEZ = 43,
			JumpNZ = 44,
			Jump = 45,
			Is = 46,
			Contains = 47,
			EqNil = 48,
			Push0 = 49,
			Push1 = 50,
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


		public byte[] GetBytes(Context context)
		{
			using (var memoryStream = new MemoryStream())
			{
				using (var writer = new BinaryWriter(memoryStream))
				{
					writer.Write((short)Type);
					writer.Write(Arg.GetBytes(context));
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

