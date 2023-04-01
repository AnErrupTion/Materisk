using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using MateriskLLVM;

namespace Materisk.Parse.Nodes.Branch;

internal class BreakNode : SyntaxNode
{
    public override NodeType Type => NodeType.Break;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        // Dummy instruction for "for" and "while" nodes
        method.CilMethodBody!.Instructions.Add(CilOpCodes.Break);
        return null!;
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method)
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