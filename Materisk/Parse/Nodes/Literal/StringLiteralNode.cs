using LLVMSharp.Interop;
using Materisk.TypeSystem;
using Materisk.Utils;

namespace Materisk.Parse.Nodes.Literal;

internal class StringLiteralNode : SyntaxNode
{
    private readonly string _text;

    public StringLiteralNode(string text)
    {
        _text = text;
    }

    public override NodeType Type => NodeType.StringLiteral;

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, LLVMBasicBlockRef thenBlock, LLVMBasicBlockRef elseBlock)
    {
        var llvmType = LLVMTypeRef.Int8;
        var llvmValue = module.LlvmBuilder.BuildAlloca(LLVMTypeRef.CreateArray(llvmType, (uint)(_text.Length + 1)));

        // Store items
        for (var i = 0; i < _text.Length; i++)
        {
            var llvmChar = LLVMValueRef.CreateConstInt(llvmType, Convert.ToUInt64(_text[i]), true);
            var llvmIndex = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, Convert.ToUInt64(i), true);
            var llvmPtr = module.LlvmBuilder.BuildGEP2(llvmType, llvmValue, new[] { llvmIndex });
            module.LlvmBuilder.BuildStore(llvmChar, llvmPtr);
        }

        // Store null char
        var charIndex = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, Convert.ToUInt64(_text.Length), true);
        var charPtr = module.LlvmBuilder.BuildGEP2(llvmType, llvmValue, new[] { charIndex });
        module.LlvmBuilder.BuildStore(LlvmUtils.ByteZero, charPtr);

        return llvmValue.ToMateriskValue();
    }
}