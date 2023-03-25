using System.Text;
using Materisk.Emit;
using Materisk.Lex;
using Materisk.Parse;
using Materisk.Parse.Nodes;

namespace MateriskCLI;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("MateriskCLI <file> [--show-lex-output] [--show-parse-output]");
            return;
        }

        var showLexOutput = args.Length > 1 && args.Contains("--show-lex-output");
        var showParseOutput = args.Length > 2 && args.Contains("--show-parse-output");

        var path = args[0];
        var name = Path.GetFileNameWithoutExtension(path);

        var lexer = new Lexer(File.ReadAllText(path));
        var lexedTokens = lexer.Lex();

        if (showLexOutput)
            foreach (var tok in lexedTokens)
                Console.WriteLine("  " + tok);

        var parser = new Parser(lexedTokens);
        var ast = parser.Parse();

        if (showParseOutput)
            PrintTree(ast);

        var emitter = new Emitter(name, $"{name}.dll", ast);
        emitter.Emit(EmitType.Cil);
    }

    private static void PrintTree(SyntaxNode node, int ident = 0)
    {
        Console.WriteLine(Ident(ident) + node);

        foreach(var n in node.GetChildren())
            PrintTree(n, ident + 2);
    }

    private static string Ident(int ident)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < ident; i++)
            sb.Append(' ');
        return sb.ToString();
    }
}