using LLVMSharp.Interop;
using MateriskLLVM;

namespace Materisk.Native;

public static class LlvmNativeFuncImpl
{
    public static void Emit(MateriskModule module, string typeName, MateriskMethod method)
    {
        switch (typeName)
        {
            case "File" when method.Name == "readText":
            {
                module.LlvmBuilder.BuildRetVoid();
                break;
            }
            case "File" when method.Name == "writeText":
            {
                module.LlvmBuilder.BuildRetVoid();
                break;
            }
            case "Console" when method.Name == "print":
            {
                var printf = module.LlvmModule.AddFunction("printf",
                    LLVMTypeRef.CreateFunction(LLVMTypeRef.Void,
                        new[] { LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0) }));
                printf.Linkage = LLVMLinkage.LLVMExternalLinkage;

                module.LlvmBuilder.BuildCall(printf, method.LlvmMethod.Params);
                module.LlvmBuilder.BuildRetVoid();
                break;
            }
            case "Int" when method.Name == "toString":
            {
                module.LlvmBuilder.BuildRetVoid();
                break;
            }
        }
    }
}