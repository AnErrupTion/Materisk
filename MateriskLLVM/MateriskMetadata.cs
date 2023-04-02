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

    public void RemoveLast()
    {
        if (Metadata.Count > 0)
            Metadata.RemoveAt(Metadata.Count - 1);
    }

    public object? Last() => Metadata.Count > 0 ? Metadata[^1] : null;
}