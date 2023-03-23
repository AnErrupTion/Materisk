using AsmResolver.DotNet;
using Materisk.BuiltinTypes;

namespace Materisk.Parsing.Nodes;

public abstract class SyntaxNode
{
    public abstract NodeType Type { get; }

    public abstract SValue Evaluate(Scope scope);
    //public abstract void Emit(ModuleDefinition module);
    public abstract IEnumerable<SyntaxNode> GetChildren();
}