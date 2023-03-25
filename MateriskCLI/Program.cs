using System.Text;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Materisk;
using Materisk.Parsing;
using Materisk.Parsing.Nodes;

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

        var showLexOutput = args.Length > 1 && args[1] == "--show-lex-output";
        var showParseOutput = args.Length > 2 && args[2] == "--show-parse-output";

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

        var module = new ModuleDefinition(name, KnownCorLibs.SystemRuntime_v7_0_0_0);
        var mainType = new TypeDefinition(name, "Program", TypeAttributes.Public | TypeAttributes.Class);
        module.TopLevelTypes.Add(mainType);

        var variables = new Dictionary<string, CilLocalVariable>();

        ast.Emit(variables, module, null, null, null);

        module.Write($"{name}.dll");
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