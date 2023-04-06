using LLVMSharp.Interop;
using Materisk.Native;
using Materisk.TypeSystem;

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

    public FunctionDefinitionNode(string name, List<MethodArgument> args, string returnType, string secondReturnType, SyntaxNode body, bool isStatic, bool isPublic, bool isNative)
    {
        Name = name;
        Args = args;
        ReturnType = returnType;
        SecondReturnType = secondReturnType;
        Body = body;
        IsStatic = isStatic;
        IsPublic = isPublic;
        IsNative = isNative;
    }

    public override NodeType Type => NodeType.ModuleFunctionDefinition;

    // TODO: Constructor and instance methods
    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, LLVMBasicBlockRef thenBlock, LLVMBasicBlockRef elseBlock)
    {
        var newMethod = MateriskHelpers.AddMethod(module, type, Name, Args, IsPublic, IsStatic, IsNative, false, ReturnType, SecondReturnType);

        type.Methods.Add(newMethod);

        if (!IsNative)
            Body.Emit(module, type, newMethod, thenBlock, elseBlock);
        else
            LlvmNativeFuncImpl.Emit(module, type.Name, newMethod);

        return newMethod;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return Body;
    }
}