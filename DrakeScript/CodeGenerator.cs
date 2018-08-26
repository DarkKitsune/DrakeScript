using System;
using System.Collections.Generic;

namespace DrakeScript
{
	public class CodeGenerator
	{
        public const int LocalsPerFunction = 0x01000000;

        internal CodeGenerator Parent = null;
        Function Func;
        Function[] Parents;
		string[] Args;
		Dictionary<string, int> ArgLookup = new Dictionary<string, int>();
		internal List<string> Locals = new List<string>();
		public bool UnrollLoops = true;
		public int MaxUnrollBytes = 4096;
		public Context Context;

		public CodeGenerator(Context context)
		{
			Context = context;
		}

		public Function Generate(SourceRef location, ASTNode node, string[] args = null, CodeGenerator parent = null, Function[] parents = null)
		{
            Parent = parent;
			if (args == null)
				Args = new string[] {};
			else
			{
				Args = args;
				var n = 0;
				foreach (var arg in Args)
				{
					ArgLookup[arg] = n++;
				}
			}
            if (parents == null)
                parents = new Function[] { };
            Parents = parents;
            var func = new Function(location, Context, Args, parents);
            Func = func;
            var code = Generate(node, false, false, parents);
            VerifyCode(code);
            code.Add(new Instruction(node.Location, Instruction.InstructionType.ReturnNil));
            func.Code = code.ToArray();
            func.Locals = Locals.ToArray();
            return func;
		}

		void VerifyCode(List<Instruction> code)
		{
			foreach (var inst in code)
			{
				if (inst.Type == Instruction.InstructionType._Break)
					throw new UnexpectedTokenException("break", inst.Location);
			}
		}

