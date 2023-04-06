using LLVMSharp.Interop;
using Materisk.TypeSystem;
using Materisk.Utils;

namespace Materisk.Parse.Nodes.Misc;

internal class BlockNode : SyntaxNode
{
    private readonly List<SyntaxNode> _nodes;

    public BlockNode(List<SyntaxNode> nodes)
    {
        _nodes = nodes;
    }

    public override NodeType Type => NodeType.Block;

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, LLVMBasicBlockRef thenBlock, LLVMBasicBlockRef elseBlock)
    {
        MateriskUnit? lastValue = null;

        foreach (var node in _nodes)
            lastValue = node.Emit(module, type, method, thenBlock, elseBlock);

        return lastValue ?? LlvmUtils.VoidNull.ToMateriskValue();
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        return _nodes;
    }
}