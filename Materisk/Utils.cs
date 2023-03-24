using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;

namespace Materisk;

public static class Utils
{
    public static TypeSignature GetTypeSignatureFor(ModuleDefinition module, string name)
    {
        switch (name)
        {
            case "int": return module.CorLibTypeFactory.Int32;
            case "float": return module.CorLibTypeFactory.Single;
            case "string": return module.CorLibTypeFactory.String;
            case "void": return module.CorLibTypeFactory.Void;
            default:
                foreach (var type in module.TopLevelTypes)
                    if (type.Name == name)
                        return type.ToTypeSignature();
                throw new NotImplementedException($"Unimplemented type: {name}");
        }
    }
}