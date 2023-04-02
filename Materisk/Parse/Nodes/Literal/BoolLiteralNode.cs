using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using LLVMSharp.Interop;
using MateriskLLVM;

namespace Materisk.Parse.Nodes.Literal;

public class BoolLiteralNode : SyntaxNode
{
    public override NodeType Type => NodeType.BooleanLiteral;

    public bool Value { get; }

    public BoolLiteralNode(bool value)
    {
        Value = value;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        var value = Value ? 1 : 0;
        method.CilMethodBody!.Instructions.Add(CilInstruction.CreateLdcI4(value));
        return value;
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var value = Value ? 1 : 0;
        var llvmValue = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int1, Convert.ToUInt64(value), true);
        return llvmValue;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        return Enumerable.Empty<SyntaxNode>();
    }

    public override string ToString()
    {
        return "BoolNode: " + Value;
    }
}