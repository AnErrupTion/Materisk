using LLVMSharp.Interop;
using Materisk.TypeSystem;

namespace Materisk.Parse.Nodes.Branch;

internal class WhileNode : SyntaxNode
{
    private readonly SyntaxNode _condNode;
    private readonly SyntaxNode _block;

    public WhileNode(SyntaxNode condNode, SyntaxNode block)
    {
        _condNode = condNode;
        _block = block;
    }

    public override NodeType Type => NodeType.While;

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var ifBlock = method.LlvmMethod.AppendBasicBlock("if");
        var thenBlock = method.LlvmMethod.AppendBasicBlock("then");
        var elseBlock = method.LlvmMethod.AppendBasicBlock("else");

        module.LlvmBuilder.BuildBr(ifBlock);

        module.LlvmBuilder.PositionAtEnd(ifBlock);
        var ifValue = (LLVMValueRef)_condNode.Emit(module, type, method, metadata);
        var conditionValue = module.LlvmBuilder.BuildCondBr(ifValue, thenBlock, elseBlock);

        module.LlvmBuilder.PositionAtEnd(thenBlock);
        metadata.AddMetadata(Tuple.Create(method, thenBlock, elseBlock));
        var lastValue = _block.Emit(module, type, method, metadata);

        // To handle "break", "continue" and "return"
        if (lastValue is LLVMValueRef { InstructionOpcode: not LLVMOpcode.LLVMBr and not LLVMOpcode.LLVMRet })
            module.LlvmBuilder.BuildBr(ifBlock);

        module.LlvmBuilder.PositionAtEnd(elseBlock);

        return conditionValue;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return _condNode;
        yield return _block;
    }
}