﻿using LLVMSharp.Interop;
using Materisk.TypeSystem;

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

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var ifBlock = method.LlvmMethod.AppendBasicBlock("if");
        var thenBlock = method.LlvmMethod.AppendBasicBlock("then");
        var elseBlock = method.LlvmMethod.AppendBasicBlock("else");

        _initialExpressionNode.Emit(module, type, method, metadata);
        module.LlvmBuilder.BuildBr(ifBlock);

        module.LlvmBuilder.PositionAtEnd(ifBlock);
        var ifValue = _condNode.Emit(module, type, method, metadata).Load();
        var conditionValue = module.LlvmBuilder.BuildCondBr(ifValue, thenBlock, elseBlock);

        module.LlvmBuilder.PositionAtEnd(thenBlock);
        _stepNode.Emit(module, type, method, metadata);
        metadata.AddMetadata(Tuple.Create(method, thenBlock, elseBlock));
        var lastValue = _block.Emit(module, type, method, metadata).Load();

        // To handle "break", "continue" and "return"
        if (lastValue is { InstructionOpcode: not LLVMOpcode.LLVMBr and not LLVMOpcode.LLVMRet })
            module.LlvmBuilder.BuildBr(ifBlock);

        module.LlvmBuilder.PositionAtEnd(elseBlock);

        return conditionValue.ToMateriskValue();
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return _initialExpressionNode;
        yield return _condNode;
        yield return _stepNode;
        yield return _block;
    }
}