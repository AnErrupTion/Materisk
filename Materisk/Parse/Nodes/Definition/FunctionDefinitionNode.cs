using Materisk.Native;
using Materisk.TypeSystem;

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
        var mType = module.Types[0];
        var newMethod = MateriskHelpers.AddMethod(module, mType, Name, Args, IsPublic, true, IsNative, false, ReturnType, SecondReturnType);

        module.Types[0].Methods.Add(newMethod);

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