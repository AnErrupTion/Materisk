using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using LLVMSharp.Interop;
using MateriskLLVM;

namespace Materisk.Parse.Nodes.Literal;

internal class DoubleLiteralNode : SyntaxNode
{
    private readonly double _value;

    public DoubleLiteralNode(double value)
    {
        _value = value;
    }

    public override NodeType Type => NodeType.DoubleLiteral;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        return null!;
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        if (_value < 0)
        {
            var llvmValue = LLVMValueRef.CreateConstInt(LLVMTypeRef.Double, Convert.ToUInt64(Math.Abs(_value)), true);
            return LLVMValueRef.CreateConstSub(LlvmUtils.DoubleZero, llvmValue);
        }

        return LLVMValueRef.CreateConstInt(LLVMTypeRef.Double, Convert.ToUInt64(_value), true);
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        return Enumerable.Empty<SyntaxNode>();
    }

    public override string ToString()
    {
        return "DoubleLitNode:";
    }
}