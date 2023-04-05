using LLVMSharp.Interop;
using Materisk.TypeSystem;

namespace Materisk.Parse.Nodes.Literal;

internal class FloatLiteralNode : SyntaxNode
{
    private readonly float _value;

    public FloatLiteralNode(float value)
    {
        _value = value;
    }

    public override NodeType Type => NodeType.FloatLiteral;

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        return LLVMValueRef.CreateConstReal(LLVMTypeRef.Float, Convert.ToDouble(_value)).ToMateriskValue();
    }
}