using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using Materisk.BuiltinTypes;

namespace Materisk.Parsing.Nodes;

internal class BreakNode : SyntaxNode
{
    public override NodeType Type => NodeType.Break;

    public override SValue Evaluate(Scope scope)
    {
        scope.SetState(ScopeState.ShouldBreak);
        return SValue.Null;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, MethodDefinition method, Dictionary<string, object> arguments)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        return Enumerable.Empty<SyntaxNode>();
    }

    public override string ToString()
    {
        return "BreakNode:";
    }
}