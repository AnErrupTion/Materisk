using Materisk.BuiltinTypes;

namespace Materisk.Stdlib.Lang;

public class Int {
    public static SClass CreateClass() {
        var @class = new SClass("int");

        @class.StaticTable.Add((new SString("parse"), new SNativeFunction(
            impl: (Scope scope, List<SValue> args) => {
                if (args[0] is not SString str) throw new Exception("Expected argument 0 to be a string");
                if (!int.TryParse(str.Value, out var valInt)) throw new Exception("Invalid number!");

                return new SInt(valInt);
            },
            expectedArgs: new() { "toParse" }
        )));

        return @class;
    }
}