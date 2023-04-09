using LLVMSharp.Interop;
using Materisk.TypeSystem;
using Materisk.Utils;

namespace Materisk.Parse.Nodes.Instantiation;

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
        // Get required methods
        var (constructorType, constructor) = MateriskHelpers.GetOrCreateMethod(module, _identifier, "ctor");
        var (_, allocate) = MateriskHelpers.GetOrCreateMethod(module, "Memory", "allocate");

        // Allocate a new struct on the heap
        var newStruct = module.LlvmBuilder.BuildCall2(
            allocate.Type,
            allocate.LlvmMethod,
            new[] { LLVMValueRef.CreateConstInt(
                LLVMTypeRef.Int64,
                LlvmUtils.GetAllocateSize(constructorType.Fields) / 8,
                true) });

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