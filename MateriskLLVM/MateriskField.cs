using LLVMSharp.Interop;

namespace MateriskLLVM;

public sealed class MateriskField
{
    public LLVMValueRef LlvmField;

    public readonly MateriskType ParentType;
    public readonly string Name;
    public readonly LLVMTypeRef Type;

    public MateriskField(MateriskType type, string name, LLVMTypeRef fieldType)
    {
        LlvmField = type.ParentModule.LlvmModule.AddGlobal(fieldType, name);

        ParentType = type;
        Name = name;
        Type = fieldType;
    }
}