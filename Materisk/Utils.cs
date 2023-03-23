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

        if (type == typeof(void))
            return module.CorLibTypeFactory.Void;

        throw new NotImplementedException($"Unimplemented type: {type}");
    }

    public static TypeSignature GetTypeSignatureFor(ModuleDefinition module, string type)
    {
        return type switch
        {
            "int" => module.CorLibTypeFactory.Int32,
            "float" => module.CorLibTypeFactory.Single,
            "string" => module.CorLibTypeFactory.String,
            "void" => module.CorLibTypeFactory.Void,
            _ => throw new NotImplementedException($"Unimplemented type: {type}")
        };
    }
}