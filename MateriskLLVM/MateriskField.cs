using LLVMSharp.Interop;

namespace MateriskLLVM;

public sealed class MateriskField
{
    public LLVMValueRef LlvmField;

    public readonly MateriskType ParentType;
    public readonly string Name;
    public readonly LLVMTypeRef Type;
    public readonly LLVMValueRef? Value;

    public unsafe MateriskField(MateriskType type, string name, LLVMTypeRef fieldType, LLVMValueRef? value)
    {
        var llvmValue = value ?? LLVM.ConstNull(fieldType);

        LlvmField = type.ParentModule.LlvmModule.AddGlobal(fieldType, name);
        LLVM.SetInitializer(LlvmField, llvmValue);

        ParentType = type;
        Name = name;
        Type = fieldType;
        Value = llvmValue;
    }
}