using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using LLVMSharp.Interop;
using MateriskLLVM;

namespace Materisk.Parse.Nodes.Misc;

internal class IndexNode : SyntaxNode
{
    private readonly SyntaxNode _nameNode;
    private readonly SyntaxNode _indexNode;
    private readonly SyntaxNode? _setNode;

    public IndexNode(SyntaxNode nameNode, SyntaxNode indexNode, SyntaxNode? setNode)
    {
        _nameNode = nameNode;
        _indexNode = indexNode;
        _setNode = setNode;
    }

    public override NodeType Type => NodeType.Index;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        if (_nameNode.Emit(variables, module, type, method, arguments) is not CilLocalVariable variable)
            throw new InvalidOperationException("Catastrophic failure: variable is null."); // This should never happen

        _indexNode.Emit(variables, module, type, method, arguments);

        // This should get the type of each element in the array or the pointer.
        // For example: for "int[]" or "int*" we'd get "int"
        var underlyingType = variable.VariableType.GetUnderlyingTypeDefOrRef();

        if (underlyingType is null)
            throw new InvalidOperationException("Catastrophic failure: variable is not array or pointer."); // This should never happen either

        switch (variable.VariableType)
        {
            case PointerTypeSignature when _setNode != null:
            {
                method.CilMethodBody!.Instructions.Add(CilOpCodes.Add);
                _setNode.Emit(variables, module, type, method, arguments);
                if (underlyingType == module.CorLibTypeFactory.Byte.ToTypeDefOrRef())
                    method.CilMethodBody!.Instructions.Add(CilOpCodes.Stind_I1);
                break;
            }
            case PointerTypeSignature:
            {
                method.CilMethodBody!.Instructions.Add(CilOpCodes.Add);
                if (underlyingType == module.CorLibTypeFactory.Byte.ToTypeDefOrRef())
                    method.CilMethodBody!.Instructions.Add(CilOpCodes.Ldind_I1);
                break;
            }
            case SzArrayTypeSignature when _setNode != null:
            {
                _setNode.Emit(variables, module, type, method, arguments);
                method.CilMethodBody!.Instructions.Add(CilOpCodes.Stelem, underlyingType);
                break;
            }
            case SzArrayTypeSignature:
            {
                method.CilMethodBody!.Instructions.Add(CilOpCodes.Ldelem, underlyingType);
                break;
            }
        }

        return null!;
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var name = _nameNode.Emit(module, type, method, metadata);

        if (name is not MateriskUnit unit)
            throw new NotImplementedException($"Value isn't MateriskUnit: {name}");

        var llvmType = unit.Type;
        var llvmElementType = unit.PointerElementType;
        var llvmValue = unit.Load();
        var underlyingType = llvmValue.TypeOf.Kind;

        if (underlyingType is not LLVMTypeKind.LLVMPointerTypeKind)
            throw new InvalidOperationException($"Catastrophic failure: variable is not pointer: {underlyingType}"); // This should never happen either

        var indexValue = _indexNode.Emit(module, type, method, metadata);
        var index = indexValue is MateriskUnit indexUnit ? indexUnit.Load() : (LLVMValueRef)indexValue;

        switch (underlyingType)
        {
            case LLVMTypeKind.LLVMPointerTypeKind when _setNode is not null:
            {
                var llvmPtr = module.LlvmBuilder.BuildGEP2(llvmType, llvmValue, new[] { index });
                var value = _setNode.Emit(module, type, method, metadata);
                return module.LlvmBuilder.BuildStore(value is MateriskUnit valueUnit ? valueUnit.Load() : (LLVMValueRef)value, llvmPtr);
            }
            case LLVMTypeKind.LLVMPointerTypeKind:
            {
                var llvmPtr = module.LlvmBuilder.BuildGEP2(llvmType, llvmValue, new[] { index });
                return module.LlvmBuilder.BuildLoad2(llvmElementType, llvmPtr);
            }
        }

        return null!;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return _nameNode;
        yield return _indexNode;
        if (_setNode is not null) yield return _setNode;
    }

    public override string ToString()
    {
        return "IdxNode:";
    }
}