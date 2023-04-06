using LLVMSharp.Interop;
using Materisk.Native;
using Materisk.TypeSystem;
using Materisk.Utils;

namespace Materisk.Parse.Nodes.Definition;

internal class FunctionDefinitionNode : SyntaxNode
{
    public readonly string Name;
    public readonly List<MethodArgument> Args;
    public readonly string ReturnType;
    public readonly string SecondReturnType;
    public readonly SyntaxNode Body;
    public readonly bool IsStatic;
    public readonly bool IsPublic;
    public readonly bool IsNative;
    public readonly bool IsExternal;

    public FunctionDefinitionNode(string name, List<MethodArgument> args, string returnType, string secondReturnType, SyntaxNode body, bool isStatic, bool isPublic, bool isNative, bool isExternal)
    {
        Name = name;
        Args = args;
        ReturnType = returnType;
        SecondReturnType = secondReturnType;
        Body = body;
        IsStatic = isStatic;
        IsPublic = isPublic;
        IsNative = isNative;
        IsExternal = isExternal;
    }

    public override NodeType Type => NodeType.ModuleFunctionDefinition;

    // TODO: Constructor and instance methods
    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, LLVMBasicBlockRef thenBlock, LLVMBasicBlockRef elseBlock)
    {
        var newMethod = MateriskHelpers.AddMethod(module, type, Name, Args, IsPublic, IsStatic, IsNative, IsExternal, ReturnType, SecondReturnType);

        type.Methods.Add(newMethod);

        if (!IsNative)
            Body.Emit(module, type, newMethod, thenBlock, elseBlock);
        else if (!LlvmUtils.NoStdLib)
            LlvmNativeFuncImpl.Emit(module, type.Name, newMethod);

        return newMethod;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return Body;
    }
}