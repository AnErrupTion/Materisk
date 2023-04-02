using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using MateriskLLVM;

namespace Materisk.Parse.Nodes.Misc;

internal class BlockNode : SyntaxNode
{
    private readonly List<SyntaxNode> _nodes;

    public BlockNode(List<SyntaxNode> nodes)
    {
        _nodes = nodes;
    }

    public override NodeType Type => NodeType.Block;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        return null!;
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method)
    {
        object? lastValue = null;

        foreach (var node in _nodes)
            lastValue = node.Emit(module, type, method);

        return lastValue!;
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