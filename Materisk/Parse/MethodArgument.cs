namespace Materisk.Parse;

internal sealed class MethodArgument
{
    public readonly string Type;
    public readonly string SecondType;
    public readonly string Name;

    public MethodArgument(string type, string secondType, string name)
    {
        Type = type;
        SecondType = secondType;
        Name = name;
    }
}