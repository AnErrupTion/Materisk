using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using Materisk.BuiltinTypes;

namespace Materisk.Parsing.Nodes;

internal class IntLiteralNode : SyntaxNode
{
    private readonly SyntaxToken syntaxToken;

    public IntLiteralNode(SyntaxToken syntaxToken)
    {
        this.syntaxToken = syntaxToken;
    }

    public override NodeType Type => NodeType.IntLiteral;

    public override SValue Evaluate(Scope scope)
    {
        var sint = new SInt((int)syntaxToken.Value);
        return sint;
    }

    public override object Emit(ModuleDefinition module, CilMethodBody body)
    {
        var value = (int)syntaxToken.Value;
        body.Instructions.Add(CilInstruction.CreateLdcI4(value));
        return value;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(syntaxToken);
    }

    public override string ToString()
    {
        return "IntLitNode:";
    }
}