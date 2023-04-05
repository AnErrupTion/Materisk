using LLVMSharp.Interop;
using Materisk.TypeSystem;

namespace Materisk.Parse.Nodes.Literal;

internal class ByteLiteralNode : SyntaxNode
{
    private readonly byte _value;

    public ByteLiteralNode(byte value)
    {
        _value = value;
    }

    public override NodeType Type => NodeType.ByteLiteral;

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int8, Convert.ToUInt64(_value), true);
    }
}