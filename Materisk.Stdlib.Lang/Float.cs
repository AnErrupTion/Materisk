using System.Globalization;
using Materisk.BuiltinTypes;

namespace Materisk.Stdlib.Lang;

public class Float {
    public static SClass CreateClass() {
        var @class = new SClass("float");

        @class.StaticTable.Add((new SString("parse"), new SNativeFunction(
            impl: (Scope scope, List<SValue> args) => {
                if (args[0] is not SString str) throw new Exception("Expected argument 0 to be a string");
                if (!float.TryParse(str.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var valFloat)) throw new Exception("Invalid number!");

                return new SFloat(valFloat);
            },
            expectedArgs: new() { "toParse" }
        ){
            IsPublic = true
        }));

        return @class;
    }
}