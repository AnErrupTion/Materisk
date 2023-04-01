using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using LLVMSharp.Interop;
using MateriskLLVM;
using Materisk.Lex;
using Materisk.Parse.Nodes.Misc;

namespace Materisk.Parse.Nodes.Literal;

internal class StringLiteralNode : SyntaxNode
{
    private readonly SyntaxToken _syntaxToken;

    public StringLiteralNode(SyntaxToken syntaxToken)
    {
        _syntaxToken = syntaxToken;
    }

    public override NodeType Type => NodeType.StringLiteral;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        var value = _syntaxToken.Text;
        method.CilMethodBody!.Instructions.Add(CilOpCodes.Ldstr, value);
        return value;
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method)
    {
        // TODO: Is this right?
        var value = _syntaxToken.Text;
        var llvmValue = module.LlvmBuilder.BuildArrayAlloca(LLVMTypeRef.Int8, LLVMValueRef.CreateConstInt(LLVMTypeRef.Int8, Convert.ToUInt64(value.Length), true));
        for (var i = 0; i < value.Length; i++)
        {
            var llvmChar = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int8, Convert.ToUInt64(value[i]), true);
            var llvmIndex = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, Convert.ToUInt64(i), true);
            var llvmPtr = module.LlvmBuilder.BuildGEP(llvmValue, new[] { llvmIndex });
            module.LlvmBuilder.BuildStore(llvmChar, llvmPtr);
        }
        return llvmValue;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(_syntaxToken);
    }

    public override string ToString()
    {
        return "StringLitNode:";
    }
}