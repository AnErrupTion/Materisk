using LLVMSharp.Interop;

namespace MateriskCLI;

public class Settings
{
    public readonly string InputFile, TargetTriple = LLVMTargetRef.DefaultTriple, Cpu = "generic", Features = string.Empty;
    public readonly bool ShowLexOutput, ShowParseOutput, NoStdLib;

    public Settings(ref string[] args)
    {
        var index = 0;
        while (index < args.Length)
        {
            var arg = args[index++];
            if (arg[0] != '-')
            {
                InputFile = arg;
                continue;
            }

            var param = arg[1];

            switch (param)
            {
                case 'l': ShowLexOutput = true; break;
                case 'p': ShowParseOutput = true; break;
                case 'n': NoStdLib = true; break;
                case 't': TargetTriple = args[index++]; break;
                case 'c': Cpu = args[index++]; break;
                case 'f': Features = args[index++]; break;
            }
        }
    }
}