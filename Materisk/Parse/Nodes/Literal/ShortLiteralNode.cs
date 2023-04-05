using LLVMSharp.Interop;
using Materisk.TypeSystem;
using Materisk.Utils;

namespace Materisk.Parse.Nodes.Literal;

internal class ShortLiteralNode : SyntaxNode
{
    private readonly short _value;

    public ShortLiteralNode(short value)
    {
        _value = value;
    }

    public override NodeType Type => NodeType.ShortLiteral;

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        if (_value < 0)
        {
            var llvmValue = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int16, Convert.ToUInt64(Math.Abs(_value)), true);
            return LLVMValueRef.CreateConstSub(LlvmUtils.ShortZero, llvmValue).ToMateriskValue();
        }

        return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int16, Convert.ToUInt64(_value), true).ToMateriskValue();
    }
}