using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using LLVMSharp.Interop;
using MateriskLLVM;
using Materisk.Lex;
using Materisk.Utils;

namespace Materisk.Parse.Nodes.Misc;

internal class ArrayNode : SyntaxNode
{
    private readonly SyntaxToken _typeToken;
    private readonly SyntaxNode _itemCountNode;

    public ArrayNode(SyntaxToken typeToken, SyntaxNode itemCountNode)
    {
        _typeToken = typeToken;
        _itemCountNode = itemCountNode;
    }

    public override NodeType Type => NodeType.Array;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        return null!;
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var elementCount = (LLVMValueRef)_itemCountNode.Emit(module, type, method, metadata);
        var arrayType = TypeSigUtils.GetTypeSignatureFor(_typeToken.Text);
        return module.LlvmBuilder.BuildArrayAlloca(arrayType, elementCount);
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return _itemCountNode;
    }
}