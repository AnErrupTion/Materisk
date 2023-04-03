using LLVMSharp.Interop;

namespace MateriskLLVM;

public static class LlvmUtils
{
    public static readonly LLVMValueRef ByteZero = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int8, 0, true);
    public static readonly LLVMValueRef IntZero = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0, true);
    public static readonly LLVMValueRef LongZero = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int64, 0, true);

    public static readonly LLVMTypeRef BytePointer = LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0);
    public static readonly LLVMTypeRef ShortPointer = LLVMTypeRef.CreatePointer(LLVMTypeRef.Int16, 0);
    public static readonly LLVMTypeRef IntPointer = LLVMTypeRef.CreatePointer(LLVMTypeRef.Int32, 0);
    public static readonly LLVMTypeRef LongPointer = LLVMTypeRef.CreatePointer(LLVMTypeRef.Int64, 0);
    public static readonly LLVMTypeRef FloatPointer = LLVMTypeRef.CreatePointer(LLVMTypeRef.Float, 0);
    public static readonly LLVMTypeRef DoublePointer = LLVMTypeRef.CreatePointer(LLVMTypeRef.Double, 0);
    public static readonly LLVMTypeRef VoidPointer = LLVMTypeRef.CreatePointer(LLVMTypeRef.Void, 0);

    public static readonly LLVMValueRef VoidNull = LLVMValueRef.CreateConstNull(LLVMTypeRef.Void);

    public static string MainFunctionNameOverride = "main";
}