﻿namespace Materisk.BuiltinTypes;

public abstract class SValue
{
    public static SValue Null => new SNull();

    public bool IsPublic { get; init; }

    public abstract SBuiltinType BuiltinName { get; }

    #region Metadata
    internal bool TypeIsFixed { get; set; } = true;
    #endregion

    public virtual SValue Add(SValue other)
    {
        throw NotSupportedOn("Add");
    }

    public virtual SValue Sub(SValue other)
    {
        throw NotSupportedOn("Sub");
    }

    public virtual SValue Mul(SValue other)
    {
        throw NotSupportedOn("Mul");
    }

    public virtual SValue Div(SValue other)
    {
        throw NotSupportedOn("Div");
    }

    public virtual SValue Mod(SValue other)
    {
        throw NotSupportedOn("Mod");
    }

    public virtual SValue Idx(SValue other)
    {
        throw NotSupportedOn("Idx");
    }

    public virtual SValue Dot(SValue other) {
        throw NotSupportedOn("Dot");
    }

    public virtual SValue DotAssignment(SValue key, SValue value) {
        throw NotSupportedOn("DotAssignment");
    }

    // TODO: Maybe force equals to be implemented?
    public virtual SValue Equals(SValue other) {
        throw NotSupportedOn("Equals");
    }

    public virtual SValue LessThan(SValue other) {
        throw NotSupportedOn("LessThan");
    }

    public virtual SValue LessThanEqu(SValue other) {
        throw NotSupportedOn("LessThanEqu");
    }

    public virtual SValue GreaterThan(SValue other) {
        throw NotSupportedOn("GreaterThan");
    }

    public virtual SValue GreaterThanEqu(SValue other) {
        throw NotSupportedOn("GreaterThanEqu");
    }

    public virtual SValue CastToBuiltin(SBuiltinType other) {
        throw NotSupportedOn("CastToBuiltin");
    }

    public virtual SValue Call(Scope scope, List<SValue> args) {
        throw NotSupportedOn("Call");
    }

    public virtual bool IsNull() {
        return false;
    }

    public virtual SValue Not()
    {
        throw NotSupportedOn("Not");
    }

    public virtual SValue ArithNot()
    {
        throw NotSupportedOn("ArithNot");
    }

    public abstract bool IsTruthy();

    public override string ToString()
    {
        return BuiltinName.ToString();
    }

    public virtual SString ToSpagString()
    {
        return new SString("<unknown of type " + BuiltinName + ">");
    }

    protected NotImplementedException NotSupportedBetween(SValue other, string type) {
        return new NotImplementedException(type + " not supported between " + BuiltinName + " and " + other.BuiltinName);
    }

    protected NotImplementedException NotSupportedOn(string type) {
        return new NotImplementedException(type + " is not supported on " + BuiltinName);
    }

    protected ArgumentException CastInvalid(string type) {
        return new ArgumentException(BuiltinName + " can not be cast to " + type);
    }

    internal void CopyMeta(ref SValue other) {
        other.TypeIsFixed = TypeIsFixed;
    }

    public string SpagToCsString() {
        return ToSpagString().Value;
    }
}