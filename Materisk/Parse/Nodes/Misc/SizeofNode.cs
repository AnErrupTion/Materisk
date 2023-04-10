using LLVMSharp.Interop;
using Materisk.Parse.Nodes.Identifier;
using Materisk.TypeSystem;
using Materisk.Utils;

namespace Materisk.Parse.Nodes.Misc;

internal class SizeofNode : SyntaxNode
{
    private readonly string _identifier;

    public SizeofNode(string identifier)
    {
        _identifier = identifier;
    }

    public override NodeType Type => NodeType.Sizeof;

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, LLVMBasicBlockRef thenBlock, LLVMBasicBlockRef elseBlock)
    {
        switch (_identifier)
        {
            case "bool":
            {
                return LLVMValueRef.CreateConstInt(
                    LLVMTypeRef.Int64,
                    LlvmUtils.DataLayout.SizeOfTypeInBits(LLVMTypeRef.Int1) / 8,
                    true).ToMateriskValue();
            }
            case "i8" or "u8":
            {
                return LLVMValueRef.CreateConstInt(
                    LLVMTypeRef.Int64,
                    LlvmUtils.DataLayout.SizeOfTypeInBits(LLVMTypeRef.Int8) / 8,
                    true).ToMateriskValue();
            }
            case "i16" or "u16":
            {
                return LLVMValueRef.CreateConstInt(
                    LLVMTypeRef.Int64,
                    LlvmUtils.DataLayout.SizeOfTypeInBits(LLVMTypeRef.Int16) / 8,
                    true).ToMateriskValue();
            }
            case "i32" or "u32":
            {
                return LLVMValueRef.CreateConstInt(
                    LLVMTypeRef.Int64,
                    LlvmUtils.DataLayout.SizeOfTypeInBits(LLVMTypeRef.Int32) / 8,
                    true).ToMateriskValue();
            }
            case "i64" or "u64":
            {
                return LLVMValueRef.CreateConstInt(
                    LLVMTypeRef.Int64,
                    LlvmUtils.DataLayout.SizeOfTypeInBits(LLVMTypeRef.Int64) / 8,
                    true).ToMateriskValue();
            }
            case "f32":
            {
                return LLVMValueRef.CreateConstInt(
                    LLVMTypeRef.Int64,
                    LlvmUtils.DataLayout.SizeOfTypeInBits(LLVMTypeRef.Float) / 8,
                    true).ToMateriskValue();
            }
            case "f64":
            {
                return LLVMValueRef.CreateConstInt(
                    LLVMTypeRef.Int64,
                    LlvmUtils.DataLayout.SizeOfTypeInBits(LLVMTypeRef.Double) / 8,
                    true).ToMateriskValue();
            }
            case "str": throw new InvalidOperationException("Unable to get the size of str!");
            case "void": throw new InvalidOperationException("Unable to get the size of void!");
            default:
            {
                var identifier = new IdentifierNode(_identifier).Emit(module, type, method, thenBlock, elseBlock);

                if (identifier is not MateriskType mType)
                    throw new InvalidOperationException($"Identifier is not type: {identifier}");

                if (!mType.Attributes.HasFlag(MateriskAttributes.Struct))
                    throw new InvalidOperationException($"Type is not struct: {mType.ParentModule.Name}.{mType.Name}");

                return LLVMValueRef.CreateConstInt(
                    LLVMTypeRef.Int64,
                    LlvmUtils.GetAllocateSize(mType.Fields) / 8,
                    true).ToMateriskValue();
            }
        }
    }
}