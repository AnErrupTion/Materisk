using Materisk.TypeSystem;

namespace Materisk.Parse.Nodes;

public abstract class SyntaxNode
{
    public abstract NodeType Type { get; }

    public abstract MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata);

    public virtual IEnumerable<SyntaxNode> GetChildren() => Enumerable.Empty<SyntaxNode>();
}