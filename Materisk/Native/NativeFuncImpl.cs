using System.Globalization;
using Materisk.BuiltinTypes;

namespace Materisk.Native;

public static class NativeFuncImpl
{
    public static Func<Scope, List<SValue>, SValue> GetImplFor(string name)
    {
        return name switch
        {
            "File:readText" => (_, args) =>
            {
                if (args[0] is not SString str) throw new Exception("Expected argument 0 to be a string");
                if (!File.Exists(str.Value)) throw new Exception("File not found!");

                return new SString(File.ReadAllText(str.Value));
            },
            "File:writeText" => (_, args) =>
            {
                if (args[0] is not SString str) throw new Exception("Expected argument 0 to be a string");
                if (args[1] is not SString strData) throw new Exception("Expected argument 1 to be a string");

                File.WriteAllText(str.Value, strData.Value);
                return new SInt(1);
            },
            "Int:parse" => (_, args) =>
            {
                if (args[0] is not SString str) throw new Exception("Expected argument 0 to be a string");
                if (!int.TryParse(str.Value, out var valInt)) throw new Exception("Invalid number!");

                return new SInt(valInt);
            },
            "Float:parse" => (_, args) =>
            {
                if (args[0] is not SString str) throw new Exception("Expected argument 0 to be a string");
                if (!float.TryParse(str.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var valFloat)) throw new Exception("Invalid number!");

                return new SFloat(valFloat);
            },
            "println" => (_, args) =>
            {
                Console.WriteLine(args[0].ToSpagString().Value);
                return args[0];
            },
            "print" => (_, args) =>
            {
                Console.Write(args[0].ToSpagString().Value);
                return args[0];
            },
            "read" => (_, args) =>
            {
                return new SString(Console.ReadKey().KeyChar.ToString());
            },
            "readLine" => (_, args) =>
            {
                return new SString(Console.ReadLine());
            },
            "typeOf" => (_, args) =>
            {
                var builtinType = args[0].BuiltinName;

                if(builtinType == SBuiltinType.ClassInstance) {
                    if (args[0] is not SClassInstance inst) throw new Exception("Unexpected value! BuiltinName was set to ClassInstance but it was not of type SClassInstance!");
                    return new SString(inst.Class.Name);
                }
                return new SString(builtinType.ToString());
            },
            "toString" => (_, args) =>
            {
                return args[0].ToSpagString();
            },
            "eval" => (_, args) =>
            {
                if (args[0] is not SString code) throw new Exception("Expected argument 0 to be of type string");

                Interpreter ip = new();
                InterpreterResult res = new();
                ip.Interpret(code.Value, ref res);

                return res.LastValue;
            },
            _ => throw new NotSupportedException($"Unable to find native implementation for function: {name}")
        };
    }
}