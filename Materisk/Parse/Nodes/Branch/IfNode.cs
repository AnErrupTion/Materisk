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

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, LLVMBasicBlockRef thenBlock, LLVMBasicBlockRef elseBlock)
    {
        var value = _conditionNode.Emit(module, type, method, thenBlock, elseBlock).Load();

        if (_elseBlockNode is null)
        {
            var llvmThenBlock = method.LlvmMethod.AppendBasicBlock("then");
            var nextBlock = method.LlvmMethod.AppendBasicBlock("next");
            var conditionValue = module.LlvmBuilder.BuildCondBr(value, llvmThenBlock, nextBlock);

            module.LlvmBuilder.PositionAtEnd(llvmThenBlock);
            _blockNode.Emit(module, type, method, llvmThenBlock, elseBlock).Load();

            // To handle "break", "continue" and "return"
            if (module.LlvmBuilder.InsertBlock.LastInstruction is { InstructionOpcode: not LLVMOpcode.LLVMBr and not LLVMOpcode.LLVMRet })
                module.LlvmBuilder.BuildBr(nextBlock);

            module.LlvmBuilder.PositionAtEnd(nextBlock);

            return conditionValue.ToMateriskValue();
        }
        else
        {
            var llvmThenBlock = method.LlvmMethod.AppendBasicBlock("then");
            var llvmElseBlock = method.LlvmMethod.AppendBasicBlock("else");
            var nextBlock = method.LlvmMethod.AppendBasicBlock("next");
            var conditionValue = module.LlvmBuilder.BuildCondBr(value, llvmThenBlock, llvmElseBlock);

            module.LlvmBuilder.PositionAtEnd(llvmThenBlock);
            _blockNode.Emit(module, type, method, llvmThenBlock, llvmElseBlock).Load();

            // To handle "break", "continue" and "return"
            if (module.LlvmBuilder.InsertBlock.LastInstruction is { InstructionOpcode: not LLVMOpcode.LLVMBr and not LLVMOpcode.LLVMRet })
                module.LlvmBuilder.BuildBr(nextBlock);

            module.LlvmBuilder.PositionAtEnd(llvmElseBlock);
            _elseBlockNode.Emit(module, type, method, llvmThenBlock, llvmElseBlock).Load();

            // To handle "break", "continue" and "return"
            if (module.LlvmBuilder.InsertBlock.LastInstruction is { InstructionOpcode: not LLVMOpcode.LLVMBr and not LLVMOpcode.LLVMRet })
                module.LlvmBuilder.BuildBr(nextBlock);

            module.LlvmBuilder.PositionAtEnd(nextBlock);

            return conditionValue.ToMateriskValue();
        }
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return _conditionNode;
        yield return _blockNode;
        if (_elseBlockNode is not null) yield return _elseBlockNode;
    }
}