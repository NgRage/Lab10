using System;
using PascalCompiler.IO;
using PascalCompiler.Lexer;

namespace PascalCompiler.Syntax
{
    public class Parser
    {
        private readonly LexicalAnalyzer _lexer;
        private readonly ErrorTable _errorTable;
        private Token _currentToken;

        public Parser(LexicalAnalyzer lexer, ErrorTable errorTable)
        {
            _lexer = lexer ??
                throw new ArgumentNullException(nameof(lexer));
            _errorTable = errorTable ??
                throw new ArgumentNullException(nameof(errorTable));

            Advance();
        }

        private void Advance()
        {
            _currentToken = _lexer.GetNextToken();
        }

        private void ReportError(string message)
        {
            _errorTable.AddError(_currentToken.Line,
                _currentToken.Column, message);
        }

        private void Recover()
        {
            while (_currentToken.Type != TokenType.Semicolon &&
                    _currentToken.Type != TokenType.End &&
                    _currentToken.Type != TokenType.Eof)
            {
                Advance();
            }
        }

        private void Match(TokenType expected)
        {
            if (_currentToken.Type == expected)
            {
                Advance();
            }
            else
            {
                string msg = $"Ожидалось '{expected}', " +
                             $"но встречено '{_currentToken.Value}'";
                ReportError(msg);
                Recover(); 
            }
        }

        public void ParseProgram()
        {
            if (_currentToken.Type == TokenType.Program)
            {
                Advance();
                Match(TokenType.Identifier);
                Match(TokenType.Semicolon);
            }

            if (_currentToken.Type == TokenType.Var)
            {
                ParseVarDeclarations();
            }

            ParseCompoundStatement();
            Match(TokenType.Period);
        }

        private void ParseVarDeclarations()
        {
            Match(TokenType.Var);
            while (_currentToken.Type == TokenType.Identifier)
            {
                Advance(); 
                Match(TokenType.Colon);

                if (_currentToken.Type == TokenType.Record)
                {
                    ParseRecordType();
                }
                else
                {
                    ParseStandardType();
                }
                Match(TokenType.Semicolon);
            }
        }

        private void ParseStandardType()
        {
            if (_currentToken.Type == TokenType.Integer ||
                _currentToken.Type == TokenType.Real ||
                _currentToken.Type == TokenType.Boolean ||
                _currentToken.Type == TokenType.Char)
            {
                Advance();
            }
            else
            {
                ReportError("Ожидался стандартный тип данных");
                Recover();
            }
        }

        private void ParseRecordType()
        {
            Match(TokenType.Record);
            while (_currentToken.Type == TokenType.Identifier)
            {
                Advance();
                Match(TokenType.Colon);
                ParseStandardType();
                Match(TokenType.Semicolon);
            }
            Match(TokenType.End);
        }

        private void ParseCompoundStatement()
        {
            Match(TokenType.Begin);
            while (_currentToken.Type != TokenType.End &&
                   _currentToken.Type != TokenType.Eof)
            {
                ParseStatement();

                if (_currentToken.Type == TokenType.Semicolon)
                {
                    Advance();
                }
                else if (_currentToken.Type != TokenType.End)
                {
                    ReportError("Ожидалась ';' между операторами");
                    Recover();
                }
            }
            Match(TokenType.End);
        }

        private void ParseStatement()
        {
            if (_currentToken.Type == TokenType.Identifier)
            {
                ParseAssignment();
            }
            else if (_currentToken.Type == TokenType.With)
            {
                ParseWithStatement();
            }
            else if (_currentToken.Type == TokenType.Begin)
            {
                ParseCompoundStatement();
            }
        }

        private void ParseVariableAccess()
        {
            Match(TokenType.Identifier);
            
            if (_currentToken.Type == TokenType.Period)
            {
                Advance();
                Match(TokenType.Identifier);
            }
        }

        private void ParseAssignment()
        {
            ParseVariableAccess();
            Match(TokenType.Assign);
            ParseExpression();
        }

        private void ParseWithStatement()
        {
            Match(TokenType.With);
            ParseVariableAccess();
            Match(TokenType.Do);
            ParseStatement();
        }

        private void ParseExpression()
        {
            ParseTerm();
            while (_currentToken.Type == TokenType.Plus ||
                   _currentToken.Type == TokenType.Minus)
            {
                Advance();
                ParseTerm();
            }
        }

        private void ParseTerm()
        {
            ParseFactor();
            while (_currentToken.Type == TokenType.Multiply ||
                   _currentToken.Type == TokenType.Divide)
            {
                Advance();
                ParseFactor();
            }
        }

        private void ParseFactor()
        {
            if (_currentToken.Type == TokenType.Identifier)
            {
                ParseVariableAccess();
            }
            else if (_currentToken.Type == TokenType.Number ||
                     _currentToken.Type == TokenType.StringLiteral)
            {
                Advance();
            }
            else if (_currentToken.Type == TokenType.LeftParen)
            {
                Advance();
                ParseExpression();
                Match(TokenType.RightParen);
            }
            else
            {
                ReportError("Ожидался идентификатор, число или '('");
                Recover();
            }
        }
    }
}