using LLVMSharp.Interop;

namespace Materisk.TypeSystem;

public sealed class MateriskLocalVariable : MateriskUnit
{
    public readonly MateriskMethod ParentMethod;
    public readonly string Name;
    public readonly bool Mutable;
    public LLVMValueRef Value;

    public MateriskLocalVariable(MateriskMethod method, string name, bool mutable, LLVMTypeRef type, LLVMTypeRef pointerElementType, bool signed, LLVMValueRef value)
    {
        ParentMethod = method;
        Name = name;
        Mutable = mutable;
        Type = type;
        PointerElementType = pointerElementType;
        Signed = signed;
        Value = value;
    }

    public override LLVMValueRef Load()
        => Mutable ? ParentMethod.ParentType.ParentModule.LlvmBuilder.BuildLoad2(Type, Value) : Value;

    public override LLVMValueRef Store(LLVMValueRef value)
    {
        if (!Mutable)
            throw new InvalidOperationException($"Can not assign to an immutable variable: {ParentMethod.ParentType.ParentModule.Name}.{ParentMethod.ParentType.Name}.{ParentMethod.Name}.{Name}");

        return ParentMethod.ParentType.ParentModule.LlvmBuilder.BuildStore(value, Value);
    }
}