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

            method.CilMethodBody!.Instructions.Add(CilOpCodes.Ldarg, method.Parameters[0]);
            method.CilMethodBody!.Instructions.Add(CilOpCodes.Call, importedMethod);
        }
        else if (typeName == "Console" && method.Name == "print")
        {
            var factory = module.CorLibTypeFactory;
            var importedMethod = factory.CorLibScope
                .CreateTypeReference("System", "Console")
                .CreateMemberReference("Write", MethodSignature.CreateStatic(factory.Void, factory.String))
                .ImportWith(module.DefaultImporter);

            method.CilMethodBody!.Instructions.Add(CilOpCodes.Ldarg, method.Parameters[0]);
            method.CilMethodBody!.Instructions.Add(CilOpCodes.Call, importedMethod);
        }
        else if (typeName == "String" && method.Name == "toString")
        {
            if (method.Parameters[0].ParameterType.Name is "Int32")
            {
                var factory = module.CorLibTypeFactory;
                var importedMethod = factory.CorLibScope
                    .CreateTypeReference("System", "Int32")
                    .CreateMemberReference("ToString", MethodSignature.CreateInstance(factory.String))
                    .ImportWith(module.DefaultImporter);

                method.CilMethodBody!.Instructions.Add(CilOpCodes.Ldarga, method.Parameters[0]);
                method.CilMethodBody!.Instructions.Add(CilOpCodes.Call, importedMethod);
            }
        }
    }
}