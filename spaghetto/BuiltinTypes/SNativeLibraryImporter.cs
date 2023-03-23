namespace spaghetto;

public class SNativeLibraryImporter : SValue {
    public override SBuiltinType BuiltinName => SBuiltinType.NativeLibraryImporter;
    public Action<Scope> Import { get; } = (Scope scope) => { };

    public SNativeLibraryImporter(Action<Scope> import) {
        Import = import;
    }

    public override bool IsTruthy() {
        return true;
    }
}