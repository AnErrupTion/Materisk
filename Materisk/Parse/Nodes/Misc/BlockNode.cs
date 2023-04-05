using Materisk.TypeSystem;
using Materisk.Utils;

namespace Materisk.Parse.Nodes.Misc;

internal class BlockNode : SyntaxNode
{
    public readonly List<SyntaxNode> Nodes;

    public BlockNode(List<SyntaxNode> nodes)
    {
        Nodes = nodes;
    }

    public override NodeType Type => NodeType.Block;

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        MateriskUnit? lastValue = null;

        foreach (var node in Nodes)
            lastValue = node.Emit(module, type, method, metadata);

        return lastValue ?? LlvmUtils.VoidNull.ToMateriskValue();
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        return Nodes;
    }
}