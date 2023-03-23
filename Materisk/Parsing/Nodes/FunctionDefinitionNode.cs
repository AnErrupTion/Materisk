using Materisk.BuiltinTypes;

namespace Materisk.Parsing.Nodes;

internal class FunctionDefinitionNode : SyntaxNode
{
    private readonly SyntaxToken? nameToken;
    private readonly List<SyntaxToken> args;
    private readonly SyntaxNode block;
    private readonly bool isPublic;

    public FunctionDefinitionNode(SyntaxToken? nameToken, List<SyntaxToken> args, SyntaxNode block, bool isPublic)
    {
        this.nameToken = nameToken;
        this.args = args;
        this.block = block;
        this.isPublic = isPublic;
    }

    public override NodeType Type => NodeType.FunctionDefinition;

    public override SValue Evaluate(Scope scope)
    {
        var f = new SFunction(scope, nameToken?.Text ?? "<anonymous>", args.Select(v => v.Text).ToList(), block)
        {
            IsPublic = isPublic
        };
        if (nameToken != null)
        {
            scope.Set(nameToken?.Text, f);
            if (isPublic) scope.GetRoot().ExportTable.Add(nameToken?.Text, f);
        }
        return f;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        if (nameToken != null) yield return new TokenNode(nameToken.Value);
        foreach (var t in args) yield return new TokenNode(t);
        yield return block;
    }
}