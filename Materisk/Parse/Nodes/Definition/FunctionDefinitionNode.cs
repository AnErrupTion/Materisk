using LLVMSharp.Interop;
using MateriskLLVM;
using Materisk.Native;
using Materisk.Utils;

namespace Materisk.Parse.Nodes.Definition;

internal class FunctionDefinitionNode : SyntaxNode
{
    private readonly string _name;
    private readonly List<MethodArgument> _args;
    private readonly string _returnType;
    private readonly string _secondReturnType;
    private readonly SyntaxNode _block;
    private readonly bool _isPublic;
    private readonly bool _isNative;

    public FunctionDefinitionNode(string name, List<MethodArgument> args, string returnType, string secondReturnType, SyntaxNode block, bool isPublic, bool isNative)
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

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var argts = new List<MateriskMethodArgument>();
        var parameters = new List<LLVMTypeRef>();

        foreach (var arg in _args)
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
            _name,
            MateriskAttributesUtils.CreateAttributes(_isPublic, true, _isNative),
            LLVMTypeRef.CreateFunction(
                TypeSigUtils.GetTypeSignatureFor(module, _returnType, _secondReturnType),
                parameters.ToArray()),
            argts.ToArray());

        module.Types[0].Methods.Add(newMethod);

        var mType = module.Types[0];

        if (!_isNative)
            _block.Emit(module, mType, newMethod, metadata);
        else
            LlvmNativeFuncImpl.Emit(module, mType.Name, newMethod);

        return newMethod;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return _block;
    }
}