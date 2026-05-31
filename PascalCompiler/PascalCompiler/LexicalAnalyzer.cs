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

        public void Advance()
        {
            if (_hasPeek)
            {
                _currentChar = _peekChar;
                _hasPeek = false;
            }
            else
        {
            _currentChar = _io.NextCh();
        }
        }

        public char Peek()
        {
            if (!_hasPeek)
            {
                _peekChar = _io.NextCh();
                _hasPeek = true;
            }
            return _peekChar;
        }

        public void SkipWhitespaceAndComments()
        {
            while (true)
            {
                if (char.IsWhiteSpace(_currentChar))
                {
                    Advance();
                }
                else if (_currentChar == '{')
                {
                    int sLine = _io.CurrentLine;
                    int sCol = _io.CurrentColumn;
                    Advance();
                    bool isClosed = false;

                    while (_currentChar != '\0')
                    {
                        if (_currentChar == '}')
                        {
                            isClosed = true;
                            Advance();
                            break;
                        }
                        Advance();
                    }
                    if (!isClosed)
                    {
                        string msg = "Незакрытый комментарий '{'";
                        _errorTable.AddError(sLine, sCol, msg);
                    }
                }
                else if (_currentChar == '(' && Peek() == '*')
                {
                    int sLine = _io.CurrentLine;
                    int sCol = _io.CurrentColumn;
                    Advance();
                    Advance();
                    bool isClosed = false;

                    while (_currentChar != '\0')
                    {
                        if (_currentChar == '*' && Peek() == ')')
                        {
                            Advance();
                            Advance();
                            isClosed = true;
                            break;
                        }
                        Advance();
                    }
                    if (!isClosed)
                    {
                        string msg = "Незакрытый комментарий '(*'";
                        _errorTable.AddError(sLine, sCol, msg);
                    }
                }
                else if (_currentChar == '/' && Peek() == '/')
        {
                    Advance();
                    Advance();
                    while (_currentChar != '\n' && _currentChar != '\0')
            {
                Advance();
            }
                }
                else
                {
                    break;
                }
            }
        }

        public Token GetNextToken()
        {
            SkipWhitespaceAndComments();

            if (_io.IsEof || _currentChar == '\0')
            {
                return new Token(TokenType.Eof, "EOF",
                    _io.CurrentLine, _io.CurrentColumn);
            }

            int startLine = _io.CurrentLine;
            int startCol = _io.CurrentColumn;

            if (char.IsLetter(_currentChar) || _currentChar == '_')
            {
                StringBuilder sb = new StringBuilder();
                while (char.IsLetterOrDigit(_currentChar) ||
                       _currentChar == '_')
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

            if (_currentChar == '\'')
            {
                Advance();
                StringBuilder sb = new StringBuilder();
                bool isClosed = false;

                while (_currentChar != '\0' && _currentChar != '\n')
                {
                    if (_currentChar == '\'')
                    {
                        isClosed = true;
                        Advance();
                        break;
                    }
                    sb.Append(_currentChar);
                    Advance();
                }

                if (!isClosed)
                {
                    string msg = "Незакрытая строковая кавычка '''";
                    _errorTable.AddError(startLine, startCol, msg);
                }

                return new Token(TokenType.StringLiteral, sb.ToString(),
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
                case '+':
                    return new Token(TokenType.Plus, "+",
                        startLine, startCol);
                case '-':
                    return new Token(TokenType.Minus, "-",
                        startLine, startCol);
                case '*':
                    return new Token(TokenType.Multiply, "*",
                        startLine, startCol);
                case '/':
                    return new Token(TokenType.Divide, "/",
                        startLine, startCol);
                case '(':
                    return new Token(TokenType.LeftParen, "(",
                        startLine, startCol);
                case ')':
                    return new Token(TokenType.RightParen, ")",
                        startLine, startCol);
                default:
                    string msg = $"Недопустимый символ: '{ch}'";
                    _errorTable.AddError(startLine, startCol, msg);
                    return new Token(TokenType.Unknown, ch.ToString(),
                        startLine, startCol);
            }
        }
    }
}