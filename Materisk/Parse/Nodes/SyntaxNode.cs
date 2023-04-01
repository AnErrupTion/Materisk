﻿using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using MateriskLLVM;

namespace Materisk.Parse.Nodes;

public abstract class SyntaxNode
{
    public abstract NodeType Type { get; }

    public abstract object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments);

    public abstract object Emit(MateriskModule module, MateriskType type, MateriskMethod method);

    public abstract IEnumerable<SyntaxNode> GetChildren();
}