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

    public override object Emit(ModuleDefinition module, CilMethodBody body)
    {
        // TODO: Check if variable already exists
        
        if (expr != null)
        {
            var value = expr.Emit(module, body);
            var variable = new CilLocalVariable(Utils.GetTypeSignatureFor(module, value.GetType()));
            body.LocalVariables.Add(variable);
            body.Instructions.Add(CilOpCodes.Stloc, variable);
            return value;
        }

        if (isFixedType) throw new InvalidOperationException("Tried to initiliaze a fixed type variable with no value; this is not permitted. Use var% instead.");

        var nullVariable = new CilLocalVariable(module.CorLibTypeFactory.Object);
        body.LocalVariables.Add(nullVariable);
        body.Instructions.Add(CilOpCodes.Stloc, nullVariable);
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