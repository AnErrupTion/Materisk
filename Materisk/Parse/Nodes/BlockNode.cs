using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using Materisk.BuiltinTypes;

namespace Materisk.Parse.Nodes;

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
        return null;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        object lastVal = null;

        foreach (var node in nodes)
        {
            var res = node.Emit(variables, module, type, method, arguments);

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