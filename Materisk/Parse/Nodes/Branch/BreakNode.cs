﻿using LLVMSharp.Interop;
using Materisk.TypeSystem;

namespace Materisk.Parse.Nodes.Branch;

internal class BreakNode : SyntaxNode
{
    public override NodeType Type => NodeType.Break;

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        foreach (var obj in metadata.Metadata)
            if (obj is Tuple<MateriskMethod, LLVMBasicBlockRef, LLVMBasicBlockRef> t && t.Item1.Name == method.Name)
                return module.LlvmBuilder.BuildBr(t.Item3);

        throw new InvalidOperationException($"Unable to find \"else\" block for method: {module.Name}.{type.Name}.{method.Name}");
    }
}