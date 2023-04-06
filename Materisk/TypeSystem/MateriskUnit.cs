using LLVMSharp.Interop;

namespace Materisk.TypeSystem;

public abstract class MateriskUnit
{
    public MateriskAttributes Attributes { get; protected init; }

    public LLVMTypeRef Type { get; protected set; }

    public LLVMTypeRef PointerElementType { get; protected init; }

    public bool Signed { get; protected init; }

    public abstract LLVMValueRef Load();

    public abstract LLVMValueRef Store(LLVMValueRef value);
}