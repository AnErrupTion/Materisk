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
    private static bool showLexOutput, showParseOutput, timings, rethrow;
    private static Interpreter interpreter;

    public static void Main(string[] args)
    {
        interpreter = new Interpreter();

        if (args.Length == 1)
        {
            var path = args[0];
            var name = Path.GetFileNameWithoutExtension(path);

            var lexer = new Lexer(File.ReadAllText(path));
            var lexedTokens = lexer.Lex();

            var parser = new Parser(lexedTokens);
            var ast = parser.Parse();

            var module = new ModuleDefinition(name, KnownCorLibs.SystemRuntime_v7_0_0_0);
            var mainType = new TypeDefinition(name, "Program", TypeAttributes.Public | TypeAttributes.Class);
            module.TopLevelTypes.Add(mainType);

            var variables = new Dictionary<string, CilLocalVariable>();

            ast.Emit(variables, module, null, null);

            module.Write($"{name}.dll");
        }
        else
        {
            while (true)
            {
                Console.Write("> ");
                var text = Console.ReadLine();

                if (text.Trim() == string.Empty) continue;

                if (text.StartsWith("#")) {
                    if (text.StartsWith("#lex")) {
                        showLexOutput = !showLexOutput;
                        Console.WriteLine("Showing Lex Output: " + showLexOutput);
                    }

                    if (text.StartsWith("#parse")) {
                        showParseOutput = !showParseOutput;
                        Console.WriteLine("Showing Parse Output: " + showParseOutput);
                    }

                    if(text.StartsWith("#time")) {
                        timings = !timings;
                        Console.WriteLine("Timings: " + timings);
                    }

                    if (text.StartsWith("#rethrow")) {
                        rethrow = !rethrow;
                        Console.WriteLine("Rethrow: " + rethrow);
                    }

                    if (text.StartsWith("#reset")) {
                        interpreter = new Interpreter();
                        Console.WriteLine("Reset interpreter");
                    }

                    if (text.StartsWith("#quit")) {
                        Environment.Exit(0);
                    }

                    continue;
                }

                RunCode(interpreter, text);
            }
        }
    }

    private static void RunCode(Interpreter interpreter, string text) {
        TimingInterpreterResult res = new();

        //try {

        interpreter.Interpret(text, ref res);

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("\n  C#: " + res.Result.LastValue);
        Console.WriteLine("  Spag: " + res.Result.LastValue.ToSpagString().Value);


        if (timings) {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Timings:");

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("  Lex: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(res.LexTime + "ms");

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("  Parse: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(res.ParseTime + "ms");

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("  Eval: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(res.EvalTime + "ms");
        }

        Console.ResetColor();
        //} catch (Exception ex) {
        //    Console.WriteLine("Error: " + ex.Message);
        //    if (rethrow) throw;
        //}

        if(showParseOutput && res.Result.AST != null) {
            PrintTree(res.Result.AST);
        }

        if(showLexOutput && res.Result.LexedTokens != null) {
            foreach (var tok in res.Result.LexedTokens) Console.WriteLine("  " + tok);
        }
    }

    private static void PrintTree(SyntaxNode node, int ident = 0) {
        Console.WriteLine(Ident(ident) + node);
            
        foreach(var n in node.GetChildren()) {
            PrintTree(n, ident + 2);
        }
    }

    private static string Ident(int ident) {
        StringBuilder b = new();
        for (var i = 0; i < ident; i++) b.Append(' ');
        return b.ToString();
    }
}