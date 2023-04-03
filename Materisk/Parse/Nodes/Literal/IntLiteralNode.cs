using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using LLVMSharp.Interop;
using MateriskLLVM;

namespace Materisk.Parse.Nodes.Literal;

internal class IntLiteralNode : SyntaxNode
{
    private readonly int _value;

    public IntLiteralNode(int value)
    {
        _value = value;
    }

    public override NodeType Type => NodeType.IntLiteral;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        return null!;
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        if (_value < 0)
        {
            var llvmValue = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, Convert.ToUInt64(Math.Abs(_value)), true);
            return LLVMValueRef.CreateConstSub(LlvmUtils.IntZero, llvmValue);
        }

        return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, Convert.ToUInt64(_value), true);
    }
}