namespace MateriskLLVM;

public sealed class MateriskMetadata
{
    public readonly List<object> Metadata;

    public MateriskMetadata() => Metadata = new();

    public void AddMetadata(object metadata) => Metadata.Add(metadata);

    public void RemoveLast()
    {
        if (Metadata.Count > 0)
            Metadata.RemoveAt(Metadata.Count - 1);
    }

    public object? Last()
    {
        if (Metadata.Count > 0)
        {
            var value = Metadata[^1];
            RemoveLast();
            return value;
        }

        return null;
    }
}