using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;

namespace Materisk;

public static class Utils
{
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