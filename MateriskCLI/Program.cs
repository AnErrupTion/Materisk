using System.Diagnostics;
using Materisk.Emit;
using Materisk.Lex;
using Materisk.Parse;
using Materisk.Parse.Nodes;
using Materisk.Utils;

namespace MateriskCLI;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("MateriskCLI <file> [-l] [-p] [-n] [-t <triple>] [-c <cpu>] [-f <features>]");
            return;
        }

        var settings = new Settings(ref args);
        var path = settings.InputFile;
        var directory = Path.GetDirectoryName(path);
        var name = Path.GetFileNameWithoutExtension(path);
        var watch = new Stopwatch();
        var diagnostics = new List<Diagnostic>();

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.SetCurrentDirectory(directory);
            path = Path.GetFileName(path);
        }

        var lexer = new Lexer(path, diagnostics);

        watch.Start();
        var lexedTokens = lexer.Lex();
        watch.Stop();

        Console.WriteLine($"Lexed tokens in {watch.Elapsed.Milliseconds} ms ({watch.Elapsed.Seconds} s).");

        if (diagnostics.Count > 0)
        {
            foreach (var diagnostic in diagnostics)
                Console.WriteLine(diagnostic.Text);

            Console.WriteLine("Found errors while lexing! Aborting.");
            Environment.Exit(1);
        }

        if (settings.ShowLexOutput)
            foreach (var tok in lexedTokens)
                Console.WriteLine(tok.ToString());

        var parser = new Parser(path, lexedTokens, diagnostics);

        watch.Restart();
        var ast = parser.Parse();
        watch.Stop();

        Console.WriteLine($"Parsed nodes in {watch.Elapsed.Milliseconds} ms ({watch.Elapsed.Seconds} s).");

        if (diagnostics.Count > 0)
        {
            foreach (var diagnostic in diagnostics)
                Console.WriteLine(diagnostic.Text);

            Console.WriteLine("Found errors while parsing! Aborting.");
            Environment.Exit(1);
        }

        if (settings.ShowParseOutput)
            PrintTree(ast);

        var emitter = new Emitter(name, ast);

        watch.Restart();
        emitter.Emit(settings.NoStdLib, settings.TargetTriple, settings.Cpu, settings.Features);
        watch.Stop();

        Console.WriteLine($"Emitted code in {watch.Elapsed.Milliseconds} ms ({watch.Elapsed.Seconds} s).");
    }

    private static void PrintTree(SyntaxNode node, int ident = 0)
    {
        if (ident > 0)
            Console.Write(new string(' ', ident));
        Console.WriteLine(node);

        foreach (var n in node.GetChildren())
            PrintTree(n, ident + 2);
    }
}