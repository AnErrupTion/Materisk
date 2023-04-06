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
        MateriskType? constructorType = null;
        MateriskMethod? constructor = null;

        foreach (var mType in module.Types)
            foreach (var mMethod in mType.Methods)
                if (mType.Name == _identifier && mMethod.Name is "ctor")
                {
                    constructorType = mType;
                    constructor = mMethod;
                    break;
                }

        if (constructorType is null || constructor is null)
            throw new InvalidOperationException($"Unable to find constructor for type: {module.Name}.{_identifier}");

        // Allocate a new struct
        // TODO: Use Memory.allocate()
        var newStruct = module.LlvmBuilder.BuildAlloca(constructorType.Type);

        // Construct arguments
        var args = new LLVMValueRef[_argumentNodes.Count + 1];
        args[0] = newStruct;

        for (var i = 1; i < args.Length; i++)
            args[i] = _argumentNodes[i].Emit(module, type, method, thenBlock, elseBlock).Load();

        // Call constructor
        module.LlvmBuilder.BuildCall2(constructor.Type, constructor.Load(), args);

        return newStruct.ToMateriskValue();
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        return _argumentNodes;
    }
}