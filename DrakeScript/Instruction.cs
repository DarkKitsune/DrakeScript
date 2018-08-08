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
			_Break = 13,
			Call = 14,
			PushIndex = 15,
			PopIndex = 16,
			Add = 17,
			Sub = 18,
			Div = 19,
			Mul = 20,
			Mod = 21,
			Pow = 22,
			Cat = 23,
			Not = 24,
			Neg = 25,
			Eq = 26,
			NEq = 27,
			Gt = 28,
			GtEq = 29,
			Lt = 30,
			LtEq = 31,
			IncVarGlobal = 32,
			IncVarLocal = 33,
			DecVarGlobal = 34,
			DecVarLocal = 35,
			IncVarByGlobal = 36,
			IncVarByLocal = 37,
			DecVarByGlobal = 38,
			DecVarByLocal = 39,
			Dec = 40,
			Dup = 41,
			Return = 42,
			Yield = 43,
			JumpEZ = 44,
			JumpNZ = 45,
			Jump = 46,
			Is = 47,
			Contains = 48,
			EqNil = 49,
			Push0 = 50,
			Push1 = 51,
			Length = 52,
			Swap = 53,
            PushArrayI = 54,
            PopArg = 55,
            ReturnNil = 56,
            Return0 = 57,
            Return1 = 58,
            Inc = 59,
            SEq = 60,
            NewThread = 61,
            PushIndexInt = 62,
            PushIndexStr = 63,
            PushMethod = 64
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