		public List<Instruction> Generate(ASTNode node, bool requirePush, bool allowConditions = false, Function[] parents = null)
		{
            if (parents == null)
                parents = new Function[] { };

            var instructions = new List<Instruction>();

			List<Instruction> range;
			int locNum;
			switch (node.Type)
			{
				case (ASTNode.NodeType.Root):
					foreach (var child in (List<ASTNode>)node.Value)
					{
						range = Generate(child, false, true);
						instructions.AddRange(range);
					}
					break;
				case (ASTNode.NodeType.Par):
					if (!requirePush)
						throw new UnexpectedTokenException("(...)", node.Location);
					foreach (var child in (List<ASTNode>)node.Value)
					{
						range = Generate(child, true, false);
						instructions.AddRange(range);
					}
					break;
				case (ASTNode.NodeType.Int):
					if (!requirePush)
						throw new UnexpectedTokenException(node.Type.ToString(), node.Location);
					instructions.Add(
						new Instruction(
							node.Location,
							Instruction.InstructionType.PushNum,
							Value.Create((double)(node.Value))
						)
					);
					break;
				case (ASTNode.NodeType.Dec):
					if (!requirePush)
						throw new UnexpectedTokenException(node.Type.ToString(), node.Location);
					instructions.Add(
						new Instruction(
							node.Location,
							Instruction.InstructionType.PushNum,
							Value.Create((double)(node.Value))
						)
					);
					break;
				case (ASTNode.NodeType.Nil):
					if (!requirePush)
						throw new UnexpectedTokenException("nil", node.Location);
					instructions.Add(
						new Instruction(
							node.Location,
							Instruction.InstructionType.PushNil
						)
					);
					break;
				case (ASTNode.NodeType.Str):
					if (!requirePush)
						throw new UnexpectedTokenException(node.Type.ToString(), node.Location);
					instructions.Add(
						new Instruction(
							node.Location,
							Instruction.InstructionType.PushStr,
							Value.Create((string)(node.Value))
						)
					);
					break;
				case (ASTNode.NodeType.Array):
					if (!requirePush)
						throw new UnexpectedTokenException("[...]", node.Location);
                    var tinst = new List<Instruction>();
					foreach (var child in (List<ASTNode>)node.Value)
					{
						range = Generate(child, true, false);
                        tinst.AddRange(range);
					}
                    var potentialArray = new List<Value>();
                    var immediate = true;
                    foreach (var inst in tinst)
                    {
                        if (inst.Type == Instruction.InstructionType.Push0 || inst.Type == Instruction.InstructionType.Push1)
                            potentialArray.Add((double)inst.Arg);
                        else if (inst.Type == Instruction.InstructionType.PushFunc)
                            potentialArray.Add((Function)inst.Arg);
                        else if (inst.Type == Instruction.InstructionType.PushNum)
                            potentialArray.Add((double)inst.Arg);
                        else if (inst.Type == Instruction.InstructionType.PushStr)
                            potentialArray.Add((string)inst.Arg);
                        else
                        {
                            immediate = false;
                            break;
                        }
                    }
                    if (immediate)
                        instructions.Add(
                            new Instruction(
                                node.Location,
                                Instruction.InstructionType.PushArrayI,
                                potentialArray
                            )
                        );
                    else
                    {
                        instructions.AddRange(tinst);
					    instructions.Add(
    						new Instruction(
    							node.Location,
    							Instruction.InstructionType.PushArray,
    							((List<ASTNode>)node.Value).Count
    						)
    					);
                    }
					break;
				case (ASTNode.NodeType.Table):
					if (!requirePush)
						throw new UnexpectedTokenException("{...}", node.Location);
					instructions.Add(
						new Instruction(
							node.Location,
							Instruction.InstructionType.PushTable
						)
					);
					foreach (var child in (List<ASTNode>)node.Value)
					{
						instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Dup));
						range = Generate(child.Branches["left"], true);
						instructions.AddRange(range);
						range = Generate(child.Branches["right"], true);
						instructions.AddRange(range);
						instructions.Add(new Instruction(node.Location, Instruction.InstructionType.PopIndex));
					}
					break;
				case (ASTNode.NodeType.Ident):
					if (!requirePush)
						throw new UnexpectedTokenException(node.Type.ToString(), node.Location);
					int argNum;
					if (ArgLookup.TryGetValue((string)node.Value, out argNum))
					{
						instructions.Add(
							new Instruction(
								node.Location,
								Instruction.InstructionType.PushArg,
								argNum
							)
						);
					}
					else
					{
                        locNum = GetLocalIndex((string)node.Value);//Locals.IndexOf((string)node.Value);
						if (locNum >= 0)
						{
							instructions.Add(
								new Instruction(
									node.Location,
									Instruction.InstructionType.PushVarLocal,
									locNum
								)
							);
						}
						else
						{
							instructions.Add(
								new Instruction(
									node.Location,
									Instruction.InstructionType.PushVarGlobal,
									Value.Create((string)(node.Value))
								)
							);
						}
					}
					break;
				case (ASTNode.NodeType.Is):
					if (!requirePush)
						throw new UnexpectedTokenException(node.Type.ToString(), node.Location);
					instructions.AddRange(Generate(node.Branches["left"], true));
					instructions.AddRange(Generate(node.Branches["right"], true));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Is));
					break;
				case (ASTNode.NodeType.Then):
					if (!requirePush)
						throw new UnexpectedTokenException(node.Type.ToString(), node.Location);
					instructions.AddRange(Generate(node.Branches["left"], true));
					var thenJumpStart = instructions.Count;
					instructions.AddRange(Generate(node.Branches["right"], true));
					var thenJumpAmount = instructions.Count - thenJumpStart + 1;
					instructions.Insert(thenJumpStart, new Instruction(node.Location, Instruction.InstructionType.JumpEZ, thenJumpAmount));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Jump, 1));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.PushNil));
					break;
				case (ASTNode.NodeType.Otherwise):
					if (!requirePush)
						throw new UnexpectedTokenException(node.Type.ToString(), node.Location);
					instructions.AddRange(Generate(node.Branches["left"], true));
					instructions.Add(new Instruction(node.Branches["left"].Location, Instruction.InstructionType.Dup));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.PushNil));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Eq));
					var otherwiseJumpStart = instructions.Count;
					instructions.Add(new Instruction(node.Branches["right"].Location, Instruction.InstructionType.Pop));
					instructions.AddRange(Generate(node.Branches["right"], true));
					var otherwiseJumpAmount = instructions.Count - otherwiseJumpStart + 1;
					instructions.Insert(otherwiseJumpStart, new Instruction(node.Location, Instruction.InstructionType.JumpEZ, otherwiseJumpAmount - 1));
					break;
				case (ASTNode.NodeType.Contains):
					if (!requirePush)
						throw new UnexpectedTokenException(node.Type.ToString(), node.Location);
					instructions.AddRange(Generate(node.Branches["left"], true));
					instructions.AddRange(Generate(node.Branches["right"], true));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Contains));
					break;
				case (ASTNode.NodeType.Length):
					if (!requirePush)
						throw new UnexpectedTokenException(node.Type.ToString(), node.Location);
					instructions.AddRange(Generate(node.Branches["right"], true));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Length));
					break;
				case (ASTNode.NodeType.Add):
					if (!requirePush)
						throw new UnexpectedTokenException(node.Type.ToString(), node.Location);
					instructions.AddRange(Generate(node.Branches["left"], true));
					instructions.AddRange(Generate(node.Branches["right"], true));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Add));
					break;
				case (ASTNode.NodeType.Subtract):
					if (!requirePush)
						throw new UnexpectedTokenException(node.Type.ToString(), node.Location);
					instructions.AddRange(Generate(node.Branches["left"], true));
					instructions.AddRange(Generate(node.Branches["right"], true));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Sub));
					break;
				case (ASTNode.NodeType.Divide):
					if (!requirePush)
						throw new UnexpectedTokenException(node.Type.ToString(), node.Location);
					instructions.AddRange(Generate(node.Branches["left"], true));
					instructions.AddRange(Generate(node.Branches["right"], true));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Div));
					break;
				case (ASTNode.NodeType.Multiply):
					if (!requirePush)
						throw new UnexpectedTokenException(node.Type.ToString(), node.Location);
					instructions.AddRange(Generate(node.Branches["left"], true));
					instructions.AddRange(Generate(node.Branches["right"], true));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Mul));
					break;
				case (ASTNode.NodeType.Modulo):
					if (!requirePush)
						throw new UnexpectedTokenException(node.Type.ToString(), node.Location);
					instructions.AddRange(Generate(node.Branches["left"], true));
					instructions.AddRange(Generate(node.Branches["right"], true));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Mod));
					break;
				case (ASTNode.NodeType.Power):
					if (!requirePush)
						throw new UnexpectedTokenException(node.Type.ToString(), node.Location);
					instructions.AddRange(Generate(node.Branches["left"], true));
					instructions.AddRange(Generate(node.Branches["right"], true));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Pow));
					break;
				case (ASTNode.NodeType.Concat):
					if (!requirePush)
						throw new UnexpectedTokenException(node.Type.ToString(), node.Location);
					instructions.AddRange(Generate(node.Branches["left"], true));
					instructions.AddRange(Generate(node.Branches["right"], true));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Cat));
					break;
				case (ASTNode.NodeType.Not):
					if (!requirePush)
						throw new UnexpectedTokenException(node.Type.ToString(), node.Location);
					instructions.AddRange(Generate(node.Branches["right"], true));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Not));
					break;
                case (ASTNode.NodeType.Thread):
                    instructions.AddRange(Generate(node.Branches["function"], true));
                    var cargs = (List<ASTNode>)(node.Branches["args"].Value);
                    foreach (var child in cargs)
                    {
                        range = Generate(child, true);
                        instructions.AddRange(range);
                    }
                    instructions.Add(new Instruction(node.Location, Instruction.InstructionType.NewThread, cargs.Count));
                    if (!requirePush)
                        instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Pop));
                    break;
                case (ASTNode.NodeType.Negative):
					if (!requirePush)
						throw new UnexpectedTokenException(node.Type.ToString(), node.Location);
					instructions.AddRange(Generate(node.Branches["right"], true));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Neg));
					break;
				case (ASTNode.NodeType.Positive):
					if (!requirePush)
						throw new UnexpectedTokenException(node.Type.ToString(), node.Location);
					instructions.AddRange(Generate(node.Branches["right"], true));
					break;
				case (ASTNode.NodeType.Eq):
					if (!requirePush)
						throw new UnexpectedTokenException(node.Type.ToString(), node.Location);
					instructions.AddRange(Generate(node.Branches["left"], true));
					instructions.AddRange(Generate(node.Branches["right"], true));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Eq));
					break;
                case (ASTNode.NodeType.SEq):
                    if (!requirePush)
                        throw new UnexpectedTokenException(node.Type.ToString(), node.Location);
                    instructions.AddRange(Generate(node.Branches["left"], true));
                    instructions.AddRange(Generate(node.Branches["right"], true));
                    instructions.Add(new Instruction(node.Location, Instruction.InstructionType.SEq));
                    break;
                case (ASTNode.NodeType.NEq):
					if (!requirePush)
						throw new UnexpectedTokenException(node.Type.ToString(), node.Location);
					instructions.AddRange(Generate(node.Branches["left"], true));
					instructions.AddRange(Generate(node.Branches["right"], true));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.NEq));
					break;
				case (ASTNode.NodeType.Gt):
					if (!requirePush)
						throw new UnexpectedTokenException(node.Type.ToString(), node.Location);
					instructions.AddRange(Generate(node.Branches["left"], true));
					instructions.AddRange(Generate(node.Branches["right"], true));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Gt));
					break;
				case (ASTNode.NodeType.GtEq):
					if (!requirePush)
						throw new UnexpectedTokenException(node.Type.ToString(), node.Location);
					instructions.AddRange(Generate(node.Branches["left"], true));
					instructions.AddRange(Generate(node.Branches["right"], true));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.GtEq));
					break;
				case (ASTNode.NodeType.Lt):
					if (!requirePush)
						throw new UnexpectedTokenException(node.Type.ToString(), node.Location);
					instructions.AddRange(Generate(node.Branches["left"], true));
					instructions.AddRange(Generate(node.Branches["right"], true));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Lt));
					break;
				case (ASTNode.NodeType.LtEq):
					if (!requirePush)
						throw new UnexpectedTokenException(node.Type.ToString(), node.Location);
					instructions.AddRange(Generate(node.Branches["left"], true));
					instructions.AddRange(Generate(node.Branches["right"], true));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.LtEq));
					break;
				case (ASTNode.NodeType.Or):
					if (!requirePush)
						throw new UnexpectedTokenException(node.Type.ToString(), node.Location);
					instructions.AddRange(Generate(node.Branches["left"], true));
					var orJumpStart = instructions.Count;
					instructions.AddRange(Generate(node.Branches["right"], true));
					var orJumpAmount = instructions.Count - orJumpStart + 1;
                    instructions.Insert(orJumpStart, new Instruction(node.Location, Instruction.InstructionType.Pop));
                    instructions.Insert(orJumpStart, new Instruction(node.Location, Instruction.InstructionType.JumpNZ, orJumpAmount));
					instructions.Insert(orJumpStart, new Instruction(node.Location, Instruction.InstructionType.Dup));
					break;
				case (ASTNode.NodeType.And):
					if (!requirePush)
						throw new UnexpectedTokenException(node.Type.ToString(), node.Location);
					instructions.AddRange(Generate(node.Branches["left"], true));
					var andJumpStart = instructions.Count;
					instructions.AddRange(Generate(node.Branches["right"], true));
					var andJumpAmount = instructions.Count - andJumpStart + 1;
                    instructions.Insert(andJumpStart, new Instruction(node.Location, Instruction.InstructionType.Pop));
                    instructions.Insert(andJumpStart, new Instruction(node.Location, Instruction.InstructionType.JumpEZ, andJumpAmount));
					instructions.Insert(andJumpStart, new Instruction(node.Location, Instruction.InstructionType.Dup));
					break;
				case (ASTNode.NodeType.NewLocal):
					if (requirePush)
						throw new UnexpectedTokenException(node.Type.ToString(), node.Location);
                    if (!Locals.Contains((string)node.Value))
					    Locals.Add((string)node.Value);
					break;
				case (ASTNode.NodeType.Set):
					if (requirePush)
						throw new UnexpectedTokenException(node.Type.ToString(), node.Location);
					if (node.Branches["left"].Type == ASTNode.NodeType.NewLocal)
					{
                        if (!Locals.Contains((string)node.Branches["left"].Value))
                            Locals.Add((string)node.Branches["left"].Value);
					}

					

					if (node.Branches["left"].Type == ASTNode.NodeType.Index)
					{
						instructions.AddRange(Generate(node.Branches["left"].Branches["arrayOrTable"], true));
						instructions.AddRange(Generate(node.Branches["left"].Branches["index"], true));
						instructions.AddRange(Generate(node.Branches["right"], true));
						instructions.Add(new Instruction(node.Location, Instruction.InstructionType.PopIndex));
					}
					else
					{
						instructions.AddRange(Generate(node.Branches["right"], true));
						locNum = GetLocalIndex((string)node.Branches["left"].Value);//Locals.IndexOf((string)node.Branches["left"].Value);
                        if (locNum >= 0)
						{
							instructions.Add(new Instruction(node.Location, Instruction.InstructionType.PopVarLocal, locNum));
						}
						else
						{
                            if (ArgLookup.TryGetValue((string)node.Branches["left"].Value, out argNum))
                            {
                                instructions.Add(new Instruction(node.Location, Instruction.InstructionType.PopArg, argNum));
                            }
                            else
                            {
                                instructions.Add(new Instruction(node.Location, Instruction.InstructionType.PopVarGlobal, Value.Create((string)node.Branches["left"].Value)));
                            }
                        }
					}
					break;
				case (ASTNode.NodeType.Index):
					if (!requirePush)
						throw new UnexpectedTokenException(node.Type.ToString(), node.Location);
					instructions.AddRange(Generate(node.Branches["arrayOrTable"], true));
                    if (node.Branches["index"].Type == ASTNode.NodeType.Str)
                    {
                        instructions.Add(new Instruction(node.Location, Instruction.InstructionType.PushIndexStr, (string)node.Branches["index"].Value));
                    }
                    else if (node.Branches["index"].Type == ASTNode.NodeType.Int)
                    {
                        instructions.Add(new Instruction(node.Location, Instruction.InstructionType.PushIndexInt, (double)node.Branches["index"].Value));
                    }
                    else
                    {
                        instructions.AddRange(Generate(node.Branches["index"], true));
                        instructions.Add(new Instruction(node.Location, Instruction.InstructionType.PushIndex));
                    }
					break;
				case (ASTNode.NodeType.PlusEq):
					if (requirePush)
						throw new UnexpectedTokenException(node.Type.ToString(), node.Location);
					instructions.AddRange(Generate(node.Branches["right"], true));

					locNum = GetLocalIndex((string)node.Branches["left"].Value);//Locals.IndexOf((string)node.Branches["left"].Value);
                    if (locNum >= 0)
					{
						instructions.Add(new Instruction(node.Location, Instruction.InstructionType.IncVarByLocal, locNum));
					}
					else
					{
						instructions.Add(new Instruction(node.Location, Instruction.InstructionType.IncVarByGlobal, Value.Create((string)node.Branches["left"].Value)));
					}
					break;
				case (ASTNode.NodeType.MinusEq):
					if (requirePush)
						throw new UnexpectedTokenException(node.Type.ToString(), node.Location);
					instructions.AddRange(Generate(node.Branches["right"], true));

					locNum = GetLocalIndex((string)node.Branches["left"].Value);//Locals.IndexOf((string)node.Branches["left"].Value);
                    if (locNum >= 0)
					{
						instructions.Add(new Instruction(node.Location, Instruction.InstructionType.DecVarByLocal, locNum));
					}
					else
					{
						instructions.Add(new Instruction(node.Location, Instruction.InstructionType.DecVarByGlobal, Value.Create((string)node.Branches["left"].Value)));
					}
					break;
				case (ASTNode.NodeType.Call):
					instructions.AddRange(Generate(node.Branches["function"], true));
					cargs = (List<ASTNode>)(node.Branches["args"].Value);
					foreach (var child in cargs)
					{
						range = Generate(child, true);
						instructions.AddRange(range);
					}
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Call, cargs.Count + (int)node.Branches["additionalArgs"].Value));
					if (!requirePush)
						instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Pop));
					break;
				case (ASTNode.NodeType.MethodCall):
					instructions.AddRange(Generate(node.Branches["arrayOrTable"], true));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Dup));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.PushMethod, Value.Create((string)node.Branches["index"].Value)));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Swap));
					cargs = (List<ASTNode>)(node.Branches["args"].Value);
					foreach (var child in cargs)
					{
						range = Generate(child, true);
						instructions.AddRange(range);
					}
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Call, cargs.Count + (int)node.Branches["additionalArgs"].Value));
					if (!requirePush)
						instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Pop));
					break;
				case (ASTNode.NodeType.Return):
					if (requirePush)
						throw new UnexpectedTokenException("return", node.Location);
					range = Generate((ASTNode)node.Value, true);
					instructions.AddRange(range);
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Return));
					break;
				case (ASTNode.NodeType.Yield):
					if (requirePush)
						throw new UnexpectedTokenException("yield", node.Location);
					range = Generate((ASTNode)node.Value, true);
					instructions.AddRange(range);
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Yield));
					break;
				case (ASTNode.NodeType.Break):
					if (requirePush)
						throw new UnexpectedTokenException("break", node.Location);
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType._Break));
					break;
				case (ASTNode.NodeType.If):
					if (requirePush)
						throw new UnexpectedTokenException(node.Type.ToString(), node.Location);
					if (!allowConditions)
						throw new UnexpectedTokenException("if", node.Location);
					var ifCond = (List<ASTNode>)node.Branches["condition"].Value;
					if (ifCond.Count != 1)
						throw new InvalidConditionException("if", node.Location);
					instructions.AddRange(Generate(ifCond[0], true));
					var ifJumpInstPos = instructions.Count;
					//instructions.Add(new Instruction(node.Location, Instruction.InstructionType.EnterScope));
					foreach (var child in (List<ASTNode>)node.Value)
					{
						range = Generate(child, false, true);
						instructions.AddRange(range);
					}
					//instructions.Add(new Instruction(node.Location, Instruction.InstructionType.LeaveScope));
					if (node.Branches.ContainsKey("else"))
					{
						var tempInst = new List<Instruction>();
						foreach (var child in (List<ASTNode>)node.Branches["else"].Value)
						{
							range = Generate(child, false, true);
							tempInst.AddRange(range);
						}
						instructions.Add(new Instruction(node.Branches["else"].Location, Instruction.InstructionType.Jump, tempInst.Count));
						var ifJumpDestPos = instructions.Count + 2;
						instructions.Insert(ifJumpInstPos, new Instruction(node.Location, Instruction.InstructionType.JumpEZ, ifJumpDestPos - ifJumpInstPos - 2));
						instructions.AddRange(tempInst);
					}
					else
					{
						var ifJumpDestPos = instructions.Count + 2;
						instructions.Insert(ifJumpInstPos, new Instruction(node.Location, Instruction.InstructionType.JumpEZ, ifJumpDestPos - ifJumpInstPos - 2));
					}
					break;
				case (ASTNode.NodeType.While):
					if (requirePush)
						throw new UnexpectedTokenException(node.Type.ToString(), node.Location);
					var whileCond = (List<ASTNode>)node.Branches["condition"].Value;
					if (whileCond.Count != 1)
						throw new InvalidConditionException("while", node.Location);
					//instructions.Add(new Instruction(node.Location, Instruction.InstructionType.EnterScope));
					var whileStart = instructions.Count;
					instructions.AddRange(Generate(whileCond[0], true));
					var whileJumpInstPos = instructions.Count;
					var whileBreakConvertStart = instructions.Count;
					foreach (var child in (List<ASTNode>)node.Value)
					{
						range = Generate(child, false, true);
						instructions.AddRange(range);
					}
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Jump, whileStart - instructions.Count - 2));
					instructions.Insert(whileJumpInstPos, new Instruction(node.Location, Instruction.InstructionType.JumpEZ, instructions.Count - whileJumpInstPos));
					for (var whileBreakInd = whileBreakConvertStart; whileBreakInd < instructions.Count; whileBreakInd++)
					{
						if (instructions[whileBreakInd].Type == Instruction.InstructionType._Break)
						{
							instructions[whileBreakInd] = new Instruction(instructions[whileBreakInd].Location, Instruction.InstructionType.Jump, instructions.Count - whileBreakInd - 1);
						}
					}
					//instructions.Add(new Instruction(node.Location, Instruction.InstructionType.LeaveScope));
					break;
				case (ASTNode.NodeType.Loop):
					if (requirePush)
						throw new UnexpectedTokenException(node.Type.ToString(), node.Location);
					var loopCond = (List<ASTNode>)node.Branches["condition"].Value;
					if (loopCond.Count != 1)
						throw new InvalidConditionException("loop", node.Location);
					var unrolled = false;
					var loopBreakConvertStart = instructions.Count;
					if (UnrollLoops && loopCond[0].Type == ASTNode.NodeType.Int)
					{
						var loopNum = (double)loopCond[0].Value;
						var tempInst = new List<Instruction>();
						foreach (var child in (List<ASTNode>)node.Value)
						{
							range = Generate(child, false, true);
							tempInst.AddRange(range);
						}
						if (loopNum * tempInst.Count <= MaxUnrollBytes / Value.MinSize)
						{
							for (var urn = 0; urn < loopNum; urn++)
							{
								instructions.AddRange(tempInst);
							}
							for (var loopBreakInd = loopBreakConvertStart; loopBreakInd < instructions.Count; loopBreakInd++)
							{
								if (instructions[loopBreakInd].Type == Instruction.InstructionType._Break)
								{
									instructions[loopBreakInd] = new Instruction(instructions[loopBreakInd].Location, Instruction.InstructionType.Jump, instructions.Count - loopBreakInd - 1);
								}
							}
							unrolled = true;
						}
					}
					if (!unrolled)
					{
						//instructions.Add(new Instruction(node.Location, Instruction.InstructionType.EnterScope));
						instructions.AddRange(Generate(loopCond[0], true));
						var loopStart = instructions.Count;
						instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Dup));
						var loopJumpInstPos = instructions.Count;
						instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Dec));
						foreach (var child in (List<ASTNode>)node.Value)
						{
							range = Generate(child, false, true);
							instructions.AddRange(range);
						}
						instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Jump, loopStart - instructions.Count - 2));
						instructions.Insert(loopJumpInstPos, new Instruction(node.Location, Instruction.InstructionType.JumpEZ, instructions.Count - loopJumpInstPos));
						//instructions.Add(new Instruction(node.Location, Instruction.InstructionType.LeaveScope));
						for (var loopBreakInd = loopBreakConvertStart; loopBreakInd < instructions.Count; loopBreakInd++)
						{
							if (instructions[loopBreakInd].Type == Instruction.InstructionType._Break)
							{
								instructions[loopBreakInd] = new Instruction(instructions[loopBreakInd].Location, Instruction.InstructionType.Jump, instructions.Count - loopBreakInd - 1);
							}
						}
						instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Pop));
					}
					break;
				case (ASTNode.NodeType.Function):
					var funcName = (string)node.Branches["functionName"].Value;
					if (funcName.Length == 0)
					{
						if (!requirePush)
							throw new UnexpectedTokenException("function", node.Location);
					}
					else if (requirePush)
							throw new UnexpectedTokenException(funcName, node.Location);
					
					var funcArgs = (List<ASTNode>)(node.Branches["args"].Value);
					var argsStrings = new string[funcArgs.Count];
					var argN = 0;
					foreach (var child in funcArgs)
					{
						if (child.Type != ASTNode.NodeType.Ident)
						{
							throw new ExpectedNodeException(ASTNode.NodeType.Ident, child.Type, child.Location);
						}
						argsStrings[argN++] = (string)child.Value;
					}
					var generator = new CodeGenerator(Context);
                    var newFuncParents = new List<Function>(Parents);
                    newFuncParents.Insert(0, Func);
					var newFunc = generator.Generate(node.Location, (ASTNode)node.Value, argsStrings, this, newFuncParents.ToArray());
					instructions.Add(
						new Instruction(
							node.Location,
							Instruction.InstructionType.PushFunc,
							Value.Create(newFunc)
						)
					);
					if (funcName.Length > 0)
					{
						locNum = GetLocalIndex(funcName);
                        if (locNum >= 0)
						{
							instructions.Add(
								new Instruction(
									node.Location,
									Instruction.InstructionType.PopVarLocal,
									locNum
								)
							);
						}
						else
						{
							instructions.Add(
								new Instruction(
									node.Location,
									Instruction.InstructionType.PopVarGlobal,
									Value.Create(funcName)
								)
							);
						}
					}
					break;
				case (ASTNode.NodeType.Invalid):
					if (requirePush)
						instructions.Add(new Instruction(node.Location, Instruction.InstructionType.PushNil));
					else
						throw new NoCodeGenerationForNodeException(node.Type, node.Location);
					break;
				default:
					throw new NoCodeGenerationForNodeException(node.Type, node.Location);
			}

			if (requirePush & instructions.Count == 0)
				instructions.Add(new Instruction(node.Location, Instruction.InstructionType.PushNil));

			return instructions;
		}

        int GetLocalIndex(string name)
        {
            var gen = this;
            var level = 0;
            
            while (true)
            {
                var ind = gen.Locals.IndexOf(name);
                if (ind != -1)
                    return level * LocalsPerFunction + ind;
                if (gen.Parent == null)
                    return -1;
                gen = gen.Parent;
                level++;
            }
        }
	}
}

