﻿using System.Diagnostics;
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
        var directory = Path.GetDirectoryName(path);
        var name = Path.GetFileNameWithoutExtension(path);
        var watch = new Stopwatch();

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.SetCurrentDirectory(directory);
            path = Path.GetFileName(path);
        }

        var lexer = new Lexer(File.ReadAllText(path));

        watch.Start();
        var lexedTokens = lexer.Lex();
        watch.Stop();

        Console.WriteLine($"Lexed tokens in {watch.Elapsed.Milliseconds} ms ({watch.Elapsed.Seconds} s).");

        if (showLexOutput)
            foreach (var tok in lexedTokens)
                Console.WriteLine($"  {tok}");

        var parser = new Parser(lexedTokens);

        watch.Restart();
        var ast = parser.Parse();
        watch.Stop();

        Console.WriteLine($"Parsed nodes in {watch.Elapsed.Milliseconds} ms ({watch.Elapsed.Seconds} s).");

        if (showParseOutput)
            PrintTree(ast);

        var emitter = new Emitter(name, ast);

        watch.Restart();
        emitter.Emit(EmitType.Cil);
        watch.Stop();

        Console.WriteLine($"Emitted code in {watch.Elapsed.Milliseconds} ms ({watch.Elapsed.Seconds} s).");
    }

    private static void PrintTree(SyntaxNode node, int ident = 0)
    {
        Console.WriteLine($"{Ident(ident)}{node}");

        foreach (var n in node.GetChildren())
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