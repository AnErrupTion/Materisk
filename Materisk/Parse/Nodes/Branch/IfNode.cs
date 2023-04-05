using LLVMSharp.Interop;
using Materisk.TypeSystem;

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

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var value = (LLVMValueRef)_conditionNode.Emit(module, type, method, metadata);

        if (_elseBlockNode is null)
        {
            var thenBlock = method.LlvmMethod.AppendBasicBlock("then");
            var nextBlock = method.LlvmMethod.AppendBasicBlock("next");
            var conditionValue = module.LlvmBuilder.BuildCondBr(value, thenBlock, nextBlock);

            module.LlvmBuilder.PositionAtEnd(thenBlock);
            var lastValue = _blockNode.Emit(module, type, method, metadata);

            // To handle "break", "continue" and "return"
            if (lastValue is LLVMValueRef { InstructionOpcode: not LLVMOpcode.LLVMBr and not LLVMOpcode.LLVMRet })
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
            var lastValue = _blockNode.Emit(module, type, method, metadata);

            // To handle "break", "continue" and "return"
            if (lastValue is LLVMValueRef { InstructionOpcode: not LLVMOpcode.LLVMBr and not LLVMOpcode.LLVMRet })
                module.LlvmBuilder.BuildBr(nextBlock);

            module.LlvmBuilder.PositionAtEnd(elseBlock);
            lastValue = _elseBlockNode.Emit(module, type, method, metadata);

            // To handle "break", "continue" and "return"
            if (lastValue is LLVMValueRef { InstructionOpcode: not LLVMOpcode.LLVMBr and not LLVMOpcode.LLVMRet })
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
}