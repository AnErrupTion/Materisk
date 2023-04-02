using LLVMSharp.Interop;

namespace MateriskLLVM;

public abstract class MateriskUnit
{
    public LLVMTypeRef Type { get; protected init; }

    public LLVMTypeRef PointerElementType { get; protected init; }

    public abstract LLVMValueRef Load();
}