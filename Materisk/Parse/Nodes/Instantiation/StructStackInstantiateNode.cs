using LLVMSharp.Interop;
using Materisk.TypeSystem;

namespace Materisk.Parse.Nodes.Instantiation;

internal class StructStackInstantiateNode : SyntaxNode
{
    private readonly SyntaxNode _identifierNode;

    public StructStackInstantiateNode(SyntaxNode identifierNode)
    {
        _identifierNode = identifierNode;
    }

    public override NodeType Type => NodeType.StructStackInstantiate;

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, LLVMBasicBlockRef thenBlock, LLVMBasicBlockRef elseBlock)
    {
        // Get required struct type
        var node = _identifierNode.Emit(module, type, method, thenBlock, elseBlock);

        if (node is not MateriskType mType)
            throw new InvalidOperationException($"Node is not type: {node}");

        // Allocate a new struct on the stack
        return module.LlvmBuilder.BuildAlloca(mType.Type).ToMateriskValue();
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return _identifierNode;
    }
}