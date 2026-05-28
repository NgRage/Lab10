using System;
using System.Collections.Generic;
using System.Text;
using PascalCompiler.IO;

namespace PascalCompiler.Lexer
{
    public class LexicalAnalyzer
    {
        private readonly InputOutputModule _io;
        private readonly ErrorTable _errorTable;
        private readonly Dictionary<string, TokenType> _keywords;
        private char _currentChar;

        public LexicalAnalyzer(InputOutputModule io, ErrorTable errors)
        {
            _io = io ??
                throw new ArgumentNullException(nameof(io));
            _errorTable = errors ??
                throw new ArgumentNullException(nameof(errors));

            var cmp = StringComparer.OrdinalIgnoreCase;
            _keywords = new Dictionary<string, TokenType>(cmp)
            {
                { "program", TokenType.Program },
                { "var", TokenType.Var },
                { "const", TokenType.Const },
                { "begin", TokenType.Begin },
                { "end", TokenType.End }
            };

            Advance();
        }

        private void Advance()
        {
            _currentChar = _io.NextCh();
        }

        public Token GetNextToken()
        {
            while (char.IsWhiteSpace(_currentChar))
            {
                Advance();
            }

            if (_io.IsEof || _currentChar == '\0')
            {
                int eLine = _io.CurrentLine;
                int eCol = _io.CurrentColumn;
                return new Token(TokenType.Eof, "EOF", eLine, eCol);
            }

            int startLine = _io.CurrentLine;
            int startCol = _io.CurrentColumn;

            if (char.IsLetter(_currentChar))
            {
                StringBuilder sb = new StringBuilder();

                while (char.IsLetterOrDigit(_currentChar))
                {
                    sb.Append(_currentChar);
                    Advance();
                }

                string val = sb.ToString();
                bool hasKey = _keywords.ContainsKey(val);
                TokenType type = hasKey ? _keywords[val] :
                    TokenType.Identifier;

                return new Token(type, val, startLine, startCol);
            }

            if (char.IsDigit(_currentChar))
            {
                StringBuilder sb = new StringBuilder();

                while (char.IsDigit(_currentChar))
                {
                    sb.Append(_currentChar);
                    Advance();
                }

                string numStr = sb.ToString();

                if (!int.TryParse(numStr, out _))
                {
                    string msg = $"Число {numStr} вне диапазона.";
                    _errorTable.AddError(startLine, startCol, msg);
                }

                return new Token(TokenType.Number, numStr,
                    startLine, startCol);
            }

            char ch = _currentChar;
            Advance();

            switch (ch)
            {
                case ';':
                    return new Token(TokenType.Semicolon, ";",
                        startLine, startCol);
                case ':':
                    if (_currentChar == '=')
                    {
                        Advance();
                        return new Token(TokenType.Assign, ":=",
                            startLine, startCol);
                    }
                    return new Token(TokenType.Colon, ":",
                        startLine, startCol);
                case '=':
                    return new Token(TokenType.Equal, "=",
                        startLine, startCol);
                case '$':
                    _errorTable.AddError(startLine, startCol,
                        "Символ '$'");
                    return new Token(TokenType.Dollar, "$",
                        startLine, startCol);
                default:
                    string msg = $"Неизвестный символ: '{ch}'";
                    _errorTable.AddError(startLine, startCol, msg);
                    return new Token(TokenType.Unknown, ch.ToString(),
                        startLine, startCol);
            }
        }
    }
}