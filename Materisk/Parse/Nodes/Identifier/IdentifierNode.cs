using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
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

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        return null!;
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var name = Token.Text;

        // TODO: Instantiation support
        /*if (name is "self" && method.DeclaringType is not null && method.Parameters.ThisParameter is not null)
        {
            method.CilMethodBody!.Instructions.Add(CilOpCodes.Ldarg, method.Parameters.ThisParameter);
            return method.DeclaringType;
        }*/

        foreach (var typeDef in module.Types)
            if (typeDef.Name == name)
                return typeDef;

        foreach (var field in module.Types[0].Fields)
            if (field.Name == name)
                return module.LlvmBuilder.BuildLoad2(field.Type, field.LlvmField);

        foreach (var meth in module.Types[0].Methods)
            if (meth.Name == name)
                return meth;

        for (var i = 0; i < method.Arguments.Length; i++)
            if (method.Arguments[i].Name == name)
                return method.LlvmMethod.Params[i];

        foreach (var variable in method.Variables)
            if (variable.Name == name)
                return variable;

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