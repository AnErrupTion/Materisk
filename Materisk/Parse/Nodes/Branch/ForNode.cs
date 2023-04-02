using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using LLVMSharp.Interop;
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
        return null!;
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var ifBlock = method.LlvmMethod.AppendBasicBlock("if");
        var thenBlock = method.LlvmMethod.AppendBasicBlock("then");
        var elseBlock = method.LlvmMethod.AppendBasicBlock("else");

        _initialExpressionNode.Emit(module, type, method, metadata);
        module.LlvmBuilder.BuildBr(ifBlock);

        module.LlvmBuilder.PositionAtEnd(ifBlock);
        var ifValue = (LLVMValueRef)_condNode.Emit(module, type, method, metadata);
        var conditionValue = module.LlvmBuilder.BuildCondBr(ifValue, thenBlock, elseBlock);

        module.LlvmBuilder.PositionAtEnd(thenBlock);
        _stepNode.Emit(module, type, method, metadata);
        metadata.AddMetadata(Tuple.Create(method, thenBlock, elseBlock));
        var lastValue = (LLVMValueRef)_block.Emit(module, type, method, metadata);

        // To handle "break" and "continue"
        if (lastValue != null && lastValue.InstructionOpcode is not LLVMOpcode.LLVMBr)
            module.LlvmBuilder.BuildBr(ifBlock);

        module.LlvmBuilder.PositionAtEnd(elseBlock);

        return conditionValue;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return _initialExpressionNode;
        yield return _condNode;
        yield return _stepNode;
        yield return _block;
    }
}