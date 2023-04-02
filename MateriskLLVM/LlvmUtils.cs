using LLVMSharp.Interop;

namespace MateriskLLVM;

public static class LlvmUtils
{
    public static readonly LLVMValueRef ByteZero = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int8, 0, true);
    public static readonly LLVMValueRef IntZero = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0, true);
    public static readonly LLVMValueRef LongZero = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int64, 0, true);
    public static readonly LLVMValueRef FloatZero = LLVMValueRef.CreateConstInt(LLVMTypeRef.Float, 0, true);
    public static readonly LLVMValueRef DoubleZero = LLVMValueRef.CreateConstInt(LLVMTypeRef.Double, 0, true);

    public static readonly LLVMTypeRef BytePointer = LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0);
    public static readonly LLVMTypeRef VoidPointer = LLVMTypeRef.CreatePointer(LLVMTypeRef.Void, 0);

    public static string MainFunctionNameOverride = "main";
}