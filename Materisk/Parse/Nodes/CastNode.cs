using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using Materisk.BuiltinTypes;
using Materisk.Lex;

namespace Materisk.Parse.Nodes;

internal class CastNode : SyntaxNode
{
    private readonly SyntaxToken ident;
    private readonly SyntaxNode node;

    public CastNode(SyntaxToken ident, SyntaxNode node)
    {
        this.ident = ident;
        this.node = node;
    }

    public override NodeType Type => NodeType.Cast;

    public override SValue Evaluate(Scope scope)
    {
        return null;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        node.Emit(variables, module, type, method, arguments);

        switch (ident.Text)
        {
            case "int": method.CilMethodBody?.Instructions.Add(CilOpCodes.Conv_I4); break;
            case "float": method.CilMethodBody?.Instructions.Add(CilOpCodes.Conv_R4); break;
        }

        return null;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(ident);
        yield return node;
    }
}