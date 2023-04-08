using LLVMSharp.Interop;
using Materisk.TypeSystem;

namespace Materisk.Parse.Nodes.Instantiation;

internal class DeallocateNode : SyntaxNode
{
    private readonly SyntaxNode _identifierNode;

    public DeallocateNode(SyntaxNode identifierNode)
    {
        _identifierNode = identifierNode;
    }

    public override NodeType Type => NodeType.Free;

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, LLVMBasicBlockRef thenBlock, LLVMBasicBlockRef elseBlock)
    {
        MateriskMethod? free = null;

        foreach (var mType in module.Types)
            foreach (var mMethod in mType.Methods)
                if (mType.Name is "Memory" && mMethod.Name is "free")
                {
                    free = mMethod;
                    break;
                }

        if (free is null)
            throw new InvalidOperationException($"Unable to find method: Memory.free()");

        // Get struct from identifier
        var structValue = _identifierNode.Emit(module, type, method, thenBlock, elseBlock);

        // Free the struct from the heap
        module.LlvmBuilder.BuildCall2(free.Type, free.LlvmMethod, new[] { structValue.Load() });

        return null!;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return _identifierNode;
    }
}