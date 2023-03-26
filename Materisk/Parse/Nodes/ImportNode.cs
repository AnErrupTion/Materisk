using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using Materisk.Lex;

namespace Materisk.Parse.Nodes;

public class ImportNode : SyntaxNode
{
    private readonly SyntaxToken _path;

    public ImportNode(SyntaxToken path)
    {
        _path = path;
    }

    public override NodeType Type => NodeType.Import;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        if (!File.Exists(_path.Text))
            throw new Exception($"Failed to import \"{_path.Text}\": File not found");

        var lexer = new Lexer(File.ReadAllText(_path.Text));
        var lexedTokens = lexer.Lex();

        var parser = new Parser(lexedTokens);
        var ast = parser.Parse();

        return ast.Emit(variables, module, type, method, arguments);
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(_path);
    }
}