using LLVMSharp.Interop;
using Materisk.TypeSystem;

namespace Materisk.Parse.Nodes.Misc;

internal class IndexNode : SyntaxNode
{
    private readonly SyntaxNode _nameNode;
    private readonly SyntaxNode _indexNode;
    private readonly SyntaxNode? _setNode;

    public IndexNode(SyntaxNode nameNode, SyntaxNode indexNode, SyntaxNode? setNode)
    {
        _nameNode = nameNode;
        _indexNode = indexNode;
        _setNode = setNode;
    }

    public override NodeType Type => NodeType.Index;

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, LLVMBasicBlockRef thenBlock, LLVMBasicBlockRef elseBlock)
    {
        var name = _nameNode.Emit(module, type, method, thenBlock, elseBlock);
        var llvmElementType = name.PointerElementType;
        var llvmValue = name.Load();
        var underlyingType = llvmValue.TypeOf.Kind;
        var index = _indexNode.Emit(module, type, method, thenBlock, elseBlock).Load();

        switch (underlyingType)
        {
            case LLVMTypeKind.LLVMPointerTypeKind when _setNode is not null:
            {
                var llvmPtr = module.LlvmBuilder.BuildGEP2(llvmElementType, llvmValue, new[] { index });
                var value = _setNode.Emit(module, type, method, thenBlock, elseBlock).Load();
                return module.LlvmBuilder.BuildStore(value, llvmPtr).ToMateriskValue();
            }
            case LLVMTypeKind.LLVMPointerTypeKind:
            {
                var llvmPtr = module.LlvmBuilder.BuildGEP2(llvmElementType, llvmValue, new[] { index });
                return module.LlvmBuilder.BuildLoad2(llvmElementType, llvmPtr).ToMateriskValue();
            }
            default:
            {
                throw new InvalidOperationException($"Catastrophic failure: value is not pointer: {underlyingType}");
            }
        }
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return _nameNode;
        yield return _indexNode;
        if (_setNode is not null) yield return _setNode;
    }
}