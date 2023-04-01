namespace MateriskCLI;

public class Settings
{
    public readonly string InputFile;
    public readonly bool ShowLexOutput, ShowParseOutput, NoStdLib;

    public Settings(ref string[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
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
            }
        }
    }
}