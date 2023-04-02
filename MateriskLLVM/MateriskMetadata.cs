namespace MateriskLLVM;

public sealed class MateriskMetadata
{
    public readonly List<object> Metadata;

    public MateriskMetadata(params object[] metadata)
    {
        Metadata = new();
        Metadata.AddRange(metadata);
    }

    public void AddMetadata(object metadata) => Metadata.Add(metadata);
}