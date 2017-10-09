using System;
using System.Collections.Generic;

namespace DrakeScript
{
	public class Optimizer
	{
		public void Optimize(Function func)
		{
			var code = new List<Instruction>(func.Code);
			var prev = Instruction.Nop;
			for (var i = 0; i < code.Count; i++)
			{
				var inst = code[i];
				Instruction next = Instruction.Nop;
				if (i + 1 < code.Count)
					next = code[i + 1];
				
				switch (inst.Type)
				{
					
					case (Instruction.InstructionType.IncVarByLocal):
						if (prev.Type == Instruction.InstructionType.PushNum & prev.Arg.Number == 1)
						{
							code[i] = new Instruction(inst.Location, Instruction.InstructionType.IncVarLocal, inst.Arg);
							code.RemoveAt(i - 1);
							FixJumps(code, i - 1, -1);
						}
						break;
					case (Instruction.InstructionType.IncVarByGlobal):
						if (prev.Type == Instruction.InstructionType.PushNum & prev.Arg.Number == 1)
						{
							code[i] = new Instruction(inst.Location, Instruction.InstructionType.IncVarGlobal, inst.Arg);
							code.RemoveAt(i - 1);
							FixJumps(code, i - 1, -1);
						}
						break;
					case (Instruction.InstructionType.DecVarByLocal):
						if (prev.Type == Instruction.InstructionType.PushNum & prev.Arg.Number == 1)
						{
							code[i] = new Instruction(inst.Location, Instruction.InstructionType.DecVarLocal, inst.Arg);
							code.RemoveAt(i - 1);
							FixJumps(code, i - 1, -1);
						}
						break;
					case (Instruction.InstructionType.DecVarByGlobal):
						if (prev.Type == Instruction.InstructionType.PushNum & prev.Arg.Number == 1)
						{
							code[i] = new Instruction(inst.Location, Instruction.InstructionType.DecVarGlobal, inst.Arg);
							code.RemoveAt(i - 1);
							FixJumps(code, i - 1, -1);
						}
						break;
				}

				prev = inst;
			}
			func.Code = code.ToArray();
		}

		void FixJumps(List<Instruction> code, int insertpos, int number)
		{
			for (var i = 0; i < code.Count; i++)
			{
				var inst = code[i];
				switch (inst.Type)
				{
					case (Instruction.InstructionType.Jump):
					case (Instruction.InstructionType.JumpEZ):
						if (i < insertpos)
						{
							if (i + inst.Arg.IntNumber > insertpos)
							{
								inst.Arg.IntNumber += number;
								code[i] = inst;
							}
						}
						else{
							if (i + inst.Arg.IntNumber < insertpos)
							{
								inst.Arg.IntNumber -= number;
								code[i] = inst;
							}
						}
						break;
				}
			}
		}
	}
}

