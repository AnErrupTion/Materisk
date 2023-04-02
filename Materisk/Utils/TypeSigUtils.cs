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
                    "i32" => module.CorLibTypeFactory.Int32.MakeSzArrayType(),
                    "f32" => module.CorLibTypeFactory.Single.MakeSzArrayType(),
                    "u8" => module.CorLibTypeFactory.Byte.MakeSzArrayType(),
                    "str" => module.CorLibTypeFactory.String.MakeSzArrayType(),
                    "void" => throw new InvalidOperationException("Unable to make a void array!"),
                    _ => throw new InvalidOperationException("Unable to make an array for a custom type!")
                };
            }
            case "ptr" when secondName is not null:
            {
                return secondName switch
                {
                    "i32" => module.CorLibTypeFactory.Int32.MakePointerType(),
                    "f32" => module.CorLibTypeFactory.Single.MakePointerType(),
                    "u8" => module.CorLibTypeFactory.Byte.MakePointerType(),
                    "str" => throw new InvalidOperationException("Unable to make a string pointer!"),
                    "void" => module.CorLibTypeFactory.Void.MakePointerType(),
                    _ => throw new InvalidOperationException("Unable to make a pointer for a custom type!")
                };
            }
            case "i32": return module.CorLibTypeFactory.Int32;
            case "f32": return module.CorLibTypeFactory.Single;
            case "u8": return module.CorLibTypeFactory.Byte;
            case "str": return module.CorLibTypeFactory.String;
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
    public static LLVMTypeRef GetTypeSignatureFor(string name, string? secondName = null)
    {
        switch (name)
        {
            case "arr" when secondName is not null:
            case "ptr" when secondName is not null:
            {
                return secondName switch
                {
                    "i32" or "u32" => LLVMTypeRef.CreatePointer(LLVMTypeRef.Int32, 0),
                    "f32" => LLVMTypeRef.CreatePointer(LLVMTypeRef.Float, 0),
                    "i8" or "u8" => LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0),
                    "str" => throw new InvalidOperationException("Unable to make a string pointer!"),
                    "void" => LLVMTypeRef.CreatePointer(LLVMTypeRef.Void, 0),
                    _ => throw new InvalidOperationException("Unable to make a pointer for a custom type!")
                };
            }
            case "i32":
            case "u32":
            {
                return LLVMTypeRef.Int32;
            }
            case "f32":
            {
                return LLVMTypeRef.Float;
            }
            case "i8":
            case "u8":
            {
                return LLVMTypeRef.Int8;
            }
            case "str":
            {
                return LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0);
            }
            case "void":
            {
                return LLVMTypeRef.Void;
            }
            default:
            {
                throw new NotImplementedException($"Unimplemented type: {name}");
            }
        }
    }
}