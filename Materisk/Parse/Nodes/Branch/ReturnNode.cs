using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using LLVMSharp.Interop;
using MateriskLLVM;

namespace Materisk.Parse.Nodes.Branch;

internal class ReturnNode : SyntaxNode
{
    private readonly SyntaxNode? _returnValueNode;
    
    public ReturnNode(SyntaxNode? returnValueNode = null)
    {
        _returnValueNode = returnValueNode;
    }

    public override NodeType Type => NodeType.Return;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        return null!;
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var value = _returnValueNode?.Emit(module, type, method, metadata);

        metadata.AddMetadata(MateriskMetadataType.Return);

        return value is not null
            ? module.LlvmBuilder.BuildRet(value is MateriskUnit unit ? unit.Load() : (LLVMValueRef)value)
            : module.LlvmBuilder.BuildRetVoid();
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        if (_returnValueNode != null) yield return _returnValueNode;
    }
}