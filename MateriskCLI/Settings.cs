namespace NativeCIL;

public class Settings
{
    public readonly string InputFile, CustomCoreLib;
    public readonly bool ShowLexOutput, ShowParseOutput;
    public readonly List<string> References;

    public Settings(ref string[] args)
    {
        References = new();

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (arg[0] != '-')
            {
                InputFile = arg;
                continue;
            }

            var param = arg[1].ToString();
            var argument = args[++i];

            switch (param)
            {
                case "l":
                    ShowLexOutput = true;
                    break;

                case "p":
                    ShowParseOutput = true;
                    break;

                case "c":
                    CustomCoreLib = argument;
                    break;
                
                case "a":
                    References.Add(argument);
                    break;
            }
        }
    }
}