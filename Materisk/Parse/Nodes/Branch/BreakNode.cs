﻿using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using LLVMSharp.Interop;
using MateriskLLVM;

namespace Materisk.Parse.Nodes.Branch;

internal class BreakNode : SyntaxNode
{
    public override NodeType Type => NodeType.Break;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        return null!;
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        foreach (var obj in metadata.Metadata)
            if (obj is Tuple<MateriskMethod, LLVMBasicBlockRef, LLVMBasicBlockRef> t && t.Item1.Name == method.Name)
                return module.LlvmBuilder.BuildBr(t.Item3); // Else block

        throw new InvalidOperationException($"Unable to find else block for method: {method.Name}");
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        return Enumerable.Empty<SyntaxNode>();
    }

    public override string ToString()
    {
        return "BreakNode:";
    }
}