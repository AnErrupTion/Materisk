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