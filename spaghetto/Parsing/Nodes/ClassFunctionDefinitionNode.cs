﻿namespace spaghetto.Parsing.Nodes;

internal class ClassFunctionDefinitionNode : SyntaxNode
{
    private SyntaxToken name;
    private List<SyntaxToken> args;
    private SyntaxNode body;
    private bool isStatic;

    public ClassFunctionDefinitionNode(SyntaxToken name, List<SyntaxToken> args, SyntaxNode body, bool isStatic)
    {
        this.name = name;
        this.args = args;
        this.body = body;
        this.isStatic = isStatic;
    }

    public override NodeType Type => NodeType.ClassFunctionDefinition;

    public override SValue Evaluate(Scope scope)
    {
        var targetName = name.Text;

        if (targetName is "ctor" or "toString") targetName = "$$" + targetName;

        var f = new SFunction(scope, targetName, args.Select((v) => v.Text).ToList(), body);
        f.IsClassInstanceMethod = !isStatic;
        return f;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(name);
        foreach (var tok in args) yield return new TokenNode(tok);
        yield return body;
    }
}