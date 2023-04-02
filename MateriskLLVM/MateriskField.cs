using LLVMSharp.Interop;

namespace MateriskLLVM;

public sealed class MateriskField : MateriskUnit
{
    public LLVMValueRef LlvmField;

    public readonly MateriskType ParentType;
    public readonly string Name;

    public MateriskField(MateriskType type, string name, LLVMTypeRef fieldType)
    {
        LlvmField = type.ParentModule.LlvmModule.AddGlobal(fieldType, name);
        LlvmField.Initializer = LLVMValueRef.CreateConstNull(fieldType);

        ParentType = type;
        Name = name;
        Type = fieldType;
    }

    public override LLVMValueRef Load() => ParentType.ParentModule.LlvmBuilder.BuildLoad2(Type, LlvmField);
}