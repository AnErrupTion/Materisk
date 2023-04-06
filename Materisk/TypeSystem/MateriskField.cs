using LLVMSharp.Interop;

namespace Materisk.TypeSystem;

public sealed class MateriskField : MateriskUnit
{
    public LLVMValueRef LlvmField;

    public readonly MateriskType ParentType;
    public readonly string Name;

    public MateriskField(MateriskType type, string name, MateriskAttributes attributes, LLVMTypeRef fieldType, LLVMTypeRef pointerElementType, bool signed)
    {
        if (type.Attributes.HasFlag(MateriskAttributes.Static))
        {
            LlvmField = type.ParentModule.LlvmModule.AddGlobal(fieldType, $"{type.Name}_{name}");
            LlvmField.Initializer = LLVMValueRef.CreateConstNull(fieldType);
        }

        ParentType = type;
        Name = name;
        Attributes = attributes;
        Type = fieldType;
        PointerElementType = pointerElementType;
        Signed = signed;
    }

    public override LLVMValueRef Load() => ParentType.ParentModule.LlvmBuilder.BuildLoad2(Type,
        ParentType.Attributes.HasFlag(MateriskAttributes.Static)
        ? ParentType.ParentModule.LlvmBuilder.BuildStructGEP2(Type, ParentType.Load(), 0)
        : LlvmField);

    public override LLVMValueRef Store(LLVMValueRef value) =>
        ParentType.ParentModule.LlvmBuilder.BuildStore(value, LlvmField);
}