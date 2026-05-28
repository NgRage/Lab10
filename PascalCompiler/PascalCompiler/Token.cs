using System;

namespace PascalCompiler.Lexer
{
    public class Token
    {
        private readonly TokenType _type;
        private readonly string _value;
        private readonly int _line;
        private readonly int _column;

        public Token(TokenType type, string value, int line, int col)
        {
            _type = type;
            _value = value ?? string.Empty;
            _line = line;
            _column = col;
        }

        public TokenType Type => _type;
        public string Value => _value;
        public int Line => _line;
        public int Column => _column;
        public int Code => (int)_type;

        public override string ToString()
        {
            string t = _type.ToString().PadRight(12);
            string c = Code.ToString().PadRight(3);
            return $"Токен: {t} | Код: {c} | Знач.: '{_value}'";
        }
    }
}