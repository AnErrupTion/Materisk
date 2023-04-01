using LLVMSharp.Interop;

namespace MateriskLLVM;

public sealed class MateriskMethodArgument
{
    public readonly string Name;
    public readonly LLVMTypeRef Type;

    public MateriskMethodArgument(string name, LLVMTypeRef type)
    {
        Name = name;
        Type = type;
    }
}