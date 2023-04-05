using LLVMSharp.Interop;
using Materisk.TypeSystem;
using Materisk.Utils;

namespace Materisk.Parse.Nodes.Misc;

internal class ArrayNode : SyntaxNode
{
    private readonly string _type;
    private readonly SyntaxNode _itemCountNode;

    public ArrayNode(string type, SyntaxNode itemCountNode)
    {
        _type = type;
        _itemCountNode = itemCountNode;
    }

    public override NodeType Type => NodeType.Array;

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var elementCount = _itemCountNode.Emit(module, type, method, metadata).Load();
        var arrayType = TypeSigUtils.GetTypeSignatureFor(module, _type);
        return module.LlvmBuilder.BuildArrayAlloca(arrayType, elementCount).ToMateriskValue();
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return _itemCountNode;
    }
}