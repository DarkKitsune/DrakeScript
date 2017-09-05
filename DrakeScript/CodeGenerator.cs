using System;
using System.Collections.Generic;

namespace DrakeScript
{
	public class CodeGenerator
	{
		public Instruction[] Generate(ASTNode node)
		{
			var instructions = new List<Instruction>();

			switch (node.Type)
			{
				case (ASTNode.NodeType.Root):
					foreach (var child in (List<ASTNode>)node.Value)
					{
						instructions.AddRange(Generate(child));
					}
					break;
				case (ASTNode.NodeType.Int):
					instructions.Add(
						new Instruction(
							node.Location,
							Instruction.InstructionType.PushNum,
							Value.Create((double)(long)(node.Value))
						)
					);
					break;
				case (ASTNode.NodeType.Dec):
					instructions.Add(
						new Instruction(
							node.Location,
							Instruction.InstructionType.PushNum,
							Value.Create((double)(node.Value))
						)
					);
					break;
				case (ASTNode.NodeType.Str):
					instructions.Add(
						new Instruction(
							node.Location,
							Instruction.InstructionType.PushStr,
							Value.Create((string)(node.Value))
						)
					);
					break;
				case (ASTNode.NodeType.Ident):
					instructions.Add(
						new Instruction(
							node.Location,
							Instruction.InstructionType.PushVar,
							Value.Create((string)(node.Value))
						)
					);
					break;
				case (ASTNode.NodeType.Add):
					instructions.AddRange(Generate(node.Branches["left"]));
					instructions.AddRange(Generate(node.Branches["right"]));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Add));
					break;
				case (ASTNode.NodeType.Subtract):
					instructions.AddRange(Generate(node.Branches["left"]));
					instructions.AddRange(Generate(node.Branches["right"]));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Sub));
					break;
				case (ASTNode.NodeType.Divide):
					instructions.AddRange(Generate(node.Branches["left"]));
					instructions.AddRange(Generate(node.Branches["right"]));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Div));
					break;
				case (ASTNode.NodeType.Multiply):
					instructions.AddRange(Generate(node.Branches["left"]));
					instructions.AddRange(Generate(node.Branches["right"]));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Mul));
					break;
				case (ASTNode.NodeType.Negative):
					instructions.AddRange(Generate(node.Branches["right"]));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Neg));
					break;
				case (ASTNode.NodeType.Positive):
					instructions.AddRange(Generate(node.Branches["right"]));
					break;
				case (ASTNode.NodeType.Eq):
					instructions.AddRange(Generate(node.Branches["left"]));
					instructions.AddRange(Generate(node.Branches["right"]));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Eq));
					break;
				case (ASTNode.NodeType.NEq):
					instructions.AddRange(Generate(node.Branches["left"]));
					instructions.AddRange(Generate(node.Branches["right"]));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.NEq));
					break;
				case (ASTNode.NodeType.Set):
					if (node.Branches["left"].Type == ASTNode.NodeType.NewLocal)
					{
						instructions.Add(new Instruction(node.Location, Instruction.InstructionType.NewLoc, Value.Create((string)node.Branches["left"].Value)));
					}
					instructions.AddRange(Generate(node.Branches["right"]));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.PopVar, Value.Create((string)node.Branches["left"].Value)));
					break;
				case (ASTNode.NodeType.Call):
					var cargs = (List<ASTNode>)(node.Branches["args"].Value);
					foreach (var child in cargs)
					{
						instructions.AddRange(Generate(child));
					}
					instructions.Add(
						new Instruction(
							node.Location,
							Instruction.InstructionType.PopArgs,
							Value.Create(cargs.Count)
						)
					);
					instructions.AddRange(Generate(node.Branches["function"]));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Call));
					break;
				case (ASTNode.NodeType.Return):
					instructions.AddRange(Generate((ASTNode)node.Value));
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Return));
					break;
				case (ASTNode.NodeType.If):
					var ifCond = (List<ASTNode>)node.Branches["condition"].Value;
					if (ifCond.Count != 1)
						throw new InvalidConditionException("if", node.Location);
					instructions.AddRange(Generate(ifCond[0]));
					var ifJumpInstPos = instructions.Count;
					foreach (var child in (List<ASTNode>)node.Value)
					{
						instructions.AddRange(Generate(child));
					}
					var ifJumpDestPos = instructions.Count + 2;
					instructions.Insert(ifJumpInstPos, new Instruction(node.Location, Instruction.InstructionType.JumpEZ, Value.Create(ifJumpDestPos - ifJumpInstPos)));
					break;
				case (ASTNode.NodeType.While):
					var whileCond = (List<ASTNode>)node.Branches["condition"].Value;
					if (whileCond.Count != 1)
						throw new InvalidConditionException("while", node.Location);
					var whileStart = instructions.Count;
					instructions.AddRange(Generate(whileCond[0]));
					var whileJumpInstPos = instructions.Count;
					foreach (var child in (List<ASTNode>)node.Value)
					{
						instructions.AddRange(Generate(child));
					}
					instructions.Add(new Instruction(node.Location, Instruction.InstructionType.Jump, Value.Create(whileStart - instructions.Count - 2)));
					var whileJumpDestPos = instructions.Count + 3;
					instructions.Insert(whileJumpInstPos, new Instruction(node.Location, Instruction.InstructionType.JumpEZ, Value.Create(instructions.Count - whileJumpInstPos)));
					break;
				default:
					throw new NoCodeGenerationForNodeException(node.Type, node.Location);
			}
			
			return instructions.ToArray();
		}
	}
}

