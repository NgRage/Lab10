namespace PascalCompiler.Lexer
{
    public enum TokenType
    {
        Unknown = 0,

        Program = 1,
        Var = 2,
        Const = 3,
        Begin = 4,
        End = 5,
        Type = 6,
        Array = 7,
        Record = 8,
        Of = 9,
        With = 10,
        Do = 11,
        If = 12,
        Then = 13,
        Else = 14,
        While = 15,
        For = 16,
        To = 17,
        Downto = 18,
        Repeat = 19,
        Until = 20,
        Case = 21,
        Procedure = 22,
        Function = 23,

        Integer = 24,
        Real = 25,
        Char = 26,
        Boolean = 27,
        String = 28,

        Identifier = 29,
        Number = 30,
        StringLiteral = 31,

        Semicolon = 32,
        Colon = 33,
        Equal = 34,
        Assign = 35,
        Comma = 36,
        Period = 37,
        Plus = 38,
        Minus = 39,
        Multiply = 40,
        Divide = 41,
        LessThan = 42,
        GreaterThan = 43,
        LessOrEqual = 44,
        GreaterOrEqual = 45,
        NotEqual = 46,
        LeftParen = 47,
        RightParen = 48,
        LeftBracket = 49,
        RightBracket = 50,

        Eof = 255
    }
}