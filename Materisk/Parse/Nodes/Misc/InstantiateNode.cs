using LLVMSharp.Interop;
using Materisk.TypeSystem;

namespace Materisk.Parse.Nodes.Misc;

internal class InstantiateNode : SyntaxNode
{
    private readonly string _identifier;
    private readonly List<SyntaxNode> _argumentNodes;

    public InstantiateNode(string identifier, List<SyntaxNode> argumentNodes)
    {
        _identifier = identifier;
        _argumentNodes = argumentNodes;
    }

    public override NodeType Type => NodeType.Instantiate;

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, LLVMBasicBlockRef thenBlock, LLVMBasicBlockRef elseBlock)
    {
        /*var name = _ident.Text;

        var constructorType = module.TopLevelTypes.FirstOrDefault(x => x.Name == name);
        if (constructorType == null)
            throw new InvalidOperationException($"Unable to find type with name: {name}");

        var constructor = constructorType.Methods.FirstOrDefault(x => x.Name == ".ctor");
        if (constructor is null)
            throw new InvalidOperationException($"Unable to find constructor of type: {name}");

        foreach (var arg in _argumentNodes)
            arg.Emit(variables, module, type, method, arguments);

        method.CilMethodBody!.Instructions.Add(CilOpCodes.Newobj, constructor);*/
        throw new NotImplementedException();
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        return _argumentNodes;
    }
}