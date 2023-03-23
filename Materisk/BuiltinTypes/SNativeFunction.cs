﻿namespace Materisk.BuiltinTypes;

public class SNativeFunction : SBaseFunction {
    public override SBuiltinType BuiltinName => SBuiltinType.NativeFunc;
    public Func<Scope, List<SValue>, SValue> Impl { get; }

    public SNativeFunction(string name, Func<Scope, List<SValue>, SValue> impl)
    {
        FunctionName = name;
        Impl = impl;
        ExpectedArgs = new();
    }

    public SNativeFunction(string name, Func<Scope, List<SValue>, SValue> impl, List<string> expectedArgs, bool isClassInstanceFunc = false)
    {
        FunctionName = name;
        Impl = impl;
        ExpectedArgs = expectedArgs;
        IsClassInstanceMethod = isClassInstanceFunc;
    }

    /// <summary>
    /// NOTE: The scope in SNativeFunction is the calling scope, but not in SFunction!
    /// </summary>
    /// <param name="scope"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public override SValue Call(Scope scope, List<SValue> args) {
        if (args.Count != ExpectedArgs.Count) throw new Exception("Expected " + ExpectedArgs.Count + " arguments. (" + string.Join(", ", ExpectedArgs) + ")");

        return Impl(scope, args);
    }

    public override bool IsTruthy() {
        return true;
    }
}