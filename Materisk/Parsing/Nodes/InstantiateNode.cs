using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using Materisk.BuiltinTypes;

namespace Materisk.Parsing.Nodes;

internal class InstantiateNode : SyntaxNode
{
    private readonly SyntaxToken ident;
    private readonly List<SyntaxNode> argumentNodes;

    public InstantiateNode(SyntaxToken ident, List<SyntaxNode> argumentNodes)
    {
        this.ident = ident;
        this.argumentNodes = argumentNodes;
    }

    public override NodeType Type => NodeType.Instantiate;

    public override SValue Evaluate(Scope scope)
    {
        var @class = scope.Get(ident.Text);
        if (@class == null || @class is not SClass sclass) throw new Exception("Class not found!");


        var instance = new SClassInstance(sclass);

        List<SValue> args = new() { instance };
        foreach (var n in argumentNodes) args.Add(n.Evaluate(scope));

        instance.CallConstructor(scope, args);

        return instance;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, MethodDefinition method, List<string> arguments)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        throw new NotImplementedException();
    }
}