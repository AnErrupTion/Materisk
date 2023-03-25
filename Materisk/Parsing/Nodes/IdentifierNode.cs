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
        return null;
    }

    // TODO: Find a way to not make the names conflict?
    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        var name = Token.Value.ToString();

        if (name is "self" && method.DeclaringType is not null)
            return method.DeclaringType;

        foreach (var typeDef in module.TopLevelTypes)
            if (typeDef.Name == name)
                return typeDef;

        foreach (var meth in module.TopLevelTypes[1].Methods)
            if (meth.Name == name)
                return meth;

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