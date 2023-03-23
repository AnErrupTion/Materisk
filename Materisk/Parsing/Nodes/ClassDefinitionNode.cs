using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using Materisk.BuiltinTypes;

namespace Materisk.Parsing.Nodes;

internal class ClassDefinitionNode : SyntaxNode
{
    private readonly SyntaxToken className;
    private readonly IEnumerable<SyntaxNode> body;
    private readonly bool isPublic;

    public ClassDefinitionNode(SyntaxToken className, IEnumerable<SyntaxNode> body, bool isPublic)
    {
        this.className = className;
        this.body = body;
        this.isPublic = isPublic;
    }

    public override NodeType Type => NodeType.ClassDefinition;

    public override SValue Evaluate(Scope scope)
    {
        var @class = new SClass
        {
            Name = className.Text
        };

        foreach (var bodyNode in body)
        {
            if (bodyNode is not ClassFunctionDefinitionNode cfdn) throw new Exception("Unexpected node in class definition");

            var funcRaw = cfdn.Evaluate(scope);

            if (funcRaw is not SBaseFunction func) throw new Exception("Expected ClassFunctionDefinitionNode to return SBaseFunction");

            if (func.IsClassInstanceMethod)
            {
                @class.InstanceBaseTable.Add((new SString(func.FunctionName), func));
            }
            else
            {
                @class.StaticTable.Add((new SString(func.FunctionName), func));
            }
        }

        scope.Set(className.Text, @class);
        if (isPublic) scope.GetRoot().ExportTable.Add(className.Text, @class);
        return @class;
    }

    public override object Emit(ModuleDefinition module, CilMethodBody body)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(className);
        foreach (var n in body) yield return n;
    }
}