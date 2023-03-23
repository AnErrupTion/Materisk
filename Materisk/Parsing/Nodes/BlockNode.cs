using System.Diagnostics;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using Materisk.BuiltinTypes;

namespace Materisk.Parsing.Nodes;

internal class BlockNode : SyntaxNode
{
    private readonly List<SyntaxNode> nodes;
    private readonly bool createNewScope;

    public BlockNode(List<SyntaxNode> nodes, bool createNewScope = true)
    {
        this.nodes = nodes;
        this.createNewScope = createNewScope;
    }

    public override NodeType Type => NodeType.Block;

    public override SValue Evaluate(Scope scope)
    {
        var lastVal = SValue.Null;
        var blockScope = scope;

        if (createNewScope) blockScope = new Scope(scope);

        foreach (var node in nodes)
        {
            var res = node.Evaluate(blockScope);

            if (!res.IsNull())
            {
                lastVal = res;
            }

            if (scope.State is ScopeState.ShouldBreak or ScopeState.ShouldContinue) return lastVal;

            if (scope.State == ScopeState.ShouldReturn)
            {
                Debug.WriteLine("Returning from call node");
                scope.SetState(ScopeState.None);
                var v = scope.ReturnValue;
                return v;
            }
        }

        return lastVal;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, MethodDefinition method, List<string> arguments)
    {
        object lastVal = null;

        foreach (var node in nodes)
        {
            var res = node.Emit(variables, module, method, arguments);

            if (res != null)
                lastVal = res;
        }

        return lastVal;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        foreach (var node in nodes) yield return node;
    }

    public override string ToString()
    {
        return "BlockNode:";
    }
}