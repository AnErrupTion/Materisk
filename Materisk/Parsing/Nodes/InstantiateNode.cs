using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
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
        var name = ident.Text;

        MethodDefinition? actualCtor = null;
        TypeDefinition? actualCtorType = null;
        MethodDefinition? constructor = null;

        foreach (var type in module.TopLevelTypes)
            foreach (var meth in type.Methods)
            {
                if (constructor != null && actualCtor != null)
                    break;

                if (type.Name == name)
                {
                    actualCtorType = type;
                    if (meth.Name == "cctor")
                        constructor = meth;
                    else if (meth.IsConstructor)
                        actualCtor = meth;
                }
            }
        
        if (actualCtorType == null)
            throw new InvalidOperationException($"Unable to find type with name: {name}");

        if (constructor == null)
            throw new InvalidOperationException($"Unable to find constructor for type: {name}");

        actualCtor ??= actualCtorType.GetOrCreateStaticConstructor();

        foreach (var arg in argumentNodes)
            arg.Emit(variables, module, method, arguments);

        // TODO: Improve performance by setting "cctor" to the default CIL constructor
        method.CilMethodBody?.Instructions.Add(CilOpCodes.Newobj, actualCtor);
        method.CilMethodBody?.Instructions.Add(CilOpCodes.Dup);
        method.CilMethodBody?.Instructions.Add(CilOpCodes.Call, constructor);
        return null;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield break;
    }
}