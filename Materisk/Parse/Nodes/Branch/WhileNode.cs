using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using LLVMSharp.Interop;
using MateriskLLVM;

namespace Materisk.Parse.Nodes.Branch;

internal class WhileNode : SyntaxNode
{
    private readonly SyntaxNode _condNode;
    private readonly SyntaxNode _block;

    public WhileNode(SyntaxNode condNode, SyntaxNode block)
    {
        _condNode = condNode;
        _block = block;
    }

    public override NodeType Type => NodeType.While;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        var index = method.CilMethodBody!.Instructions.Count;
        var condLabel = new CilInstructionLabel();
        var blockStartLabel = new CilInstructionLabel();

        method.CilMethodBody!.Instructions.Add(CilOpCodes.Br, condLabel);

        _block.Emit(variables, module, type, method, arguments);
        blockStartLabel.Instruction = method.CilMethodBody!.Instructions[index + 1];

        index = method.CilMethodBody!.Instructions.Count;
        _condNode.Emit(variables, module, type, method, arguments);
        condLabel.Instruction = method.CilMethodBody!.Instructions[index];

        method.CilMethodBody!.Instructions.Add(CilOpCodes.Brtrue, blockStartLabel);
        return null!;
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var ifBlock = method.LlvmMethod.AppendBasicBlock("if");
        var thenBlock = method.LlvmMethod.AppendBasicBlock("then");
        var elseBlock = method.LlvmMethod.AppendBasicBlock("else");

        module.LlvmBuilder.BuildBr(ifBlock);

        module.LlvmBuilder.PositionAtEnd(ifBlock);
        var ifValue = (LLVMValueRef)_condNode.Emit(module, type, method, metadata);
        var conditionValue = module.LlvmBuilder.BuildCondBr(ifValue, thenBlock, elseBlock);

        module.LlvmBuilder.PositionAtEnd(thenBlock);
        metadata.AddMetadata(Tuple.Create(method, thenBlock, elseBlock));
        var lastValue = (LLVMValueRef)_block.Emit(module, type, method, metadata);

        // To handle "break" and "continue"
        if (lastValue != null && lastValue.InstructionOpcode is not LLVMOpcode.LLVMBr)
            module.LlvmBuilder.BuildBr(ifBlock);

        module.LlvmBuilder.PositionAtEnd(elseBlock);

        return conditionValue;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return _condNode;
        yield return _block;
    }
}