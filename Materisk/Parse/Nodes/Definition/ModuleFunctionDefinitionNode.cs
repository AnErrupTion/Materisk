using LLVMSharp.Interop;
using MateriskLLVM;
using Materisk.Native;
using Materisk.Utils;

namespace Materisk.Parse.Nodes.Definition;

internal class ModuleFunctionDefinitionNode : SyntaxNode
{
    public readonly string Name;
    public readonly List<MethodArgument> Args;
    public readonly string ReturnType;
    public readonly string SecondReturnType;
    public readonly SyntaxNode Body;
    public readonly bool IsStatic;
    public readonly bool IsPublic;
    public readonly bool IsNative;

    public ModuleFunctionDefinitionNode(string name, List<MethodArgument> args, string returnType, string secondReturnType, SyntaxNode body, bool isStatic, bool isPublic, bool isNative)
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
    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var argts = new List<MateriskMethodArgument>();
        var parameters = new List<LLVMTypeRef>();

        foreach (var arg in Args)
        {
            var firstType = arg.Type;
            var secondType = arg.SecondType;
            var argType = TypeSigUtils.GetTypeSignatureFor(module, firstType, secondType);
            var pointerElementType = firstType switch
            {
                "ptr" or "arr" when !string.IsNullOrEmpty(secondType) => TypeSigUtils.GetTypeSignatureFor(module, secondType),
                "str" => LLVMTypeRef.Int8,
                _ => null
            };

            parameters.Add(argType);
            argts.Add(new(arg.Name, argType, pointerElementType, firstType[0] is 'i'));
        }

        var newMethod = new MateriskMethod(
            type,
            Name,
            MateriskAttributesUtils.CreateAttributes(IsPublic, IsStatic, IsNative, false),
            LLVMTypeRef.CreateFunction(
                TypeSigUtils.GetTypeSignatureFor(module, ReturnType, SecondReturnType),
                parameters.ToArray()),
            argts.ToArray());

        type.Methods.Add(newMethod);

        if (!IsNative)
            Body.Emit(module, type, newMethod, metadata);
        else
            LlvmNativeFuncImpl.Emit(module, type.Name, newMethod);

        return newMethod;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return Body;
    }
}