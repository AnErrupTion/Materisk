using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using LLVMSharp.Interop;
using MateriskLLVM;
using Materisk.Native;
using Materisk.Utils;

namespace Materisk.Parse.Nodes.Definition;

internal class FunctionDefinitionNode : SyntaxNode
{
    private readonly string _name;
    private readonly Dictionary<Tuple<string, string>, string> _args;
    private readonly string _returnType;
    private readonly string _secondReturnType;
    private readonly SyntaxNode _block;
    private readonly bool _isPublic;
    private readonly bool _isNative;

    public FunctionDefinitionNode(string name, Dictionary<Tuple<string, string>, string> args, string returnType, string secondReturnType, SyntaxNode block, bool isPublic, bool isNative)
    {
        _name = name;
        _args = args;
        _returnType = returnType;
        _secondReturnType = secondReturnType;
        _block = block;
        _isPublic = isPublic;
        _isNative = isNative;
    }

    public override NodeType Type => NodeType.FunctionDefinition;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        return null!;
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var argts = new List<MateriskMethodArgument>();
        var parameters = new List<LLVMTypeRef>();

        foreach (var arg in _args)
        {
            var firstType = arg.Key.Item1;
            var secondType = arg.Key.Item2;
            var argType = TypeSigUtils.GetTypeSignatureFor(firstType, secondType);
            var pointerElementType = firstType switch
            {
                "ptr" or "arr" when !string.IsNullOrEmpty(secondType) => TypeSigUtils.GetTypeSignatureFor(secondType),
                "str" => LLVMTypeRef.Int8,
                _ => null
            };

            parameters.Add(argType);
            argts.Add(new(arg.Value, argType, pointerElementType, firstType[0] is 'i'));
        }

        var newMethod = new MateriskMethod(
            module.Types[0],
            _name,
            LLVMTypeRef.CreateFunction(
                TypeSigUtils.GetTypeSignatureFor(_returnType, _secondReturnType),
                parameters.ToArray()),
            argts.ToArray());

        module.Types[0].Methods.Add(newMethod);

        var mType = module.Types[0];

        if (!_isNative)
        {
            _block.Emit(module, mType, newMethod, metadata);
            if (metadata.Last() is not MateriskMetadataType.Return)
                module.LlvmBuilder.BuildRetVoid();
        } else LlvmNativeFuncImpl.Emit(module, mType.Name, newMethod);

        return newMethod;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return _block;
    }
}