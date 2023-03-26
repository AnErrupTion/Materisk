using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using Materisk.BuiltinTypes;
using Materisk.Lex;

namespace Materisk.Parse.Nodes;

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

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        var name = ident.Text;

        var constructorType = module.TopLevelTypes.FirstOrDefault(x => x.Name == name);
        if (constructorType == null)
            throw new InvalidOperationException($"Unable to find type with name: {name}");

        var constructor = constructorType.Methods.FirstOrDefault(x => x.Name == ".ctor");
        if (constructor is null)
            throw new InvalidOperationException($"Unable to find constructor of type: {name}");

        foreach (var arg in argumentNodes)
            arg.Emit(variables, module, type, method, arguments);

        method.CilMethodBody.Instructions.Add(CilOpCodes.Newobj, constructor);
        return null;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield break;
    }
}