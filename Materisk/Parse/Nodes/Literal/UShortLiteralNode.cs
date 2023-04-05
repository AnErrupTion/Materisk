using LLVMSharp.Interop;
using MateriskLLVM;

namespace Materisk.Parse.Nodes.Literal;

internal class UShortLiteralNode : SyntaxNode
{
    private readonly ushort _value;

    public UShortLiteralNode(ushort value)
    {
        _value = value;
    }

    public override NodeType Type => NodeType.UShortLiteral;

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int16, Convert.ToUInt64(_value), true);
    }
}