using LLVMSharp.Interop;

namespace Materisk.TypeSystem;

public class MateriskValue : MateriskUnit
{
    private readonly LLVMValueRef _value;

    public MateriskValue(LLVMValueRef value) => _value = value;

    public override LLVMValueRef Load() => _value;

    public override LLVMValueRef Store(LLVMValueRef value) => throw new NotSupportedException();
}