using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using Materisk.Lex;

namespace Materisk.Parse.Nodes;

internal class StringLiteralNode : SyntaxNode
{
    private readonly SyntaxToken _syntaxToken;

    public StringLiteralNode(SyntaxToken syntaxToken)
    {
        _syntaxToken = syntaxToken;
    }

    public override NodeType Type => NodeType.StringLiteral;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        var value = _syntaxToken.Text;
        method.CilMethodBody!.Instructions.Add(CilOpCodes.Ldstr, value);
        return value;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(_syntaxToken);
    }

    public override string ToString()
    {
        return "StringLitNode:";
    }
}