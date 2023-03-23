using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;

namespace Materisk.Native;

public static class CilNativeFuncImpl
{
    public static void Emit(ModuleDefinition module, string typeName, MethodDefinition method)
    {
        if (typeName == "File" && method.Name == "readText")
        {
            var factory = module.CorLibTypeFactory;
            var importedMethod = factory.CorLibScope
                .CreateTypeReference("System", "File")
                .CreateMemberReference("ReadAllText", MethodSignature.CreateStatic(factory.String, factory.String))
                .ImportWith(module.DefaultImporter);

            method.CilMethodBody?.Instructions.Add(CilOpCodes.Ldarg, method.Parameters[0]);
            method.CilMethodBody?.Instructions.Add(CilOpCodes.Call, importedMethod);
        }
        else if (typeName == "Console" && method.Name == "printLine")
        {
            var factory = module.CorLibTypeFactory;
            var importedMethod = factory.CorLibScope
                .CreateTypeReference("System", "Console")
                .CreateMemberReference("WriteLine", MethodSignature.CreateStatic(factory.Void, factory.String))
                .ImportWith(module.DefaultImporter);

            method.CilMethodBody?.Instructions.Add(CilOpCodes.Ldarg, method.Parameters[0]);
            method.CilMethodBody?.Instructions.Add(CilOpCodes.Call, importedMethod);
        }
    }
}