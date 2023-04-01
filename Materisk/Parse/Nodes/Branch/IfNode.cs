﻿using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using LLVMSharp.Interop;
using MateriskLLVM;

namespace Materisk.Parse.Nodes.Branch;

internal class IfNode : SyntaxNode
{
    private readonly SyntaxNode _conditionNode;
    private readonly SyntaxNode _blockNode;
    private readonly SyntaxNode? _elseBlockNode;
    
    public IfNode(SyntaxNode conditionNode, SyntaxNode blockNode, SyntaxNode? elseBlockNode = null)
    {
        _conditionNode = conditionNode;
        _blockNode = blockNode;
        _elseBlockNode = elseBlockNode;
    }

    public override NodeType Type => NodeType.If;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        _conditionNode.Emit(variables, module, type, method, arguments);

        var label = new CilInstructionLabel();
        method.CilMethodBody!.Instructions.Add(CilOpCodes.Brfalse, label);
        _blockNode.Emit(variables, module, type, method, arguments);
        method.CilMethodBody!.Instructions.Add(CilOpCodes.Nop);
        label.Instruction = method.CilMethodBody!.Instructions.Last();

        return null!;
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method)
    {
        var value = (LLVMValueRef)_conditionNode.Emit(module, type, method);

        if (_elseBlockNode is null)
        {
            var thenBlock = method.LlvmMethod.AppendBasicBlock("then");
            var nextBlock = method.LlvmMethod.AppendBasicBlock("next");
            var conditionValue = module.LlvmBuilder.BuildCondBr(value, thenBlock, nextBlock);

            module.LlvmBuilder.PositionAtEnd(thenBlock);
            _blockNode.Emit(module, type, method);
            module.LlvmBuilder.BuildBr(nextBlock);

            module.LlvmBuilder.PositionAtEnd(nextBlock);

            return conditionValue;
        }
        else
        {
            var thenBlock = method.LlvmMethod.AppendBasicBlock("then");
            var elseBlock = method.LlvmMethod.AppendBasicBlock("else");
            var nextBlock = method.LlvmMethod.AppendBasicBlock("next");
            var conditionValue = module.LlvmBuilder.BuildCondBr(value, thenBlock, elseBlock);

            module.LlvmBuilder.PositionAtEnd(thenBlock);
            _blockNode.Emit(module, type, method);
            module.LlvmBuilder.BuildBr(nextBlock);

            module.LlvmBuilder.PositionAtEnd(elseBlock);
            _elseBlockNode.Emit(module, type, method);
            module.LlvmBuilder.BuildBr(nextBlock);

            module.LlvmBuilder.PositionAtEnd(nextBlock);

            return conditionValue;
        }
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return _conditionNode;
        yield return _blockNode;
        if (_elseBlockNode is not null) yield return _elseBlockNode;
    }

    public override string ToString()
    {
        return "IfNode:";
    }
}