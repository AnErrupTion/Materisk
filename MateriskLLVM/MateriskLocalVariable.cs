using LLVMSharp.Interop;

namespace MateriskLLVM;

public sealed class MateriskLocalVariable
{
    public readonly string Name;
    public readonly bool Mutable;
    public readonly LLVMTypeRef Type;
    public readonly LLVMTypeRef PointerElementType;
    public LLVMValueRef Value;

    public MateriskLocalVariable(string name, bool mutable, LLVMTypeRef type, LLVMTypeRef pointerElementType, LLVMValueRef value)
    {
        Name = name;
        Mutable = mutable;
        Type = type;
        PointerElementType = pointerElementType;
        Value = value;
    }
}