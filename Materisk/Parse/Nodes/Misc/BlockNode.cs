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

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        MateriskUnit? lastValue = null;

        foreach (var node in _nodes)
            lastValue = node.Emit(module, type, method, metadata);

        return lastValue ?? LlvmUtils.VoidNull.ToMateriskValue();
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        return _nodes;
    }
}