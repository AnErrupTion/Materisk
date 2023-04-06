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

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, LLVMBasicBlockRef thenBlock, LLVMBasicBlockRef elseBlock)
    {
        return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int8, Convert.ToUInt64(_value), true).ToMateriskValue();
    }
}