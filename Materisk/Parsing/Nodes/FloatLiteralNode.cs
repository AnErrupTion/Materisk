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

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(syntaxToken);
    }

    public override string ToString()
    {
        return "FloatLitNode:";
    }
}