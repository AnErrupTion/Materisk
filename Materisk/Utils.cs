using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;

namespace Materisk;

public static class Utils
{
    public static TypeSignature GetTypeSignatureFor(ModuleDefinition module, string name, bool isArray = false)
    {
        switch (name)
        {
            case "int": return isArray ? module.CorLibTypeFactory.Int32.MakeSzArrayType() : module.CorLibTypeFactory.Int32;
            case "float": return isArray ? module.CorLibTypeFactory.Single.MakeSzArrayType() : module.CorLibTypeFactory.Single;
            case "string": return isArray ? module.CorLibTypeFactory.String.MakeSzArrayType() : module.CorLibTypeFactory.String;
            case "void": return isArray ? throw new InvalidOperationException("Unable to make a void array!") : module.CorLibTypeFactory.Void;
            default:
            {
                if (isArray)
                    throw new InvalidOperationException("Unable to make an array for a custom type!");

                foreach (var type in module.TopLevelTypes)
                    if (type.Name == name)
                        return type.ToTypeSignature();

                throw new NotImplementedException($"Unimplemented type: {name}");
            }
        }
    }
}