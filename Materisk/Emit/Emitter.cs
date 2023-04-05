using System.Diagnostics;
using System.Text;
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
        LlvmUtils.Initialize(targetTriple, cpu, features);

        // Create module and emit LLVM IR + native code
        MateriskHelpers.CreateModuleEmit(_moduleName, _rootNode);

        // Link all ELF object files into one executable file
        var args = new StringBuilder();
        foreach (var file in Directory.GetFiles("output"))
            args.Append(file).Append(' ');
        args.Append("-o ").Append(_moduleName);
        if (noStdLib)
            args.Append(" -nostdlib");

        Process.Start("clang", args.ToString()).WaitForExit();
    }
}