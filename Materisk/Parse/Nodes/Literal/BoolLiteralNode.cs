using LLVMSharp.Interop;
using Materisk.TypeSystem;

namespace Materisk.Parse.Nodes.Literal;

public class BoolLiteralNode : SyntaxNode
{
    public override NodeType Type => NodeType.BooleanLiteral;

    private readonly bool _value;

    public BoolLiteralNode(bool value)
    {
        _value = value;
    }

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, LLVMBasicBlockRef thenBlock, LLVMBasicBlockRef elseBlock)
    {
        var value = _value ? 1 : 0;
        return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int1, Convert.ToUInt64(value), true).ToMateriskValue();
    }
}