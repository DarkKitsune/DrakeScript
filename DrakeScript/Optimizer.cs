using System;
using System.Collections.Generic;

namespace DrakeScript
{
	public class Optimizer
	{
		public bool LocalizeGlobalGets = true;

		public void Optimize(Function func)
		{
			var code = new List<Instruction>(func.Code);
			for (var i = 0; i < code.Count; i++)
			{
				var inst = code[i];
				var prev = i > 0 ? code[i - 1] : Instruction.Nop;
				var next = Instruction.Nop;
				if (i + 1 < code.Count)
					next = code[i + 1];

				switch (inst.Type)
				{
					
					case (Instruction.InstructionType.IncVarByLocal):
						if (prev.Type == Instruction.InstructionType.PushNum && prev.Arg.Number == 1)
						{
							code[i] = new Instruction(inst.Location, Instruction.InstructionType.IncVarLocal, inst.Arg);
							code.RemoveAt(i - 1);
							FixJumps(code, i - 1, -1);
							i--;
						}
						break;
					case (Instruction.InstructionType.IncVarByGlobal):
						if (prev.Type == Instruction.InstructionType.PushNum && prev.Arg.Number == 1)
						{
							code[i] = new Instruction(inst.Location, Instruction.InstructionType.IncVarGlobal, inst.Arg);
							code.RemoveAt(i - 1);
							FixJumps(code, i - 1, -1);
							i--;
						}
						break;
					case (Instruction.InstructionType.DecVarByLocal):
						if (prev.Type == Instruction.InstructionType.PushNum && prev.Arg.Number == 1)
						{
							code[i] = new Instruction(inst.Location, Instruction.InstructionType.DecVarLocal, inst.Arg);
							code.RemoveAt(i - 1);
							FixJumps(code, i - 1, -1);
							i--;
						}
						break;
					case (Instruction.InstructionType.DecVarByGlobal):
						if (prev.Type == Instruction.InstructionType.PushNum && prev.Arg.Number == 1)
						{
							code[i] = new Instruction(inst.Location, Instruction.InstructionType.DecVarGlobal, inst.Arg);
							code.RemoveAt(i - 1);
							FixJumps(code, i - 1, -1);
							i--;
						}
						break;
					case (Instruction.InstructionType.PopVarGlobal):
						if (prev.Type == Instruction.InstructionType.PushVarGlobal)
						{
							code.RemoveAt(i - 1);
							code.RemoveAt(i - 1);
							FixJumps(code, i - 1, -2);
							i -= 2;
						}
						break;
					case (Instruction.InstructionType.Eq):
						if (prev.Type == Instruction.InstructionType.PushNil)
						{
							code[i] = new Instruction(inst.Location, Instruction.InstructionType.EqNil);
							code.RemoveAt(i - 1);
							FixJumps(code, i - 1, -1);
							i--;
						}
						else if (prev.Type == Instruction.InstructionType.Push1 && next.Type == Instruction.InstructionType.JumpEZ)
						{
							code.RemoveAt(i - 1);
							code.RemoveAt(i - 1);
							FixJumps(code, i - 1, -2);
							i -= 2;
						}
						else if (prev.Type == Instruction.InstructionType.Push0 && next.Type == Instruction.InstructionType.JumpEZ)
						{
							code[i + 1] = new Instruction(code[i + 1].Location, Instruction.InstructionType.JumpNZ, code[i + 1].Arg);
							code.RemoveAt(i - 1);
							code.RemoveAt(i - 1);
							FixJumps(code, i - 1, -2);
							i -= 2;
						}
						break;
					case (Instruction.InstructionType.PushNum):
						if (inst.Arg.Number == 0.0)
						{
							code[i] = new Instruction(inst.Location, Instruction.InstructionType.Push0);
						}
						else if (inst.Arg.Number == 1.0)
						{
							code[i] = new Instruction(inst.Location, Instruction.InstructionType.Push1);
						}
						break;
				}
			}

			if (LocalizeGlobalGets)
			{
				//second pass: convert a lot of global gets to local gets
				for (var i = 0; i < code.Count; i++)
				{
					if (code[i].Type == Instruction.InstructionType.PushVarGlobal)
					{
						var end = NextPossibleGlobalChange(code, i, code[i].Arg.String);
						var gets = CountGlobalGets(code, i, end, code[i].Arg.String);
						if (gets > 2)
						{
							var locals = func.Locals;
							var locNum = locals.Length;
							Array.Resize(ref locals, locNum + 1);
							locals[locNum] = "__loc_" + code[i].Arg.String + "_" + i;
							func.Locals = locals;
							var loca = code[i].Location;
							var name = code[i].Arg.String;
							code.Insert(i, new Instruction(loca, Instruction.InstructionType.Dup));
							code.Insert(i, new Instruction(loca, Instruction.InstructionType.PushVarGlobal, name));
							i += 2;
							code[i] = new Instruction(loca, Instruction.InstructionType.PopVarLocal, Value.CreateInt(locNum));
							end += 2;
							FixJumps(code, i - 2, 2);
							for (var j = i; j < end; j++)
							{
								if (code[j].Type == Instruction.InstructionType.PushVarGlobal && code[j].Arg.String == name)
								{
									code[j] = new Instruction(code[j].Location, Instruction.InstructionType.PushVarLocal, Value.CreateInt(locNum));
								}
							}
						}
					}
				}
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

		int NextPossibleGlobalChange(List<Instruction> code, int pos, string name)
		{
			var i = pos;
			for (; i < code.Count; i++)
			{
				switch (code[i].Type)
				{
					case (Instruction.InstructionType.Call):
					case (Instruction.InstructionType.Jump):
					case (Instruction.InstructionType.JumpEZ):
					case (Instruction.InstructionType.JumpNZ):
					case (Instruction.InstructionType.PopVarGlobal):
					case (Instruction.InstructionType.IncVarGlobal):
					case (Instruction.InstructionType.IncVarByGlobal):
					case (Instruction.InstructionType.DecVarGlobal):
					case (Instruction.InstructionType.DecVarByGlobal):
					case (Instruction.InstructionType.Yield):
						switch (code[i].Type)
						{
							case (Instruction.InstructionType.PopVarGlobal):
							case (Instruction.InstructionType.IncVarGlobal):
							case (Instruction.InstructionType.IncVarByGlobal):
							case (Instruction.InstructionType.DecVarGlobal):
							case (Instruction.InstructionType.DecVarByGlobal):
								if (code[i].Arg.String == name)
									return i;
								break;
							case (Instruction.InstructionType.Jump):
							case (Instruction.InstructionType.JumpEZ):
							case (Instruction.InstructionType.JumpNZ):
								if (i + code[i].Arg.IntNumber + 1 > i || i + code[i].Arg.IntNumber + 1 < pos)
									return i;
								break;
							default:
								return i;
						}
						break;
				}
			}
			return i;
		}

		int CountGlobalGets(List<Instruction> code, int pos, int end, string name)
		{
			var num = 0;
			for (var i = pos; i < end; i++)
			{
				if (code[i].Type ==Instruction.InstructionType.PushVarGlobal && code[i].Arg.String == name)
					num++;
			}
			return num;
		}
	}
}

