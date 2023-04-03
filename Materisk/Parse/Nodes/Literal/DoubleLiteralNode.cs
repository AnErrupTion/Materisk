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
        return LLVMValueRef.CreateConstReal(LLVMTypeRef.Double, _value);
    }
}