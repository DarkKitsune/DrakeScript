using System;
using System.Collections.Generic;

namespace DrakeScript
{
	public class Optimizer
	{
		public void Optimize(List<Instruction> code)
		{
			for (var i = 0; i < code.Count; i++)
			{
				var inst = code[i];
				Instruction next = Instruction.Nop;
				if (i + 1 < code.Count)
					next = code[i + 1];
				
				switch (inst.Type)
				{
					case (Instruction.InstructionType.LeaveScope):
						if (next.Type == Instruction.InstructionType.EnterScope)
						{
							code[i] = new Instruction(inst.Location, Instruction.InstructionType.ResetScope);
							code.RemoveAt(i + 1);
							FixJumps(code, i + 1, -1);
						}
						break;
				}
			}
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
							if (i + inst.Arg.Number > insertpos)
							{
								inst.Arg.Number += number;
								code[i] = inst;
							}
						}
						else{
							if (i + inst.Arg.Number < insertpos)
							{
								inst.Arg.Number -= number;
								code[i] = inst;
							}
						}
						break;
				}
			}
		}
	}
}

