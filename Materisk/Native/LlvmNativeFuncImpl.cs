using LLVMSharp.Interop;
using Materisk.TypeSystem;
using Materisk.Utils;

namespace Materisk.Native;

internal static class LlvmNativeFuncImpl
{
    public static LLVMTypeRef MallocType, FreeType, PrintfType;
    public static LLVMValueRef Malloc, Free, Printf;

    public static void Emit(MateriskModule module, string typeName, MateriskMethod method)
    {
        switch (typeName)
        {
            case "Memory" when method.Name is "allocate":
            {
                MallocType = LLVMTypeRef.CreateFunction(LlvmUtils.VoidPointer, new[] { LLVMTypeRef.Int64 });
                Malloc = module.LlvmModule.AddFunction("malloc", MallocType);
                Malloc.Linkage = LLVMLinkage.LLVMExternalLinkage;

                var pointer = module.LlvmBuilder.BuildCall2(MallocType, Malloc, method.LlvmMethod.Params);
                module.LlvmBuilder.BuildRet(pointer);
                return;
            }
            case "Memory" when method.Name is "free":
            {
                FreeType = LLVMTypeRef.CreateFunction(LLVMTypeRef.Void,  new[] { LlvmUtils.VoidPointer });
                Free = module.LlvmModule.AddFunction("free", FreeType);
                Free.Linkage = LLVMLinkage.LLVMExternalLinkage;

                module.LlvmBuilder.BuildCall2(FreeType, Free, method.LlvmMethod.Params);
                module.LlvmBuilder.BuildRetVoid();
                return;
            }
            case "File" when method.Name is "readText":
            {
                module.LlvmBuilder.BuildRetVoid();
                return;
            }
            case "File" when method.Name is "writeText":
            {
                module.LlvmBuilder.BuildRetVoid();
                return;
            }
            case "Console" when method.Name is "print":
            {
                PrintfType = LLVMTypeRef.CreateFunction(LLVMTypeRef.Void, new[] { LlvmUtils.BytePointer });
                Printf = module.LlvmModule.AddFunction("printf", PrintfType);
                Printf.Linkage = LLVMLinkage.LLVMExternalLinkage;

                module.LlvmBuilder.BuildCall2(PrintfType, Printf, method.LlvmMethod.Params);
                module.LlvmBuilder.BuildRetVoid();
                return;
            }
            default: throw new NotImplementedException($"Unimplemented native method: {module.Name}.{typeName}.{method.Name}");
        }
    }
}