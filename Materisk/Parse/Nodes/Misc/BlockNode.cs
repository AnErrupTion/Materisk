using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using MateriskLLVM;

namespace Materisk.Parse.Nodes.Misc;

internal class BlockNode : SyntaxNode
{
    public readonly List<SyntaxNode> Nodes;

    public BlockNode(List<SyntaxNode> nodes)
    {
        Nodes = nodes;
    }

    public override NodeType Type => NodeType.Block;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        return null!;
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        object? lastValue = null;

        foreach (var node in Nodes)
            lastValue = node.Emit(module, type, method, metadata);

        return lastValue!;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        return Nodes;
    }
}