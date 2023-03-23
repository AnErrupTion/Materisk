namespace spaghetto.Parsing.Nodes;

internal class ClassFunctionDefinitionNode : SyntaxNode
{
    private readonly SyntaxToken name;
    private readonly List<SyntaxToken> args;
    private readonly SyntaxNode body;
    private readonly bool isStatic;
    private readonly bool isPublic;

    public ClassFunctionDefinitionNode(SyntaxToken name, List<SyntaxToken> args, SyntaxNode body, bool isStatic, bool isPublic)
    {
        this.name = name;
        this.args = args;
        this.body = body;
        this.isStatic = isStatic;
        this.isPublic = isPublic;
    }

    public override NodeType Type => NodeType.ClassFunctionDefinition;

    public override SValue Evaluate(Scope scope)
    {
        var targetName = name.Text;

        if (targetName is "ctor" or "toString") targetName = "$$" + targetName;

        var f = new SFunction(scope, targetName, args.Select(v => v.Text).ToList(), body) 
        { 
            IsClassInstanceMethod = !isStatic
        };
        if (isPublic) scope.GetRoot().ExportTable.Add(name.Text, f);
        return f;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(name);
        foreach (var tok in args) yield return new TokenNode(tok);
        yield return body;
    }
}