namespace NativeCIL;

public class Settings
{
    public readonly string InputFile;
    public readonly bool ShowLexOutput, ShowParseOutput;

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

            string param, argument;
            if (arg.StartsWith("--"))
            {
                param = arg[2..];
                argument = args[i++];
            }
            else
            {
                param = arg[1].ToString();
                argument = arg[2..];
            }

            switch (param)
            {
                case "show-lex-output":
                case "l":
                    ShowLexOutput = true;
                    break;

                case "show-parse-output":
                case "p":
                    ShowParseOutput = true;
                    break;
            }
        }
    }
}