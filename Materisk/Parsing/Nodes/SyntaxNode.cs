using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using Materisk.BuiltinTypes;

namespace Materisk.Parsing.Nodes;

public abstract class SyntaxNode
{
    public abstract NodeType Type { get; }

    public abstract SValue Evaluate(Scope scope);
    public abstract object Emit(ModuleDefinition module, CilMethodBody body);
    public abstract IEnumerable<SyntaxNode> GetChildren();
}