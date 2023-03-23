﻿using System.Text;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Materisk;
using Materisk.BuiltinTypes;
using Materisk.Parsing;
using Materisk.Parsing.Nodes;

namespace MateriskCLI;

public static class Program
{
    private static bool showLexOutput, showParseOutput, timings, rethrow;
    private static Interpreter interpreter;

    public static void Main(string[] args)
    {
        InitInterpreter();

        if (args.Length == 1)
        {
            var lexer = new Lexer(File.ReadAllText(args[0]));
            var lexedTokens = lexer.Lex();

            var parser = new Parser(lexedTokens);
            var ast = parser.Parse();

            var module = new ModuleDefinition("MateriskModule", KnownCorLibs.SystemRuntime_v7_0_0_0);
            var type = new TypeDefinition("MateriskType", "Program", TypeAttributes.Public | TypeAttributes.Class);
            module.TopLevelTypes.Add(type);

            ast.Emit(module, null);

            module.Write("output.dll");
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
                        InitInterpreter();
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

    private static void InitInterpreter() {
        interpreter = new Interpreter();

        var tdict = new SDictionary();
        tdict.Value.Add((new SString("ok"), new SString("works string key")));
        tdict.Value.Add((new SInt(0), new SString("works int key")));

        var classInstTest = new SClass("color");
        classInstTest.InstanceBaseTable.Add((new SString("$$ctor"),
                new SNativeFunction("$$ctor",
                    impl: (Scope scope, List<SValue> args) => {
                        // TODO: Add dot stack assignment; not possible yet

                        // TODO: Remove this code and replace it by safe methods
                        (args[0] as SClassInstance).InstanceTable.Add((new SString("r"), args[1] as SInt));
                        (args[0] as SClassInstance).InstanceTable.Add((new SString("g"), args[2] as SInt));
                        (args[0] as SClassInstance).InstanceTable.Add((new SString("b"), args[3] as SInt));

                        return args[0];
                    },
                    expectedArgs: new() { "self", "r", "g", "b" }
                )
                {
                    IsPublic = true
                }
            ));

        classInstTest.InstanceBaseTable.Add((new SString("mul"),
                new SNativeFunction("mul",
                    impl: (Scope scope, List<SValue> args) => {
                        if (args[1] is not SClassInstance inst || inst.Class.Name != "color") throw new Exception("Expected argument 0 to be of type 'color'");

                        var current = args[0] as SClassInstance;

                        SClassInstance newInst = new(inst.Class);
                        newInst.CallConstructor(scope, new() { newInst, current.Dot(new SString("r")).Mul(inst.Dot(new SString("r"))), current.Dot(new SString("g")).Mul(inst.Dot(new SString("g"))), current.Dot(new SString("b")).Mul(inst.Dot(new SString("b"))) });

                        return newInst;
                    },
                    expectedArgs: new() { "self", "other" },
                    isClassInstanceFunc: true
                )
                {
                    IsPublic = true
                }
            ));

        classInstTest.InstanceBaseTable.Add((new SString("$$toString"),
                new SNativeFunction("$$toString",
                    impl: (Scope scope, List<SValue> args) => {
                        var current = args[0] as SClassInstance;
                        return new SString("<Color R=" + args[0].Dot(new SString("r")).SpagToCsString() + " G=" + args[0].Dot(new SString("g")).SpagToCsString() + " B=" + args[0].Dot(new SString("b")).SpagToCsString() + ">");
                    },
                    expectedArgs: new() { "self" },
                    isClassInstanceFunc: true
                )
                {
                    IsPublic = true
                }
            ));

        interpreter.GlobalScope.Set("test", tdict);
        interpreter.GlobalScope.Set("color", classInstTest);
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