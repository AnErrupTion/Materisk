using Materisk.Parse.Nodes;
using Materisk.TypeSystem;
using Materisk.Utils;

namespace Materisk.Emit;

public class Emitter
{
    private readonly string _moduleName;
    private readonly SyntaxNode _rootNode;

    public Emitter(string moduleName, SyntaxNode rootNode)
    {
        _moduleName = moduleName;
        _rootNode = rootNode;
    }

    public void Emit(bool noStdLib, string targetTriple, string cpu, string features)
    {
        if (noStdLib)
            LlvmUtils.MainFunctionNameOverride = "_start";

        // Initialize LLVM
        LlvmUtils.Initialize(targetTriple, cpu, features, noStdLib);

        // Create module and emit LLVM IR + native code
        MateriskHelpers.CreateModuleEmit(_moduleName, _rootNode);
    }
}