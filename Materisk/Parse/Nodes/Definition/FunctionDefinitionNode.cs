using LLVMSharp.Interop;
using MateriskLLVM;
using Materisk.Native;
using Materisk.Utils;

namespace Materisk.Parse.Nodes.Definition;

internal class FunctionDefinitionNode : SyntaxNode
{
    public readonly string Name;
    public readonly List<MethodArgument> Args;
    public readonly string ReturnType;
    public readonly string SecondReturnType;
    public readonly SyntaxNode Block;
    public readonly bool IsPublic;
    public readonly bool IsNative;

    public FunctionDefinitionNode(string name, List<MethodArgument> args, string returnType, string secondReturnType, SyntaxNode block, bool isPublic, bool isNative)
    {
        Name = name;
        Args = args;
        ReturnType = returnType;
        SecondReturnType = secondReturnType;
        Block = block;
        IsPublic = isPublic;
        IsNative = isNative;
    }

    public override NodeType Type => NodeType.FunctionDefinition;

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
            module.Types[0],
            Name,
            MateriskAttributesUtils.CreateAttributes(IsPublic, true, IsNative, false),
            LLVMTypeRef.CreateFunction(
                TypeSigUtils.GetTypeSignatureFor(module, ReturnType, SecondReturnType),
                parameters.ToArray()),
            argts.ToArray());

        module.Types[0].Methods.Add(newMethod);

        var mType = module.Types[0];

        if (!IsNative)
            Block.Emit(module, mType, newMethod, metadata);
        else
            LlvmNativeFuncImpl.Emit(module, mType.Name, newMethod);

        return newMethod;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return Block;
    }
}