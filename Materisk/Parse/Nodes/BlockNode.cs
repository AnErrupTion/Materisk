using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;

namespace Materisk.Parse.Nodes;

internal class BlockNode : SyntaxNode
{
    private readonly List<SyntaxNode> _nodes;
    private readonly bool _createNewScope;

    public BlockNode(List<SyntaxNode> nodes, bool createNewScope = true)
    {
        _nodes = nodes;
        _createNewScope = createNewScope;
    }

    public override NodeType Type => NodeType.Block;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        object lastVal = null;

        foreach (var node in _nodes)
        {
            var res = node.Emit(variables, module, type, method, arguments);

            if (res != null)
                lastVal = res;
        }

        return lastVal;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        foreach (var node in _nodes) yield return node;
    }

    public override string ToString()
    {
        return "BlockNode:";
    }
}