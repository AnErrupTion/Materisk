using LLVMSharp.Interop;

namespace MateriskLLVM;

public static class LlvmUtils
{
    public static readonly LLVMValueRef ByteZero = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int8, 0, true);
    public static readonly LLVMValueRef IntZero = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0, true);
    public static readonly LLVMValueRef FloatZero = LLVMValueRef.CreateConstInt(LLVMTypeRef.Float, 0, true);

    public static string MainFunctionNameOverride = "main";
}