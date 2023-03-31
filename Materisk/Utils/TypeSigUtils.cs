using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;

namespace Materisk.Utils;

internal static class TypeSigUtils
{
    public static TypeSignature GetTypeSignatureFor(ModuleDefinition module, string name, string? secondName = null)
    {
        switch (name)
        {
            case "arr" when secondName is not null:
            {
                return secondName switch
                {
                    "int" => module.CorLibTypeFactory.Int32.MakeSzArrayType(),
                    "float" => module.CorLibTypeFactory.Single.MakeSzArrayType(),
                    "byte" => module.CorLibTypeFactory.Byte.MakeSzArrayType(),
                    "string" => module.CorLibTypeFactory.String.MakeSzArrayType(),
                    "void" => throw new InvalidOperationException("Unable to make a void array!"),
                    _ => throw new InvalidOperationException("Unable to make an array for a custom type!")
                };
            }
            case "ptr" when secondName is not null:
            {
                return secondName switch
                {
                    "int" => module.CorLibTypeFactory.Int32.MakePointerType(),
                    "float" => module.CorLibTypeFactory.Single.MakePointerType(),
                    "byte" => module.CorLibTypeFactory.Byte.MakePointerType(),
                    "string" => module.CorLibTypeFactory.String.MakePointerType(),
                    "void" => module.CorLibTypeFactory.Void.MakePointerType(),
                    _ => throw new InvalidOperationException("Unable to make a pointer for a custom type!")
                };
            }
            case "int": return module.CorLibTypeFactory.Int32;
            case "float": return module.CorLibTypeFactory.Single;
            case "byte": return module.CorLibTypeFactory.Byte;
            case "string": return module.CorLibTypeFactory.String;
            case "void": return module.CorLibTypeFactory.Void;
            default:
            {
                foreach (var type in module.TopLevelTypes)
                    if (type.Name == name)
                        return type.ToTypeSignature();

                throw new NotImplementedException($"Unimplemented type: {name}");
            }
        }
    }
}