using LLVMSharp.Interop;
using Materisk.TypeSystem;

namespace Materisk.Parse.Nodes.Instantiation;

internal class StackInstantiateNode : SyntaxNode
{
    private readonly string _identifier;
    private readonly List<SyntaxNode> _argumentNodes;

    public StackInstantiateNode(string identifier, List<SyntaxNode> argumentNodes)
    {
        _identifier = identifier;
        _argumentNodes = argumentNodes;
    }

    public override NodeType Type => NodeType.StackInstantiate;

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, LLVMBasicBlockRef thenBlock, LLVMBasicBlockRef elseBlock)
    {
        var (constructorType, constructor) = MateriskHelpers.GetOrCreateMethod(module, _identifier, "ctor");

        // Allocate a new struct on the stack
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