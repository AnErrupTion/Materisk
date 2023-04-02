using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
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
        return null!;
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var value = _syntaxToken.Text;
        var llvmType = LLVMTypeRef.Int8;
        var llvmValue = module.LlvmBuilder.BuildAlloca(LLVMTypeRef.CreateArray(llvmType, (uint)(value.Length + 1)));

        // Store items
        for (var i = 0; i < value.Length; i++)
        {
            var llvmChar = LLVMValueRef.CreateConstInt(llvmType, Convert.ToUInt64(value[i]), true);
            var llvmIndex = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, Convert.ToUInt64(i), true);
            var llvmPtr = module.LlvmBuilder.BuildGEP2(llvmType, llvmValue, new[] { llvmIndex });
            module.LlvmBuilder.BuildStore(llvmChar, llvmPtr);
        }

        // Store null char
        var charIndex = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, Convert.ToUInt64(value.Length), true);
        var charPtr = module.LlvmBuilder.BuildGEP2(llvmType, llvmValue, new[] { charIndex });
        module.LlvmBuilder.BuildStore(LlvmUtils.ByteZero, charPtr);

        return llvmValue;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(_syntaxToken);
    }

    public override string ToString()
    {
        return "StrLitNode:";
    }
}