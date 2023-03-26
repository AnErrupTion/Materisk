using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;

namespace Materisk.Parse.Nodes;

internal class BreakNode : SyntaxNode
{
    public override NodeType Type => NodeType.Break;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        // Dummy instruction for "for" and "while" nodes
        method.CilMethodBody!.Instructions.Add(CilOpCodes.Break);
        return null!;
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