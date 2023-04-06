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

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, LLVMBasicBlockRef thenBlock, LLVMBasicBlockRef elseBlock)
    {
        var ifBlock = method.LlvmMethod.AppendBasicBlock("if");
        var llvmThenBlock = method.LlvmMethod.AppendBasicBlock("then");
        var llvmElseBlock = method.LlvmMethod.AppendBasicBlock("else");

        module.LlvmBuilder.BuildBr(ifBlock);

        module.LlvmBuilder.PositionAtEnd(ifBlock);
        var ifValue = _condNode.Emit(module, type, method, llvmThenBlock, llvmElseBlock).Load();
        var conditionValue = module.LlvmBuilder.BuildCondBr(ifValue, llvmThenBlock, llvmElseBlock);

        module.LlvmBuilder.PositionAtEnd(llvmThenBlock);
        _block.Emit(module, type, method, llvmThenBlock, llvmElseBlock).Load();

        // To handle "break", "continue" and "return"
        if (module.LlvmBuilder.InsertBlock.LastInstruction is { InstructionOpcode: not LLVMOpcode.LLVMBr and not LLVMOpcode.LLVMRet })
            module.LlvmBuilder.BuildBr(ifBlock);

        module.LlvmBuilder.PositionAtEnd(llvmElseBlock);

        return conditionValue.ToMateriskValue();
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return _condNode;
        yield return _block;
    }
}