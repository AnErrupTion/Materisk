using Materisk.Parse.Nodes;

namespace Materisk.BuiltinTypes;

public class SFunction : SBaseFunction {
    public override SBuiltinType BuiltinName => SBuiltinType.Function;
    public SyntaxNode Callback { get; }
    public Scope DefiningScope { get; }


    public SFunction(Scope definingScope, string functionName, List<string> args, SyntaxNode callback) {
        DefiningScope = definingScope;
        FunctionName = functionName;
        ExpectedArgs = args;
        Callback = callback;
    }

    public override SValue Call(Scope scope, List<SValue> args) {
        if (args.Count != ExpectedArgs.Count) throw new Exception(FunctionName + " expected " + ExpectedArgs.Count + " arguments. (" + string.Join(", ", ExpectedArgs) + ")");

        Scope funcScope = new(DefiningScope);
            
        for(var i = 0; i < ExpectedArgs.Count; i++) {
            funcScope.Set(ExpectedArgs[i], args[i]);
        }

        Callback.Evaluate(funcScope);

        return funcScope.ReturnValue;
    }

    public override bool IsTruthy() {
        return true;
    }
}