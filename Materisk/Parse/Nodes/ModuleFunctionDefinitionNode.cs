using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Materisk.Lex;
using Materisk.Native;
using Materisk.Utils;

namespace Materisk.Parse.Nodes;

internal class ModuleFunctionDefinitionNode : SyntaxNode
{
    private readonly SyntaxToken _moduleName;
    private readonly SyntaxToken _name;
    private readonly Dictionary<SyntaxToken, SyntaxToken> _args;
    private readonly SyntaxToken _returnType;
    private readonly SyntaxNode _body;
    private readonly bool _isStatic;
    private readonly bool _isPublic;
    private readonly bool _isNative;

    public ModuleFunctionDefinitionNode(SyntaxToken moduleName, SyntaxToken name, Dictionary<SyntaxToken, SyntaxToken> args, SyntaxToken returnType, SyntaxNode body, bool isStatic, bool isPublic, bool isNative)
    {
        _moduleName = moduleName;
        _name = name;
        _args = args;
        _returnType = returnType;
        _body = body;
        _isStatic = isStatic;
        _isPublic = isPublic;
        _isNative = isNative;
    }

    public override NodeType Type => NodeType.ModuleFunctionDefinition;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        var targetName = _name.Text;

        var argts = new List<string>();
        var parameters = new List<TypeSignature>();

        foreach (var arg in _args)
        {
            parameters.Add(TypeSigUtils.GetTypeSignatureFor(module, arg.Key.Text));
            argts.Add(arg.Value.Text);
        }

        MethodDefinition newMethod;

        if (targetName is "ctor")
        {
            if (_returnType.Text is not "void")
                throw new InvalidOperationException("Return type for constructor must be void!");

            newMethod = new MethodDefinition(".ctor",
                MethodAttributes.Public,
                MethodSignature.CreateInstance(module.CorLibTypeFactory.Void, parameters)) 
            {
                IsSpecialName = true,
                IsRuntimeSpecialName = true
            };
        }
        else
        {
            MethodAttributes attributes = 0;

            if (_isPublic)
                attributes |= MethodAttributes.Public;

            if (_isStatic)
                attributes |= MethodAttributes.Static;

            newMethod = new MethodDefinition(targetName,
                attributes,
                _isStatic
                    ? MethodSignature.CreateStatic(TypeSigUtils.GetTypeSignatureFor(module, _returnType.Text), parameters)
                    : MethodSignature.CreateInstance(TypeSigUtils.GetTypeSignatureFor(module, _returnType.Text), parameters));
        }

        type.Methods.Add(newMethod);

        newMethod.CilMethodBody = new(newMethod);

        if (_isNative)
            CilNativeFuncImpl.Emit(module, _moduleName.Text, newMethod);
        else
            _body.Emit(variables, module, type, newMethod, argts);

        newMethod.CilMethodBody?.Instructions.Add(CilOpCodes.Ret);

        return newMethod;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(_name);
        foreach (var tok in _args) yield return new TokenNode(tok.Value);
        yield return _body;
    }
}