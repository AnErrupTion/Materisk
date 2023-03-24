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
        if (@class is not SClass sclass) throw new Exception("Module not found!");


        var instance = new SClassInstance(sclass);

        List<SValue> args = new() { instance };
        foreach (var n in argumentNodes) args.Add(n.Evaluate(scope));

        instance.CallConstructor(scope, args);

        return instance;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, MethodDefinition method, List<string> arguments)
    {
        /*var name = ident.Text;

        TypeDefinition? typeDef = null;
        foreach (var type in module.TopLevelTypes)
            if (type.Name == name)
            {
                typeDef = type;
                break;
            }

        if (typeDef == null)
            throw new InvalidOperationException($"Unable to find type with name: {name}");

        MethodDefinition? constructor = null;
        foreach (var meth in typeDef.Methods)
            if (meth.Name == "ctor")
            {
                constructor = meth;
                break;
            }

        if (constructor == null)
            throw new InvalidOperationException($"Unable to find constructor for type: {name}");

        foreach (var arg in argumentNodes)
            arg.Emit(variables, module, method, arguments);

        method.CilMethodBody?.Instructions.Add(CilOpCodes.Call, constructor);
        return typeDef;*/
        throw new NotImplementedException();
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield break;
    }
}