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
        var values = new LLVMValueRef[_text.Length + 1];

        for (var i = 0; i < _text.Length; i++)
            values[i] = LLVMValueRef.CreateConstInt(llvmType, Convert.ToUInt64(_text[i]), true);

        values[_text.Length] = LlvmUtils.ByteZero;

        var global = module.LlvmModule.AddGlobal(
            LLVMTypeRef.CreateArray(llvmType, (uint)(_text.Length + 1)),
            $"str_{module.Counter++}");
        global.Initializer = LLVMValueRef.CreateConstArray(llvmType, values);
        global.Linkage = LLVMLinkage.LLVMInternalLinkage;
        global.IsGlobalConstant = true;
        return global.ToMateriskValue();
    }
}