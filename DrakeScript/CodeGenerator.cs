using System;
using System.Collections.Generic;

namespace DrakeScript
{
	public class CodeGenerator
	{
		string[] Args;
		Dictionary<string, int> ArgLookup = new Dictionary<string, int>();
		List<string> Locals = new List<string>();

		public Function Generate(ASTNode node, string[] args = null)
		{
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
			List<Instruction> before;
			var code = Generate(node, false, out before);
			code.Add(new Instruction(node.Location, Instruction.InstructionType.PushNil));
			code.Add(new Instruction(node.Location, Instruction.InstructionType.Return));
			return new Function(code.ToArray(), Args, Locals.ToArray());
		}

		public List<Instruction> Generate(ASTNode node, bool allowPush, out List<Instruction> insert, bool allowConditions = false)
		{
			insert = new List<Instruction>();
			var instructions = new List<Instruction>();

			List<Instruction> range;
			List<Instruction> before;
			int locNum;
			switch (node.Type)
			{
				case (ASTNode.NodeType.Root):
					foreach (var child in (List<ASTNode>)node.Value)
					{
						range = Generate(child, false, out before, true);
						if (before.Count > 0)
							instructions.InsertRange(instructions.Count - 1, before);
						instructions.AddRange(range);
					}
					break;
				case (ASTNode.NodeType.Par):
					if (!allowPush)
						throw new UnexpectedTokenException("(...)", node.Location);
					foreach (var child in (List<ASTNode>)node.Value)
					{
						range = Generate(child, true, out before, false);
						if (before.Count > 0)
							instructions.InsertRange(instructions.Count - 1, before);
						instructions.AddRange(range);
					}
					break;
				case (ASTNode.NodeType.Int):
					if (!allowPush)
						throw new UnexpectedTokenException(node.Type + "(" + node.Value.ToString() + ")", node.Location);
					instructions.Add(
						new Instruction(
							node.Location,
							Instruction.InstructionType.PushNum,
							Value.Create((double)(long)(node.Value))
						)
					);
					break;
				case (ASTNode.NodeType.Dec):
					if (!allowPush)
						throw new UnexpectedTokenException(node.Type + "(" + node.Value.ToString() + ")", node.Location);
					instructions.Add(
						new Instruction(
							node.Location,
							Instruction.InstructionType.PushNum,
							Value.Create((double)(node.Value))
						)
					);
					break;
				case (ASTNode.NodeType.Str):
					if (!allowPush)
						throw new UnexpectedTokenException(node.Type + "(" + node.Value.ToString() + ")", node.Location);
					instructions.Add(
						new Instruction(
							node.Location,
							Instruction.InstructionType.PushStr,
							Value.Create((string)(node.Value))
						)
					);
					break;
				case (ASTNode.NodeType.Ident):
					if (!allowPush)
						throw new UnexpectedTokenException(node.Type + "(" + node.Value.ToString() + ")", node.Location);
					int argNum;
					if (ArgLookup.TryGetValue((string)node.Value, out argNum))
					{
						instructions.Add(
							new Instruction(
								node.Location,
								Instruction.InstructionType.PushArg,
								Value.Create(argNum)
							)
						);
					}
					else
					{
						locNum = Locals.IndexOf((string)node.Value);
						if (locNum >= 0)
						{
							instructions.Add(
								new Instruction(
									node.Location,
									Instruction.InstructionType.PushVarLocal,
									Value.Create(locNum)
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
				case (ASTNode.NodeType.Add):
					instructions.AddRange(Generate(node.Branches["left"], true, out before));
					instructions.AddRange(Generate(node.Branches["right"], true, out before));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Add));
					break;
				case (ASTNode.NodeType.Subtract):
					instructions.AddRange(Generate(node.Branches["left"], true, out before));
					instructions.AddRange(Generate(node.Branches["right"], true, out before));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Sub));
					break;
				case (ASTNode.NodeType.Divide):
					instructions.AddRange(Generate(node.Branches["left"], true, out before));
					instructions.AddRange(Generate(node.Branches["right"], true, out before));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Div));
					break;
				case (ASTNode.NodeType.Multiply):
					instructions.AddRange(Generate(node.Branches["left"], true, out before));
					instructions.AddRange(Generate(node.Branches["right"], true, out before));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Mul));
					break;
				case (ASTNode.NodeType.Not):
					instructions.AddRange(Generate(node.Branches["right"], true, out before));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Not));
					break;
				case (ASTNode.NodeType.Negative):
					instructions.AddRange(Generate(node.Branches["right"], true, out before));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Neg));
					break;
				case (ASTNode.NodeType.Positive):
					instructions.AddRange(Generate(node.Branches["right"], true, out before));
					break;
				case (ASTNode.NodeType.Eq):
					instructions.AddRange(Generate(node.Branches["left"], true, out before));
					instructions.AddRange(Generate(node.Branches["right"], true, out before));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Eq));
					break;
				case (ASTNode.NodeType.NEq):
					instructions.AddRange(Generate(node.Branches["left"], true, out before));
					instructions.AddRange(Generate(node.Branches["right"], true, out before));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.NEq));
					break;
				case (ASTNode.NodeType.Gt):
					instructions.AddRange(Generate(node.Branches["left"], true, out before));
					instructions.AddRange(Generate(node.Branches["right"], true, out before));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Gt));
					break;
				case (ASTNode.NodeType.GtEq):
					instructions.AddRange(Generate(node.Branches["left"], true, out before));
					instructions.AddRange(Generate(node.Branches["right"], true, out before));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.GtEq));
					break;
				case (ASTNode.NodeType.Lt):
					instructions.AddRange(Generate(node.Branches["left"], true, out before));
					instructions.AddRange(Generate(node.Branches["right"], true, out before));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Lt));
					break;
				case (ASTNode.NodeType.LtEq):
					instructions.AddRange(Generate(node.Branches["left"], true, out before));
					instructions.AddRange(Generate(node.Branches["right"], true, out before));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.LtEq));
					break;
				case (ASTNode.NodeType.NewLocal):
					Locals.Add((string)node.Value);
					break;
				case (ASTNode.NodeType.Set):
					if (node.Branches["left"].Type == ASTNode.NodeType.NewLocal)
					{
						Locals.Add((string)node.Branches["left"].Value);
					}

					instructions.AddRange(Generate(node.Branches["right"], true, out before));

					locNum = Locals.IndexOf((string)node.Branches["left"].Value);
					if (locNum >= 0)
					{
						instructions.Add(new Instruction(node.Location, Instruction.InstructionType.PopVarLocal, Value.Create(locNum)));
					}
					else
					{
						instructions.Add(new Instruction(node.Location, Instruction.InstructionType.PopVarGlobal, Value.Create((string)node.Branches["left"].Value)));
					}
					break;
				case (ASTNode.NodeType.PlusEq):
					instructions.AddRange(Generate(node.Branches["right"], true, out before));

					locNum = Locals.IndexOf((string)node.Branches["left"].Value);
					if (locNum >= 0)
					{
						instructions.Add(new Instruction(node.Location, Instruction.InstructionType.IncVarByLocal, Value.Create(locNum)));
					}
					else
					{
						instructions.Add(new Instruction(node.Location, Instruction.InstructionType.IncVarByGlobal, Value.Create((string)node.Branches["left"].Value)));
					}
					break;
				case (ASTNode.NodeType.MinusEq):
					instructions.AddRange(Generate(node.Branches["right"], true, out before));

					locNum = Locals.IndexOf((string)node.Branches["left"].Value);
					if (locNum >= 0)
					{
						instructions.Add(new Instruction(node.Location, Instruction.InstructionType.DecVarByLocal, Value.Create(locNum)));
					}
					else
					{
						instructions.Add(new Instruction(node.Location, Instruction.InstructionType.DecVarByGlobal, Value.Create((string)node.Branches["left"].Value)));
					}
					break;
				case (ASTNode.NodeType.Call):
					var cargs = (List<ASTNode>)(node.Branches["args"].Value);
					foreach (var child in cargs)
					{
						range = Generate(child, true, out before);
						if (before.Count > 0)
							instructions.InsertRange(instructions.Count - 1, before);
						instructions.AddRange(range);
					}
					instructions.Add(
						new Instruction(
							node.Location,
							Instruction.InstructionType.PopArgs,
							Value.Create(cargs.Count)
						)
					);
					instructions.AddRange(Generate(node.Branches["function"], true, out before));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Call));
					if (!allowPush)
						instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Pop));
					break;
				case (ASTNode.NodeType.Return):
					if (allowPush)
						throw new UnexpectedTokenException("return", node.Location);
					range = Generate((ASTNode)node.Value, true, out before);
					if (before.Count > 0)
						instructions.InsertRange(instructions.Count - 1, before);
					instructions.AddRange(range);
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Return));
					break;
				case (ASTNode.NodeType.If):
					if (!allowConditions)
						throw new UnexpectedTokenException("if", node.Location);
					var ifCond = (List<ASTNode>)node.Branches["condition"].Value;
					if (ifCond.Count != 1)
						throw new InvalidConditionException("if", node.Location);
					instructions.AddRange(Generate(ifCond[0], true, out before));
					var ifJumpInstPos = instructions.Count;
					//instructions.Add(new Instruction(node.Location, Instruction.InstructionType.EnterScope));
					foreach (var child in (List<ASTNode>)node.Value)
					{
						range = Generate(child, false, out before, true);
						if (before.Count > 0)
							instructions.InsertRange(instructions.Count - 1, before);
						instructions.AddRange(range);
					}
					//instructions.Add(new Instruction(node.Location, Instruction.InstructionType.LeaveScope));
					var ifJumpDestPos = instructions.Count + 2;
					instructions.Insert(ifJumpInstPos, new Instruction(node.Location, Instruction.InstructionType.JumpEZ, Value.Create(ifJumpDestPos - ifJumpInstPos - 2)));
					break;
				case (ASTNode.NodeType.Else):
					//insert.Add(new Instruction(node.Location, Instruction.InstructionType.EnterScope));
					foreach (var child in (List<ASTNode>)node.Value)
					{
						range = Generate(child, false, out before, true);
						if (before.Count > 0)
							instructions.InsertRange(instructions.Count - 1, before);
						insert.AddRange(range);
					}
					insert.Insert(0, new Instruction(node.Location, Instruction.InstructionType.Jump, Value.Create(insert.Count)));

					break;
				case (ASTNode.NodeType.While):
					var whileCond = (List<ASTNode>)node.Branches["condition"].Value;
					if (whileCond.Count != 1)
						throw new InvalidConditionException("while", node.Location);
					//instructions.Add(new Instruction(node.Location, Instruction.InstructionType.EnterScope));
					var whileStart = instructions.Count;
					instructions.AddRange(Generate(whileCond[0], true, out before));
					var whileJumpInstPos = instructions.Count;
					foreach (var child in (List<ASTNode>)node.Value)
					{
						range = Generate(child, false, out before, true);
						if (before.Count > 0)
							instructions.InsertRange(instructions.Count - 1, before);
						instructions.AddRange(range);
					}
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Jump, Value.Create(whileStart - instructions.Count - 2)));
					instructions.Insert(whileJumpInstPos, new Instruction(node.Location, Instruction.InstructionType.JumpEZ, Value.Create(instructions.Count - whileJumpInstPos)));
					//instructions.Add(new Instruction(node.Location, Instruction.InstructionType.LeaveScope));
					break;
				case (ASTNode.NodeType.Loop):
					var loopCond = (List<ASTNode>)node.Branches["condition"].Value;
					if (loopCond.Count != 1)
						throw new InvalidConditionException("loop", node.Location);
					//instructions.Add(new Instruction(node.Location, Instruction.InstructionType.EnterScope));
					var loopStart = instructions.Count;
					instructions.AddRange(Generate(loopCond[0], true, out before));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Dup));
					var loopJumpInstPos = instructions.Count;
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Dec));
					foreach (var child in (List<ASTNode>)node.Value)
					{
						range = Generate(child, false, out before, true);
						if (before.Count > 0)
							instructions.InsertRange(instructions.Count - 1, before);
						instructions.AddRange(range);
					}
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Jump, Value.Create(loopStart - instructions.Count - 1)));
					instructions.Insert(loopJumpInstPos, new Instruction(node.Location, Instruction.InstructionType.JumpEZ, Value.Create(instructions.Count - loopJumpInstPos)));
					//instructions.Add(new Instruction(node.Location, Instruction.InstructionType.LeaveScope));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Pop));
					break;
				case (ASTNode.NodeType.Function):
					var funcName = (string)node.Branches["functionName"].Value;
					if (funcName.Length == 0)
					{
						if (!allowPush)
							throw new UnexpectedTokenException("function", node.Location);

					}
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
					var generator = new CodeGenerator();
					var newFunc = generator.Generate((ASTNode)node.Value, argsStrings);
					instructions.Add(
						new Instruction(
							node.Location,
							Instruction.InstructionType.PushFunc,
							Value.Create(newFunc)
						)
					);
					if (funcName.Length > 0)
					{
						locNum = Locals.IndexOf(funcName);
						if (locNum >= 0)
						{
							instructions.Add(
								new Instruction(
									node.Location,
									Instruction.InstructionType.PopVarLocal,
									Value.Create(locNum)
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
				default:
					throw new NoCodeGenerationForNodeException(node.Type, node.Location);
			}

			return instructions;
		}
	}
}

