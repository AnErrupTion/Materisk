namespace Materisk.Parsing;

public enum NodeType
{
    Return,
    BinaryExpression,
    Token,
    BooleanLiteral,
    Block,
    Continue,
    Break,
    InitVariable,
    AssignVariable,
    UnaryExpression,
    Dot,
    Call,
    IntLiteral,
    FloatLiteral,
    StringLiteral,
    Identifier,
    List,
    If,
    For,
    Cast,
    While,
    FunctionDefinition,
    FieldDefinition,
    ModuleFieldDefinition,
    Instantiate,
    ModuleDefinition,
    ModuleFunctionDefinition,
    Import
}