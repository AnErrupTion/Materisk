using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using Materisk.BuiltinTypes;
using Materisk.Lex;

namespace Materisk.Parse.Nodes;

internal class CastNode : SyntaxNode
{
    private readonly SyntaxToken _ident;
    private readonly SyntaxNode _node;

    public CastNode(SyntaxToken ident, SyntaxNode node)
    {
        _ident = ident;
        _node = node;
    }

    public override NodeType Type => NodeType.Cast;

    public override SValue Evaluate(Scope scope)
    {
        return null;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        _node.Emit(variables, module, type, method, arguments);

        switch (_ident.Text)
        {
            case "int": method.CilMethodBody.Instructions.Add(CilOpCodes.Conv_I4); break;
            case "float": method.CilMethodBody.Instructions.Add(CilOpCodes.Conv_R4); break;
        }

        return null;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(_ident);
        yield return _node;
    }
}