using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using Materisk.BuiltinTypes;

namespace Materisk.Parsing.Nodes;

internal class InitVariableNode : SyntaxNode
{
    private readonly SyntaxToken ident;
    private readonly SyntaxNode expr;
    private readonly bool isFixedType = true;

    public InitVariableNode(SyntaxToken ident, bool isFixedType)
    {
        this.ident = ident;
        this.isFixedType = isFixedType;
    }

    public InitVariableNode(SyntaxToken ident, SyntaxNode expr, bool isFixedType)
    {
        this.ident = ident;
        this.expr = expr;
        this.isFixedType = isFixedType;
    }

    public override NodeType Type => NodeType.InitVariable;

    public override SValue Evaluate(Scope scope)
    {
        if (scope.Get(ident.Value.ToString()) != null)
        {
            throw new InvalidOperationException("Can not initiliaze the same variable twice!");
        }

        if (expr != null)
        {
            var val = expr.Evaluate(scope);
            val.TypeIsFixed = isFixedType;

            scope.Set(ident.Value.ToString(), val);
            return val;
        }

        if (isFixedType) throw new InvalidOperationException("Tried to initiliaze a fixed type variable with no value; this is not permitted. Use var% instead.");

        scope.Set(ident.Value.ToString(), SValue.Null);
        return SValue.Null;

    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, MethodDefinition method, List<string> arguments)
    {
        var name = ident.Value.ToString();

        if (name is null)
            throw new InvalidOperationException("Can not assign to a non-existent identifier!");

        if (variables.ContainsKey(name))
            throw new InvalidOperationException("Can not initialize the same variable twice!");

        if (expr != null)
        {
            var value = expr.Emit(variables, module, method, arguments);
            var variable = new CilLocalVariable(Utils.GetTypeSignatureFor(module, value.GetType()));
            method.CilMethodBody?.LocalVariables.Add(variable);
            method.CilMethodBody?.Instructions.Add(CilOpCodes.Stloc, variable);
            variables.Add(name, variable);
            return value;
        }

        if (isFixedType)
            throw new InvalidOperationException("Tried to initialize a fixed type variable with no value; this is not permitted. Use var% instead.");

        var nullVariable = new CilLocalVariable(module.CorLibTypeFactory.Object);
        method.CilMethodBody?.LocalVariables.Add(nullVariable);
        method.CilMethodBody?.Instructions.Add(CilOpCodes.Stloc, nullVariable);
        variables.Add(name, nullVariable);
        return null;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(ident);
        if (expr != null) yield return expr;
    }

    public override string ToString()
    {
        return "InitVariableNode:";
    }
}