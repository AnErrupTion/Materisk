using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using LLVMSharp.Interop;
using MateriskLLVM;

namespace Materisk.Parse.Nodes.Literal;

internal class FloatLiteralNode : SyntaxNode
{
    private readonly float _value;

    public FloatLiteralNode(float value)
    {
        _value = value;
    }

    public override NodeType Type => NodeType.FloatLiteral;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        return null!;
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        if (_value < 0)
        {
            var llvmValue = LLVMValueRef.CreateConstInt(LLVMTypeRef.Float, Convert.ToUInt64(Math.Abs(_value)), true);
            return LLVMValueRef.CreateConstSub(LlvmUtils.FloatZero, llvmValue);
        }

        return LLVMValueRef.CreateConstInt(LLVMTypeRef.Float, Convert.ToUInt64(_value), true);
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        return Enumerable.Empty<SyntaxNode>();
    }

    public override string ToString()
    {
        return "FloatLitNode:";
    }
}