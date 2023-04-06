using LLVMSharp.Interop;
using Materisk.TypeSystem;

namespace Materisk.Parse.Nodes.Literal;

internal class DoubleLiteralNode : SyntaxNode
{
    private readonly double _value;

    public DoubleLiteralNode(double value)
    {
        _value = value;
    }

    public override NodeType Type => NodeType.DoubleLiteral;

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, LLVMBasicBlockRef thenBlock, LLVMBasicBlockRef elseBlock)
    {
        return LLVMValueRef.CreateConstReal(LLVMTypeRef.Double, _value).ToMateriskValue();
    }
}