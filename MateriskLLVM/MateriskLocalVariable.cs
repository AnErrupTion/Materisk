using LLVMSharp.Interop;

namespace MateriskLLVM;

public sealed class MateriskLocalVariable
{
    public readonly string Name;
    public readonly bool Mutable;
    public LLVMValueRef Value;

    public MateriskLocalVariable(string name, bool mutable, LLVMValueRef value)
    {
        Name = name;
        Mutable = mutable;
        Value = value;
    }
}