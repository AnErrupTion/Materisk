﻿using Materisk.BuiltinTypes;

namespace Materisk.Parsing.Nodes;

internal class IdentifierNode : SyntaxNode
{
    public SyntaxToken Token { get; }

    public IdentifierNode(SyntaxToken syntaxToken)
    {
        Token = syntaxToken;
    }

    public override NodeType Type => NodeType.Identifier;

    public override SValue Evaluate(Scope scope)
    {
        return scope.Get((string)Token.Value) ?? SValue.Null;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(Token);
    }

    public override string ToString()
    {
        return "IdentNode:";
    }
}