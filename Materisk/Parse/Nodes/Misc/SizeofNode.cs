using LLVMSharp.Interop;
using Materisk.TypeSystem;
using Materisk.Utils;

namespace Materisk.Parse.Nodes.Misc;

internal class SizeofNode : SyntaxNode
{
    private readonly SyntaxNode _identifierNode;

    public SizeofNode(SyntaxNode identifierNode)
    {
        _identifierNode = identifierNode;
    }

    public override NodeType Type => NodeType.Sizeof;

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, LLVMBasicBlockRef thenBlock, LLVMBasicBlockRef elseBlock)
    {
        var identifier = _identifierNode.Emit(module, type, method, thenBlock, elseBlock);

        if (identifier is not MateriskType mType)
            throw new InvalidOperationException($"Identifier is not type: {identifier}");

        if (!mType.Attributes.HasFlag(MateriskAttributes.Struct))
            throw new InvalidOperationException($"Type is not struct: {mType.ParentModule.Name}.{mType.Name}");

        return LLVMValueRef.CreateConstInt(
            LLVMTypeRef.Int64,
            LlvmUtils.GetAllocateSize(mType.Fields) / 8,
            true).ToMateriskValue();
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return _identifierNode;
    }
}