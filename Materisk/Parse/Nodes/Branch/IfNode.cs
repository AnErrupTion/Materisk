using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
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
        return null!;
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var value = (LLVMValueRef)_conditionNode.Emit(module, type, method, metadata);

        if (_elseBlockNode is null)
        {
            var thenBlock = method.LlvmMethod.AppendBasicBlock("then");
            var nextBlock = method.LlvmMethod.AppendBasicBlock("next");
            var conditionValue = module.LlvmBuilder.BuildCondBr(value, thenBlock, nextBlock);

            module.LlvmBuilder.PositionAtEnd(thenBlock);
            _blockNode.Emit(module, type, method, metadata);

            // To handle "break" and "continue"
            if (metadata.Last() is not true)
            {
                module.LlvmBuilder.BuildBr(nextBlock);
                metadata.RemoveLast();
            }

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
            _blockNode.Emit(module, type, method, metadata);

            // To handle "break" and "continue"
            if (metadata.Last() is not true)
            {
                module.LlvmBuilder.BuildBr(nextBlock);
                metadata.RemoveLast();
            }

            module.LlvmBuilder.PositionAtEnd(elseBlock);
            _elseBlockNode.Emit(module, type, method, metadata);

            // To handle "break" and "continue"
            if (metadata.Last() is not true)
            {
                module.LlvmBuilder.BuildBr(nextBlock);
                metadata.RemoveLast();
            }

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