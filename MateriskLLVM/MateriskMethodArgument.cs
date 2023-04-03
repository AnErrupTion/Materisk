using LLVMSharp.Interop;

namespace MateriskLLVM;

public sealed class MateriskMethodArgument : MateriskUnit
{
    public MateriskMethod ParentMethod;
    public readonly string Name;
    public LLVMValueRef Value;

    public MateriskMethodArgument(string name, LLVMTypeRef type, LLVMTypeRef pointerElementType)
    {
        Name = name;
        Type = type;
        PointerElementType = pointerElementType;
    }

    public override LLVMValueRef Load() => Value;

    public override LLVMValueRef Store(LLVMValueRef value) => throw new NotImplementedException();
}