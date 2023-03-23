using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using Materisk.BuiltinTypes;

namespace Materisk.Parsing.Nodes;

internal class ContinueNode : SyntaxNode
{
    public override NodeType Type => NodeType.Continue;

    public override SValue Evaluate(Scope scope)
    {
        scope.SetState(ScopeState.ShouldContinue);
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
        return "ContinueNode:";
    }
}