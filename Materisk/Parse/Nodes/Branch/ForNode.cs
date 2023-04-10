using LLVMSharp.Interop;
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

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, LLVMBasicBlockRef thenBlock, LLVMBasicBlockRef elseBlock)
    {
        var ifBlock = method.LlvmMethod.AppendBasicBlock("if");
        var llvmThenBlock = method.LlvmMethod.AppendBasicBlock("then");
        var llvmElseBlock = method.LlvmMethod.AppendBasicBlock("else");

        _initialExpressionNode.Emit(module, type, method, llvmThenBlock, llvmElseBlock);
        module.LlvmBuilder.BuildBr(ifBlock);

        module.LlvmBuilder.PositionAtEnd(ifBlock);
        var ifValue = _condNode.Emit(module, type, method, llvmThenBlock, llvmElseBlock).Load();
        var conditionValue = module.LlvmBuilder.BuildCondBr(ifValue, llvmThenBlock, llvmElseBlock);

        module.LlvmBuilder.PositionAtEnd(llvmThenBlock);
        _block.Emit(module, type, method, llvmThenBlock, llvmElseBlock).Load();
        _stepNode.Emit(module, type, method, llvmThenBlock, llvmElseBlock);

        // To handle "break", "continue" and "return"
        if (module.LlvmBuilder.InsertBlock.LastInstruction is { InstructionOpcode: not LLVMOpcode.LLVMBr and not LLVMOpcode.LLVMRet })
            module.LlvmBuilder.BuildBr(ifBlock);

        module.LlvmBuilder.PositionAtEnd(llvmElseBlock);

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