﻿using System.Globalization;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using LLVMSharp.Interop;
using MateriskLLVM;
using Materisk.Lex;
using Materisk.Parse.Nodes.Misc;

namespace Materisk.Parse.Nodes.Literal;

internal class IntLiteralNode : SyntaxNode
{
    private readonly SyntaxToken _syntaxToken;

    public IntLiteralNode(SyntaxToken syntaxToken)
    {
        _syntaxToken = syntaxToken;
    }

    public override NodeType Type => NodeType.IntLiteral;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        var value = int.Parse(_syntaxToken.Text, CultureInfo.InvariantCulture);
        method.CilMethodBody!.Instructions.Add(CilInstruction.CreateLdcI4(value));
        return value;
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method)
    {
        var value = int.Parse(_syntaxToken.Text, CultureInfo.InvariantCulture);
        if (value < 0)
        {
            var llvmValue = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, Convert.ToUInt64(Math.Abs(value)), true);
            var sub = LLVMValueRef.CreateConstSub(LlvmUtils.IntZero, llvmValue);
            return sub;
        }
        else
        {
            var llvmValue = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, Convert.ToUInt64(value), true);
            return llvmValue;
        }
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(_syntaxToken);
    }

    public override string ToString()
    {
        return "IntLitNode:";
    }
}