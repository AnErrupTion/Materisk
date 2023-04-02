using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using MateriskLLVM;
using Materisk.Lex;

namespace Materisk.Parse.Nodes.Misc;

/// <summary>
/// dummy node for tree view
/// </summary>
public class TokenNode : SyntaxNode
{
    public override NodeType Type => NodeType.Token;
    public SyntaxToken Token { get; }

    public TokenNode(SyntaxToken token)
    {
        Token = token;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        throw new NotSupportedException();
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        throw new NotSupportedException();
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        return Enumerable.Empty<SyntaxNode>();
    }

    public override string ToString()
    {
        return "TokenNode: " + Token.Type + " text=" + Token.Text;
    }
}