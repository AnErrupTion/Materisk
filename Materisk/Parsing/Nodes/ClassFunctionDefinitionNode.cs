using Materisk.BuiltinTypes;
using Materisk.Native;

namespace Materisk.Parsing.Nodes;

internal class ClassFunctionDefinitionNode : SyntaxNode
{
    private readonly SyntaxToken className;
    private readonly SyntaxToken name;
    private readonly List<SyntaxToken> args;
    private readonly SyntaxNode body;
    private readonly bool isStatic;
    private readonly bool isPublic;
    private readonly bool isNative;

    public ClassFunctionDefinitionNode(SyntaxToken className, SyntaxToken name, List<SyntaxToken> args, SyntaxNode body, bool isStatic, bool isPublic, bool isNative)
    {
        this.className = className;
        this.name = name;
        this.args = args;
        this.body = body;
        this.isStatic = isStatic;
        this.isPublic = isPublic;
        this.isNative = isNative;
    }

    public override NodeType Type => NodeType.ClassFunctionDefinition;

    public override SValue Evaluate(Scope scope)
    {
        var targetName = name.Text;

        if (targetName is "ctor" or "toString") {
            if(args.Count(v => v.Text == "self") != 1) {
                throw new Exception($"Special class method '{targetName}' must contain the argument 'self' exactly once.");
            }

            targetName = "$$" + targetName;
        }

        var fullName = $"{className.Text}:{targetName}";

        SBaseFunction f;

        if (isNative)
        {
            f = new SNativeFunction(targetName, NativeFuncImpl.GetImplFor(fullName), args.Select(v => v.Text).ToList(), !isStatic)
            {
                IsPublic = isPublic
            };
        }
        else
        {
            f = new SFunction(scope, targetName, args.Select(v => v.Text).ToList(), body) 
            { 
                IsClassInstanceMethod = !isStatic,
                IsPublic = isPublic
            };
        }
        
        if (isPublic) scope.GetRoot().ExportTable.Add(fullName, f);
        return f;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(name);
        foreach (var tok in args) yield return new TokenNode(tok);
        yield return body;
    }
}