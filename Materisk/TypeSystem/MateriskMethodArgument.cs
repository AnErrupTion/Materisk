using LLVMSharp.Interop;

namespace Materisk.TypeSystem;

public sealed class MateriskMethodArgument : MateriskUnit
{
    public MateriskMethod ParentMethod;
    public readonly string Name;
    public LLVMValueRef Value;

    public MateriskMethodArgument(string name, string typeName, LLVMTypeRef type, LLVMTypeRef pointerElementType, bool signed)
    {
        Name = name;
        TypeName = typeName;
        Type = type;
        PointerElementType = pointerElementType;
        Signed = signed;
    }

    public override LLVMValueRef Load() => Value;

    public override LLVMValueRef Store(LLVMValueRef value) => throw new NotSupportedException();
}