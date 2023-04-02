using LLVMSharp.Interop;
using MateriskLLVM;

namespace Materisk.Native;

public static class LlvmNativeFuncImpl
{
    public static LLVMTypeRef MallocType, FreeType, StrlenType, PrintfType, SprintfType;
    public static LLVMValueRef Malloc, Free, Strlen, Printf, Sprintf;

    public static void Emit(MateriskModule module, string typeName, MateriskMethod method)
    {
        switch (method.Name)
        {
            case "allocate":
            {
                MallocType = LLVMTypeRef.CreateFunction(LlvmUtils.VoidPointer, new[] { LLVMTypeRef.Int32 });
                Malloc = module.LlvmModule.AddFunction("malloc", MallocType);
                Malloc.Linkage = LLVMLinkage.LLVMExternalLinkage;

                var pointer = module.LlvmBuilder.BuildCall2(MallocType, Malloc, method.LlvmMethod.Params);
                module.LlvmBuilder.BuildRet(pointer);
                return;
            }
            case "free":
            {
                FreeType = LLVMTypeRef.CreateFunction(LLVMTypeRef.Void,  new[] { LlvmUtils.VoidPointer });
                Free = module.LlvmModule.AddFunction("free", FreeType);
                Free.Linkage = LLVMLinkage.LLVMExternalLinkage;

                module.LlvmBuilder.BuildCall2(FreeType, Free, method.LlvmMethod.Params);
                module.LlvmBuilder.BuildRetVoid();
                return;
            }
            case "lenof":
            {
                StrlenType = LLVMTypeRef.CreateFunction(LLVMTypeRef.Int32, new[] { LlvmUtils.BytePointer });
                Strlen = module.LlvmModule.AddFunction("strlen", StrlenType);
                Strlen.Linkage = LLVMLinkage.LLVMExternalLinkage;

                var length = module.LlvmBuilder.BuildCall2(StrlenType, Strlen, method.LlvmMethod.Params);
                module.LlvmBuilder.BuildRet(length);
                return;
            }
        }
        switch (typeName)
        {
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
            case "Int" when method.Name is "toString":
            {
                module.LlvmBuilder.BuildRetVoid();
                return;
            }
            default: throw new NotImplementedException($"Unimplemented native method: {typeName}.{method.Name}");
        }
    }
}