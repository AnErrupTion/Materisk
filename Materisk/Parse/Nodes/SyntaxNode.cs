using LLVMSharp.Interop;
using Materisk.TypeSystem;

namespace Materisk.Parse.Nodes;

public abstract class SyntaxNode
{
    public abstract NodeType Type { get; }

    public abstract MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, LLVMBasicBlockRef thenBlock, LLVMBasicBlockRef elseBlock);

    public virtual IEnumerable<SyntaxNode> GetChildren() => Enumerable.Empty<SyntaxNode>();
}