using LLVMSharp.Interop;
using MateriskLLVM;

namespace Materisk.Native;

public static class LlvmNativeFuncImpl
{
    public static void Emit(MateriskModule module, string typeName, MateriskMethod method)
    {
        switch (method.Name)
        {
            case "lenof":
            {
                var llvmType = LLVMTypeRef.CreateFunction(LLVMTypeRef.Int32,
                    new[] { LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0) });
                var strlen = module.LlvmModule.AddFunction("strlen", llvmType);
                strlen.Linkage = LLVMLinkage.LLVMExternalLinkage;

                var length = module.LlvmBuilder.BuildCall2(llvmType, strlen, method.LlvmMethod.Params);
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
                var llvmType = LLVMTypeRef.CreateFunction(LLVMTypeRef.Void,
                    new[] { LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0) });
                var printf = module.LlvmModule.AddFunction("printf", llvmType);
                printf.Linkage = LLVMLinkage.LLVMExternalLinkage;

                module.LlvmBuilder.BuildCall2(llvmType, printf, method.LlvmMethod.Params);
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