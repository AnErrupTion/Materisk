using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using Materisk.BuiltinTypes;

namespace Materisk.Parsing.Nodes;

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
        return new SFloat((float)syntaxToken.Value);
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, MethodDefinition method, List<string> arguments)
    {
        var value = (float)syntaxToken.Value;
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