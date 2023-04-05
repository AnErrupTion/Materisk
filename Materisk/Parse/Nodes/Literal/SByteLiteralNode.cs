using LLVMSharp.Interop;
using Materisk.TypeSystem;
using Materisk.Utils;

namespace Materisk.Parse.Nodes.Literal;

internal class SByteLiteralNode : SyntaxNode
{
    private readonly sbyte _value;

    public SByteLiteralNode(sbyte value)
    {
        _value = value;
    }

    public override NodeType Type => NodeType.SByteLiteral;

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        if (_value < 0)
        {
            var llvmValue = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int8, Convert.ToUInt64(Math.Abs(_value)), true);
            return LLVMValueRef.CreateConstSub(LlvmUtils.ByteZero, llvmValue).ToMateriskValue();
        }

        return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int8, Convert.ToUInt64(_value), true).ToMateriskValue();
    }
}