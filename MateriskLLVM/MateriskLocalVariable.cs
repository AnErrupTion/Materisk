using LLVMSharp.Interop;

namespace MateriskLLVM;

public sealed class MateriskLocalVariable
{
    public readonly string Name;
    public readonly bool Mutable;
    public readonly LLVMTypeRef Type;
    public LLVMValueRef Value;

    public MateriskLocalVariable(string name, bool mutable, LLVMTypeRef type, LLVMValueRef value)
    {
        Name = name;
        Mutable = mutable;
        Type = type;
        Value = value;
    }
}