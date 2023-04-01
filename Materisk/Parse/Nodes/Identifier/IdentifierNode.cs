using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using MateriskLLVM;
using Materisk.Lex;
using Materisk.Parse.Nodes.Misc;

namespace Materisk.Parse.Nodes.Identifier;

internal class IdentifierNode : SyntaxNode
{
    public SyntaxToken Token { get; }

    public IdentifierNode(SyntaxToken syntaxToken)
    {
        Token = syntaxToken;
    }

    public override NodeType Type => NodeType.Identifier;

    // TODO: Find a way to not make the names conflict?
    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        var name = Token.Text;

        if (name is "self" && method.DeclaringType is not null && method.Parameters.ThisParameter is not null)
        {
            method.CilMethodBody!.Instructions.Add(CilOpCodes.Ldarg, method.Parameters.ThisParameter);
            return method.DeclaringType;
        }

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
                method.CilMethodBody!.Instructions.Add(CilOpCodes.Ldarg, method.Parameters[index]);
                return argument;
            }

            index++;
        }

        foreach (var variable in variables)
            if (variable.Key == name)
            {
                method.CilMethodBody!.Instructions.Add(CilOpCodes.Ldloc, variable.Value);
                return variable.Value;
            }

        throw new InvalidOperationException($"Unable to find value for identifier: {name}");
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method)
    {
        var name = Token.Text;

        foreach (var typeDef in module.Types)
            if (typeDef.Name == name)
                return typeDef;

        foreach (var field in module.Types[0].Fields)
            if (field.Name == name)
                return field.LlvmField;

        foreach (var meth in module.Types[0].Methods)
            if (meth.Name == name)
                return meth.LlvmMethod;

        var index = 0;

        foreach (var argument in method.Arguments)
        {
            if (argument.Name == name)
                return method.LlvmMethod.Params[index];

            index++;
        }

        foreach (var variable in method.Variables)
            if (variable.Name == name)
                return variable.Mutable ? module.LlvmBuilder.BuildLoad(variable.Value) : variable.Value;

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