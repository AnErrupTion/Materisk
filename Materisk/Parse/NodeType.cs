﻿namespace Materisk.Parse;

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
    AssignExpression,
    UnaryExpression,
    Dot,
    Call,
    IntLiteral,
    FloatLiteral,
    StringLiteral,
    Identifier,
    Array,
    ArrayIndex,
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