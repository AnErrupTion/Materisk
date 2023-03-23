using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using Materisk.BuiltinTypes;

namespace Materisk.Parsing.Nodes;

internal class IdentifierNode : SyntaxNode
{
    public SyntaxToken Token { get; }

    public IdentifierNode(SyntaxToken syntaxToken)
    {
        Token = syntaxToken;
    }

    public override NodeType Type => NodeType.Identifier;

    public override SValue Evaluate(Scope scope)
    {
        return scope.Get((string)Token.Value) ?? SValue.Null;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, MethodDefinition method, List<string> arguments)
    {
        var name = Token.Value.ToString();

        foreach (var type in module.TopLevelTypes)
            if (type.Name == name)
                return type;

        var index = 0;

        foreach (var argument in arguments)
        {
            if (argument == name)
            {
                method.CilMethodBody?.Instructions.Add(CilOpCodes.Ldarg, method.Parameters[index]);
                return argument;
            }

            index++;
        }
        
        foreach (var variable in variables)
            if (variable.Key == name)
            {
                method.CilMethodBody?.Instructions.Add(CilOpCodes.Ldloc, variable.Value);
                return variable.Value;
            }

        throw new InvalidOperationException($"Unable to find value for identifier: {name}");
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(Token);
    }

    public override string ToString()
    {
        return "IdentNode:";
    }
}