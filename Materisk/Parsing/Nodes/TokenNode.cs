using Materisk.BuiltinTypes;

namespace Materisk.Parsing.Nodes;

/// <summary>
/// dummy node for tree view
/// </summary>
public class TokenNode : SyntaxNode
{
    public override NodeType Type => NodeType.Token;
    public SyntaxToken Token { get; }

    public TokenNode(SyntaxToken token)
    {
        Token = token;
    }

    public override SValue Evaluate(Scope scope)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        return Enumerable.Empty<SyntaxNode>();
    }

    public override string ToString()
    {
        return "TokenNode: " + Token.Type + " val=" + Token.Value + " text=" + Token.Text;
    }
}