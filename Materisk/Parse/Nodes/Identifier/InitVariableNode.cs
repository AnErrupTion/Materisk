using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using LLVMSharp.Interop;
using Materisk.Lex;
using Materisk.Parse.Nodes.Misc;
using Materisk.Utils;

namespace Materisk.Parse.Nodes.Identifier;

internal class InitVariableNode : SyntaxNode
{
    private readonly SyntaxToken _identToken;
    private readonly SyntaxToken _typeToken;
    private readonly SyntaxToken? _secondTypeToken;
    private readonly SyntaxNode _expr;

    public InitVariableNode(SyntaxToken identToken, SyntaxToken typeToken, SyntaxToken? secondTypeToken, SyntaxNode expr)
    {
        _identToken = identToken;
        _typeToken = typeToken;
        _secondTypeToken = secondTypeToken;
        _expr = expr;
    }

    public override NodeType Type => NodeType.InitVariable;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        var name = _identToken.Text;

        if (name is null)
            throw new InvalidOperationException("Can not assign to a non-existent identifier!");

        if (variables.ContainsKey(name))
            throw new InvalidOperationException("Can not initialize the same variable twice!");

        var value = _expr.Emit(variables, module, type, method, arguments);
        var variable = new CilLocalVariable(TypeSigUtils.GetTypeSignatureFor(module, _typeToken.Text, _secondTypeToken?.Text));
        method.CilMethodBody!.LocalVariables.Add(variable);
        method.CilMethodBody!.Instructions.Add(CilOpCodes.Stloc, variable);
        variables.Add(name, variable);
        return value;
    }

    public override object Emit(List<string> variables, LLVMModuleRef module, LLVMValueRef method, List<string> arguments)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(_identToken);
        yield return new TokenNode(_typeToken);
        if (_secondTypeToken is not null) yield return new TokenNode(_secondTypeToken);
        yield return _expr;
    }

    public override string ToString()
    {
        return "InitVariableNode:";
    }
}