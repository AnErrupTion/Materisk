﻿namespace Materisk.Parse;

public enum NodeType
{
    Return,
    BinaryExpression,
    BooleanLiteral,
    Block,
    Continue,
    Break,
    InitVariable,
    AssignExpression,
    UnaryExpression,
    Dot,
    Call,
    SByteLiteral,
    ByteLiteral,
    ShortLiteral,
    UShortLiteral,
    UIntLiteral,
    IntLiteral,
    ULongLiteral,
    LongLiteral,
    FloatLiteral,
    DoubleLiteral,
    StringLiteral,
    Identifier,
    Array,
    Index,
    If,
    For,
    Cast,
    While,
    FunctionDefinition,
    FieldDefinition,
    ModuleFieldDefinition,
    Instantiate,
    ModuleDefinition,
    ModuleFunctionDefinition
}