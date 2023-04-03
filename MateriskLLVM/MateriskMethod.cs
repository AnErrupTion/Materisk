using LLVMSharp.Interop;

namespace MateriskLLVM;

public sealed class MateriskMethod : MateriskUnit
{
    public LLVMValueRef LlvmMethod;

    public readonly MateriskType ParentType;
    public readonly string Name;
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

        for (var i = 0; i < Arguments.Length; i++)
        {
            Arguments[i].ParentMethod = this;
            Arguments[i].Value = LlvmMethod.Params[i];
        }
    }

    public MateriskLocalVariable? GetVariableByName(string name)
    {
        foreach (var variable in Variables)
            if (variable.Name == name)
                return variable;

        return null;
    }

    public override LLVMValueRef Load() => throw new NotImplementedException();

    public override LLVMValueRef Store(LLVMValueRef value) => throw new NotImplementedException();
}