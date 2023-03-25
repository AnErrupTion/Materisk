using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using Materisk.BuiltinTypes;

namespace Materisk.Parsing.Nodes;

internal class ListNode : SyntaxNode
{
    private readonly List<SyntaxNode> list;

    public ListNode(List<SyntaxNode> list)
    {
        this.list = list;
    }

    public override NodeType Type => NodeType.List;

    public override SValue Evaluate(Scope scope)
    {
        SList sList = new();

        foreach (var n in list)
        {
            sList.Value.Add(n.Evaluate(scope));
        }

        return sList;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        foreach (var n in list) yield return n;
    }

    public override string ToString()
    {
        return "ListNode:";
    }
}