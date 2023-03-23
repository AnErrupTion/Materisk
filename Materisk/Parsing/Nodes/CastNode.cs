using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using Materisk.BuiltinTypes;

namespace Materisk.Parsing.Nodes;

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
        // TODO: maybe improve this
        switch (ident.Text)
        {
            case "int":
                return node.Evaluate(scope).CastToBuiltin(SBuiltinType.Int);
            case "float":
                return node.Evaluate(scope).CastToBuiltin(SBuiltinType.Float);
            case "string":
                return node.Evaluate(scope).CastToBuiltin(SBuiltinType.String);
            case "list":
                return node.Evaluate(scope).CastToBuiltin(SBuiltinType.List);
            default: throw new InvalidOperationException("INTERNAL: Cast was parsed successfully, but cast is not implemented for that!");
        }
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, MethodDefinition method, List<string> arguments)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(ident);
        yield return node;
    }
}