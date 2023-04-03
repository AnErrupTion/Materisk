using LLVMSharp.Interop;

namespace MateriskLLVM;

public abstract class MateriskUnit
{
    public LLVMTypeRef Type { get; protected init; }

    public LLVMTypeRef PointerElementType { get; protected init; }

    public bool Signed { get; protected init; }

    public abstract LLVMValueRef Load();

    public abstract LLVMValueRef Store(LLVMValueRef value);
}