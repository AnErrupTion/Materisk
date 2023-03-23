using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;

namespace Materisk;

public static class Utils
{
    public static TypeSignature GetTypeSignatureFor(ModuleDefinition module, Type type)
    {
        if (type == typeof(int))
            return module.CorLibTypeFactory.Int32;

        if (type == typeof(float))
            return module.CorLibTypeFactory.Single;

        if (type == typeof(string))
            return module.CorLibTypeFactory.String;

        throw new NotImplementedException($"Unimplemented type: {type}");
    }
}