using LLVMSharp.Interop;
using Materisk.TypeSystem;

namespace Materisk.Parse.Nodes.Literal;

internal class ULongLiteralNode : SyntaxNode
{
    private readonly ulong _value;

    public ULongLiteralNode(ulong value)
    {
        _value = value;
    }

    public override NodeType Type => NodeType.ULongLiteral;

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int64, Convert.ToUInt64(_value), true).ToMateriskValue();
    }
}