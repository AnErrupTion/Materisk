using System.Globalization;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using Materisk.BuiltinTypes;
using Materisk.Lex;

namespace Materisk.Parse.Nodes;

internal class FloatLiteralNode : SyntaxNode
{
    private readonly SyntaxToken syntaxToken;

    public FloatLiteralNode(SyntaxToken syntaxToken)
    {
        this.syntaxToken = syntaxToken;
    }

    public override NodeType Type => NodeType.FloatLiteral;

    public override SValue Evaluate(Scope scope)
    {
        return null;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        var value = float.Parse(syntaxToken.Text, CultureInfo.InvariantCulture);
        method.CilMethodBody?.Instructions.Add(CilOpCodes.Ldc_R4, value);
        return value;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(syntaxToken);
    }

    public override string ToString()
    {
        return "FloatLitNode:";
    }
}