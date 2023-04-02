using System.Globalization;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using LLVMSharp.Interop;
using MateriskLLVM;
using Materisk.Lex;
using Materisk.Parse.Nodes.Misc;

namespace Materisk.Parse.Nodes.Literal;

internal class ByteLiteralNode : SyntaxNode
{
    private readonly SyntaxToken _syntaxToken;

    public ByteLiteralNode(SyntaxToken syntaxToken)
    {
        _syntaxToken = syntaxToken;
    }

    public override NodeType Type => NodeType.ByteLiteral;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        var value = byte.Parse(_syntaxToken.Text, CultureInfo.InvariantCulture);
        method.CilMethodBody!.Instructions.Add(CilInstruction.CreateLdcI4(value));
        method.CilMethodBody!.Instructions.Add(CilOpCodes.Conv_I1);
        return value;
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var value = byte.Parse(_syntaxToken.Text, CultureInfo.InvariantCulture);
        var llvmValue = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int8, Convert.ToUInt64(value), true);
        return llvmValue;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(_syntaxToken);
    }

    public override string ToString()
    {
        return "ByteLitNode:";
    }
}