using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using Materisk.BuiltinTypes;
using Materisk.Lex;

namespace Materisk.Parse.Nodes;

internal class ArrayNode : SyntaxNode
{
    private readonly SyntaxToken typeToken;
    private readonly SyntaxNode itemCountNode;

    public ArrayNode(SyntaxToken typeToken, SyntaxNode itemCountNode)
    {
        this.typeToken = typeToken;
        this.itemCountNode = itemCountNode;
    }

    public override NodeType Type => NodeType.Array;

    public override SValue Evaluate(Scope scope)
    {
        return null;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        var arrayType = Utils.GetTypeSignatureFor(module, typeToken.Text).ToTypeDefOrRef();

        itemCountNode.Emit(variables, module, type, method, arguments);

        method.CilMethodBody.Instructions.Add(CilOpCodes.Newarr, arrayType);

        return null;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return itemCountNode;
    }

    public override string ToString()
    {
        return "ArrayNode:";
    }
}