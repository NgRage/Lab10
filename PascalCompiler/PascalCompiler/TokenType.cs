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
        Identifier = 10,
        Number = 11,
        Semicolon = 20,
        Colon = 21,
        Equal = 22,
        Assign = 23,
        Dollar = 99,
        Eof = 255
    }
}