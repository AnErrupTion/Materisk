using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;
using LLVMSharp.Interop;

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
                    "string" => throw new InvalidOperationException("Unable to make a string pointer!"),
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

    // TODO: Structs as types
    public static LLVMTypeRef GetTypeSignatureFor(string name, uint arrayElementCount = 0, string? secondName = null)
    {
        switch (name)
        {
            case "arr" when secondName is not null:
            {
                return secondName switch
                {
                    "int" => LLVMTypeRef.CreateArray(LLVMTypeRef.Int32, arrayElementCount),
                    "float" => LLVMTypeRef.CreateArray(LLVMTypeRef.Float, arrayElementCount),
                    "byte" => LLVMTypeRef.CreateArray(LLVMTypeRef.Int8, arrayElementCount),
                    "string" => throw new InvalidOperationException("Unable to make a string array!"), // TODO: String array?
                    "void" => LLVMTypeRef.CreateArray(LLVMTypeRef.Void, arrayElementCount),
                    _ => throw new InvalidOperationException("Unable to make a pointer for a custom type!")
                };
            }
            case "ptr" when secondName is not null:
            {
                return secondName switch
                {
                    "int" => LLVMTypeRef.CreatePointer(LLVMTypeRef.Int32, 0),
                    "float" => LLVMTypeRef.CreatePointer(LLVMTypeRef.Float, 0),
                    "byte" => LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0),
                    "string" => throw new InvalidOperationException("Unable to make a string pointer!"),
                    "void" => LLVMTypeRef.CreatePointer(LLVMTypeRef.Void, 0),
                    _ => throw new InvalidOperationException("Unable to make a pointer for a custom type!")
                };
            }
            case "int": return LLVMTypeRef.Int32;
            case "float": return LLVMTypeRef.Float;
            case "byte": return LLVMTypeRef.Int8;
            case "string": return LLVMTypeRef.CreateArray(LLVMTypeRef.Int8, arrayElementCount);
            case "void": return LLVMTypeRef.Void;
            default:
            {
                throw new NotImplementedException($"Unimplemented type: {name}");
            }
        }
    }
}