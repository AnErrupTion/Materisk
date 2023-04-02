using LLVMSharp.Interop;

namespace MateriskLLVM;

public sealed class MateriskMethodArgument : MateriskUnit
{
    public readonly string Name;
    public LLVMValueRef Value;

    public MateriskMethodArgument(string name, LLVMTypeRef type, LLVMTypeRef pointerElementType)
    {
        Name = name;
        Type = type;
        PointerElementType = pointerElementType;
    }

    public override LLVMValueRef Load() => Value;
}