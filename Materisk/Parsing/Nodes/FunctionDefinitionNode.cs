using Materisk.BuiltinTypes;
using Materisk.Native;

namespace Materisk.Parsing.Nodes;

internal class FunctionDefinitionNode : SyntaxNode
{
    private readonly SyntaxToken? nameToken;
    private readonly List<SyntaxToken> args;
    private readonly SyntaxNode block;
    private readonly bool isPublic;
    private readonly bool isNative;

    public FunctionDefinitionNode(SyntaxToken? nameToken, List<SyntaxToken> args, SyntaxNode block, bool isPublic, bool isNative)
    {
        this.nameToken = nameToken;
        this.args = args;
        this.block = block;
        this.isPublic = isPublic;
        this.isNative = isNative;
    }

    public override NodeType Type => NodeType.FunctionDefinition;

    public override SValue Evaluate(Scope scope)
    {
        SBaseFunction f;
        var name = nameToken?.Text ?? "<anonymous>";

        if (isNative)
        {
            f = new SNativeFunction(name, NativeFuncImpl.GetImplFor(name), args.Select(v => v.Text).ToList())
            {
                IsPublic = isPublic
            };
        }
        else
        {
            f = new SFunction(scope, name, args.Select(v => v.Text).ToList(), block)
            {
                IsPublic = isPublic
            };
        }

        if (name != "<anonymous>")
        {
            scope.Set(name, f);
            if (isPublic) scope.GetRoot().ExportTable.Add(name, f);
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