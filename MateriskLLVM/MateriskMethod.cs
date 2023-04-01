using LLVMSharp.Interop;

namespace MateriskLLVM;

public sealed class MateriskMethod
{
    public LLVMValueRef LlvmMethod;

    public readonly MateriskType ParentType;
    public readonly string Name;
    public readonly LLVMTypeRef Type;
    public readonly MateriskMethodArgument[] Arguments;
    public readonly List<MateriskLocalVariable> Variables;

    public MateriskMethod(MateriskType type, string name, LLVMTypeRef methodType, MateriskMethodArgument[] arguments)
    {
        LlvmMethod = type.ParentModule.LlvmModule.AddFunction(name is "main" ? LlvmUtils.MainFunctionNameOverride : $"{type.Name}_{name}", methodType);

        var entryBlock = LlvmMethod.AppendBasicBlock("entry");
        type.ParentModule.LlvmBuilder.PositionAtEnd(entryBlock);

        ParentType = type;
        Name = name;
        Type = methodType;
        Arguments = arguments;
        Variables = new();
    }

    public MateriskLocalVariable? GetVariableByName(string name)
    {
        foreach (var variable in Variables)
            if (variable.Name == name)
                return variable;

        return null;
    }
}