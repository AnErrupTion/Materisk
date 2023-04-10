using LLVMSharp.Interop;
using Materisk.TypeSystem;
using Materisk.Utils;

namespace Materisk.Parse.Nodes.Misc;

internal class CastNode : SyntaxNode
{
    private readonly string _type;
    private readonly string _secondType;
    private readonly SyntaxNode _node;

    public CastNode(string type, string secondType, SyntaxNode node)
    {
        _type = type;
        _secondType = secondType;
        _node = node;
    }

    public override NodeType Type => NodeType.Cast;

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, LLVMBasicBlockRef thenBlock, LLVMBasicBlockRef elseBlock)
    {
        var value = _node.Emit(module, type, method, thenBlock, elseBlock);
        var llvmValue = value.Load();
        LLVMValueRef resultValue;
        switch (_type)
        {
            case "i8" or "u8": resultValue = module.LlvmBuilder.BuildIntCast(llvmValue, LLVMTypeRef.Int8); break;
            case "i16" or "u16": resultValue = module.LlvmBuilder.BuildIntCast(llvmValue, LLVMTypeRef.Int16); break;
            case "i32" or "u32": resultValue = module.LlvmBuilder.BuildIntCast(llvmValue, LLVMTypeRef.Int32); break;
            case "i64" or "u64": resultValue = module.LlvmBuilder.BuildIntCast(llvmValue, LLVMTypeRef.Int64); break;
            case "f32":
                resultValue = llvmValue.TypeOf == LLVMTypeRef.Float || llvmValue.TypeOf == LLVMTypeRef.Double
                    ? module.LlvmBuilder.BuildFPCast(llvmValue, LLVMTypeRef.Float)
                    : module.LlvmBuilder.BuildIntCast(llvmValue, LLVMTypeRef.Float);
                break;
            case "f64":
                resultValue = llvmValue.TypeOf == LLVMTypeRef.Float || llvmValue.TypeOf == LLVMTypeRef.Double
                    ? module.LlvmBuilder.BuildFPCast(llvmValue, LLVMTypeRef.Double)
                    : module.LlvmBuilder.BuildIntCast(llvmValue, LLVMTypeRef.Double);
                break;
            case "ptr" when !string.IsNullOrEmpty(_secondType):
                switch (_secondType)
                {
                    case "i8" or "u8": resultValue = value.PointerElementType != null
                        ? module.LlvmBuilder.BuildPointerCast(llvmValue, LlvmUtils.BytePointer)
                        : module.LlvmBuilder.BuildIntToPtr(llvmValue, LlvmUtils.BytePointer); break;
                    case "i16" or "u16": resultValue = value.PointerElementType != null
                        ? module.LlvmBuilder.BuildPointerCast(llvmValue, LlvmUtils.ShortPointer)
                        : module.LlvmBuilder.BuildIntToPtr(llvmValue, LlvmUtils.ShortPointer); break;
                    case "i32" or "u32": resultValue = value.PointerElementType != null
                        ? module.LlvmBuilder.BuildPointerCast(llvmValue, LlvmUtils.IntPointer)
                        : module.LlvmBuilder.BuildIntToPtr(llvmValue, LlvmUtils.IntPointer); break;
                    case "i64" or "u64": resultValue = value.PointerElementType != null
                        ? module.LlvmBuilder.BuildPointerCast(llvmValue, LlvmUtils.LongPointer)
                        : module.LlvmBuilder.BuildIntToPtr(llvmValue, LlvmUtils.LongPointer); break;
                    case "f32": resultValue = value.PointerElementType != null
                        ? module.LlvmBuilder.BuildPointerCast(llvmValue, LlvmUtils.FloatPointer)
                        : module.LlvmBuilder.BuildIntToPtr(llvmValue, LlvmUtils.FloatPointer); break;
                    case "f64": resultValue = value.PointerElementType != null
                        ? module.LlvmBuilder.BuildPointerCast(llvmValue, LlvmUtils.DoublePointer)
                        : module.LlvmBuilder.BuildIntToPtr(llvmValue, LlvmUtils.DoublePointer); break;
                    case "void": resultValue = value.PointerElementType != null
                        ? module.LlvmBuilder.BuildPointerCast(llvmValue, LlvmUtils.VoidPointer)
                        : module.LlvmBuilder.BuildIntToPtr(llvmValue, LlvmUtils.VoidPointer); break;
                    default:
                    {
                        foreach (var mType in module.Types)
                        {
                            if (mType.Name == _secondType)
                                return (value.PointerElementType != null
                                    ? module.LlvmBuilder.BuildPointerCast(llvmValue, LLVMTypeRef.CreatePointer(mType.Type, 0))
                                    : module.LlvmBuilder.BuildIntToPtr(llvmValue, LLVMTypeRef.CreatePointer(mType.Type, 0)))
                                    .ToMateriskValue();
                        }

                        throw new InvalidOperationException($"Can not cast to pointer type \"{_secondType}\" in method: {module.Name}.{type.Name}.{method.Name}");
                    }
                }
                break;
            default: throw new InvalidOperationException($"Can not cast to type \"{_type}\" in method: {module.Name}.{type.Name}.{method.Name}");
        }

        return resultValue.ToMateriskValue();
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return _node;
    }
}