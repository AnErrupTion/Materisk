using LLVMSharp.Interop;
using Materisk.TypeSystem;
using Materisk.Utils;

namespace Materisk.Parse.Nodes.Literal;

internal class IntLiteralNode : SyntaxNode
{
    private readonly int _value;

    public IntLiteralNode(int value)
    {
        _value = value;
    }

    public override NodeType Type => NodeType.IntLiteral;

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        if (_value < 0)
        {
            var llvmValue = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, Convert.ToUInt64(Math.Abs(_value)), true);
            return LLVMValueRef.CreateConstSub(LlvmUtils.IntZero, llvmValue).ToMateriskValue();
        }

        return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, Convert.ToUInt64(_value), true).ToMateriskValue();
    }
}