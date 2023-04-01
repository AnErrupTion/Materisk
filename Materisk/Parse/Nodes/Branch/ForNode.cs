﻿using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using MateriskLLVM;

namespace Materisk.Parse.Nodes.Branch;

internal class ForNode : SyntaxNode
{
    private readonly SyntaxNode _initialExpressionNode;
    private readonly SyntaxNode _condNode;
    private readonly SyntaxNode _stepNode;
    private readonly SyntaxNode _block;

    public ForNode(SyntaxNode initialExpressionNode, SyntaxNode condNode, SyntaxNode stepNode, SyntaxNode block)
    {
        _initialExpressionNode = initialExpressionNode;
        _condNode = condNode;
        _stepNode = stepNode;
        _block = block;
    }

    public override NodeType Type => NodeType.For;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        _initialExpressionNode.Emit(variables, module, type, method, arguments);

        var index = method.CilMethodBody!.Instructions.Count;
        var condLabel = new CilInstructionLabel();
        var stepLabel = new CilInstructionLabel();
        var shouldBreak = false;
        var breakIndex = 0;

        method.CilMethodBody!.Instructions.Add(CilOpCodes.Br, condLabel);

        _stepNode.Emit(variables, module, type, method, arguments);
        stepLabel.Instruction = method.CilMethodBody!.Instructions[index + 1];

        index = method.CilMethodBody!.Instructions.Count;
        _block.Emit(variables, module, type, method, arguments);

        for (var i = index; i < method.CilMethodBody!.Instructions.Count; i++)
        {
            var instruction = method.CilMethodBody!.Instructions[i];

            if (instruction.OpCode == CilOpCodes.Break) // Break
            {
                shouldBreak = true;
                breakIndex = i;
            }
            else if (instruction.OpCode == CilOpCodes.Leave) // Continue
            {
                method.CilMethodBody!.Instructions[i] = new(CilOpCodes.Br, stepLabel);
            }
        }

        index = method.CilMethodBody!.Instructions.Count;
        _condNode.Emit(variables, module, type, method, arguments);
        condLabel.Instruction = method.CilMethodBody!.Instructions[index];

        method.CilMethodBody!.Instructions.Add(CilOpCodes.Brtrue, stepLabel);

        if (shouldBreak)
        {
            method.CilMethodBody!.Instructions.Add(CilOpCodes.Nop);
            var breakLabel = new CilInstructionLabel(method.CilMethodBody!.Instructions.Last());
            method.CilMethodBody!.Instructions[breakIndex] = new(CilOpCodes.Br, breakLabel);
        }

        return null!;
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return _initialExpressionNode;
        yield return _condNode;
        yield return _stepNode;
        yield return _block;
    }
}