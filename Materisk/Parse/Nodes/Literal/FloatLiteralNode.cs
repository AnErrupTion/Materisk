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
        return LLVMValueRef.CreateConstReal(LLVMTypeRef.Float, Convert.ToDouble(_value));
    }
}