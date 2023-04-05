using LLVMSharp.Interop;
using MateriskLLVM;

namespace Materisk.Utils;

internal static class TypeSigUtils
{
    public static LLVMTypeRef GetTypeSignatureFor(MateriskModule module, string name, string secondName = "")
    {
        switch (name)
        {
            case "arr" or "ptr" when !string.IsNullOrEmpty(secondName):
            {
                return secondName switch
                {
                    "i8" or "u8" => LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0),
                    "i16" or "u16" => LLVMTypeRef.CreatePointer(LLVMTypeRef.Int16, 0),
                    "i32" or "u32" => LLVMTypeRef.CreatePointer(LLVMTypeRef.Int32, 0),
                    "i64" or "u64" => LLVMTypeRef.CreatePointer(LLVMTypeRef.Int64, 0),
                    "f32" => LLVMTypeRef.CreatePointer(LLVMTypeRef.Float, 0),
                    "f64" => LLVMTypeRef.CreatePointer(LLVMTypeRef.Double, 0),
                    "str" => LLVMTypeRef.CreatePointer(LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0), 0),
                    "bool" => LLVMTypeRef.CreatePointer(LLVMTypeRef.Int1, 0),
                    "void" => LLVMTypeRef.CreatePointer(LLVMTypeRef.Void, 0),
                    _ => throw new InvalidOperationException("Unable to make a pointer for a custom type!")
                };
            }
            case "i8":
            case "u8":
            {
                return LLVMTypeRef.Int8;
            }
            case "i16":
            case "u16":
            {
                return LLVMTypeRef.Int16;
            }
            case "i32":
            case "u32":
            {
                return LLVMTypeRef.Int32;
            }
            case "i64":
            case "u64":
            {
                return LLVMTypeRef.Int64;
            }
            case "f32":
            {
                return LLVMTypeRef.Float;
            }
            case "f64":
            {
                return LLVMTypeRef.Double;
            }
            case "str":
            {
                return LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0);
            }
            case "bool":
            {
                return LLVMTypeRef.Int1;
            }
            case "void":
            {
                return LLVMTypeRef.Void;
            }
            default:
            {
                foreach (var type in module.Types)
                    if (type.Name == name)
                        return type.Type;

                throw new NotImplementedException($"Unimplemented type: {name}");
            }
        }
    }
}