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

    public override LLVMValueRef Load() => !ParentType.Attributes.HasFlag(MateriskAttributes.Static)
        ? throw new NotSupportedException()
        : ParentType.ParentModule.LlvmBuilder.BuildLoad2(Type, LlvmField);

    public override LLVMValueRef Store(LLVMValueRef value) => !ParentType.Attributes.HasFlag(MateriskAttributes.Static)
        ? throw new NotSupportedException()
        : ParentType.ParentModule.LlvmBuilder.BuildStore(value, LlvmField);

    public LLVMValueRef LoadInstance(LLVMValueRef instance, uint index)
    {
        return ParentType.ParentModule.LlvmBuilder.BuildLoad2(Type,
            ParentType.ParentModule.LlvmBuilder.BuildStructGEP2(Type, instance, index));
    }

    public LLVMValueRef StoreInstance(LLVMValueRef instance, uint index, LLVMValueRef value)
    {
        return ParentType.ParentModule.LlvmBuilder.BuildStore(value,
            ParentType.ParentModule.LlvmBuilder.BuildStructGEP2(Type, instance, index));
    }
}