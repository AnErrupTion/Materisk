using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using Materisk.BuiltinTypes;
using Materisk.Lexing;

namespace Materisk.Parsing.Nodes;

internal class ArrayNode : SyntaxNode
{
    private readonly SyntaxToken? typeToken;
    private readonly List<SyntaxNode> list;

    public ArrayNode(SyntaxToken? typeToken, List<SyntaxNode> list)
    {
        this.typeToken = typeToken;
        this.list = list;
    }

    public override NodeType Type => NodeType.List;

    public override SValue Evaluate(Scope scope)
    {
        return null;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        var arrayType = Utils.GetTypeSignatureFor(module, typeToken.Text).ToTypeDefOrRef();

        method.CilMethodBody?.Instructions.Add(CilInstruction.CreateLdcI4(list.Count));
        method.CilMethodBody?.Instructions.Add(CilOpCodes.Newarr, arrayType);

        var items = new object[list.Count];
        var index = 0;

        /*foreach (var node in list)
        {
            method.CilMethodBody?.Instructions.Add(CilInstruction.CreateLdcI4(index));
            items[index++] = node.Emit(variables, module, type, method, arguments);
            method.CilMethodBody?.Instructions.Add(CilOpCodes.Stelem, arrayType);
        }*/

        return items;
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