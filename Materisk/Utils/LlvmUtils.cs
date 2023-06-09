using LLVMSharp.Interop;
using Materisk.TypeSystem;

namespace Materisk.Utils;

internal static class LlvmUtils
{
    public static readonly LLVMValueRef ByteZero = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int8, 0, true);
    public static readonly LLVMValueRef ShortZero = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int16, 0, true);
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
    public static string TargetTriple;
    public static LLVMTargetMachineRef TargetMachine;
    public static LLVMTargetDataRef DataLayout;
    public static bool NoStdLib;

    public static void Initialize(string targetTriple, string cpu, string features, bool noStdLib)
    {
        LLVM.InitializeAllTargetInfos();
        LLVM.InitializeAllTargets();
        LLVM.InitializeAllTargetMCs();
        LLVM.InitializeAllAsmParsers();
        LLVM.InitializeAllAsmPrinters();

        TargetTriple = targetTriple;
        TargetMachine = LLVMTargetRef.GetTargetFromTriple(targetTriple).CreateTargetMachine(
            targetTriple,
            cpu, features,
            LLVMCodeGenOptLevel.LLVMCodeGenLevelNone,
            LLVMRelocMode.LLVMRelocDefault,
            LLVMCodeModel.LLVMCodeModelDefault);
        DataLayout = TargetMachine.CreateTargetDataLayout();
        NoStdLib = noStdLib;
    }

    public static ulong GetAllocateSize(List<MateriskField> fields)
    {
        ulong allocateSize = 0;

        foreach (var field in fields)
        {
            if (field.Type.SubtypesCount > 0)
            {
                MateriskType? type = null;
                foreach (var mType in field.ParentType.ParentModule.Types)
                    if (mType.Name == field.TypeName)
                    {
                        type = mType;
                        break;
                    }

                if (type is null)
                    throw new InvalidOperationException($"Could not find type with name \"{field.TypeName}\" in module: {field.ParentType.ParentModule.Name}");

                allocateSize += GetAllocateSize(type.Fields);
            } else allocateSize += DataLayout.SizeOfTypeInBits(field.Type);
        }

        return allocateSize;
    }
}