using System;
using PascalCompiler.IO;
using PascalCompiler.Lexer;
using PascalCompiler.Semantics;

namespace PascalCompiler.Syntax
{
    public class Parser
    {
        private readonly LexicalAnalyzer _lexer;
        private readonly ErrorTable _errorTable;
        private readonly SemanticAnalyzer _semantic;
        private Token _currentToken;

        public Parser(LexicalAnalyzer lexer, ErrorTable errorTable, SemanticAnalyzer semantic)
        {
            _lexer = lexer ??
                throw new ArgumentNullException(nameof(lexer));
            _errorTable = errorTable ??
                throw new ArgumentNullException(nameof(errorTable));
            _semantic = semantic;

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

        private PascalType DeterminePascalType(TokenType type)
        {
            switch (type)
            {
                case TokenType.Integer: return PascalType.Integer;
                case TokenType.Real: return PascalType.Real;
                case TokenType.String: return PascalType.String;
                case TokenType.Boolean: return PascalType.Boolean;
                case TokenType.Char: return PascalType.Char;
                case TokenType.Record: return PascalType.Record;
                default: return PascalType.Unknown;
            }
        }

        private void ParseVarDeclarations()
        {
            Match(TokenType.Var);

            while (_currentToken.Type == TokenType.Identifier)
            {
                List<Token> idTokens = new List<Token>();
                idTokens.Add(_currentToken);
                Match(TokenType.Identifier);

                while (_currentToken.Type == TokenType.Comma)
                {
                    Match(TokenType.Comma);
                    idTokens.Add(_currentToken);
                    Match(TokenType.Identifier);
                }

                Match(TokenType.Colon);

                TypeInfo declaredType;
                if (_currentToken.Type == TokenType.Record)
                {
                    declaredType = ParseRecordType();
                }
                else
                {
                    PascalType baseType = DeterminePascalType(_currentToken.Type);
                    ParseStandardType();
                    declaredType = new TypeInfo(baseType);
                }

                Match(TokenType.Semicolon);
                foreach (var token in idTokens)
                {
                    _semantic.DeclareVariable(token.Value, declaredType, token.Line, token.Column);
                }
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

        private TypeInfo ParseRecordType()
        {
            Match(TokenType.Record);
            TypeInfo recordType = new TypeInfo(PascalType.Record);

            while (_currentToken.Type == TokenType.Identifier)
            {
                List<Token> fieldTokens = new List<Token>();
                fieldTokens.Add(_currentToken);
                Match(TokenType.Identifier);

                while (_currentToken.Type == TokenType.Comma)
                {
                    Match(TokenType.Comma);
                    fieldTokens.Add(_currentToken);
                    Match(TokenType.Identifier);
                }

                Match(TokenType.Colon);
                PascalType fieldBaseType = DeterminePascalType(_currentToken.Type);
                ParseStandardType();
                Match(TokenType.Semicolon);

                TypeInfo fieldType = new TypeInfo(fieldBaseType);

                foreach (var fToken in fieldTokens)
                {
                    if (recordType.RecordFields.ContainsKey(fToken.Value))
                    {
                        _errorTable.AddError(fToken.Line, fToken.Column,
                            $"Семантика: Дублирование поля '{fToken.Value}'" +
                            $" внутри record.");
                    }
                    else
                    {
                        recordType.RecordFields[fToken.Value] = fieldType;
                    }
                }
            }

            Match(TokenType.End);
            return recordType;
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

        private TypeInfo ParseVariableAccess()
        {
            Token idToken = _currentToken;
            Match(TokenType.Identifier);

            TypeInfo currentType = _semantic.GetVariableType(idToken.Value, idToken.Line, idToken.Column);

            while (_currentToken.Type == TokenType.Period)
            {
                Match(TokenType.Period);
                Token fieldToken = _currentToken;
                Match(TokenType.Identifier);

                if (currentType.BaseType == PascalType.Record)
                {
                    if (currentType.RecordFields.TryGetValue(fieldToken.Value, out TypeInfo fieldType))
                    {
                        currentType = fieldType;
                    }
                    else
                    {
                        _errorTable.AddError(fieldToken.Line, fieldToken.Column,
                            $"Поле '{fieldToken.Value}'" +
                            $" не найдено в структуре record.");
                        currentType = new TypeInfo(PascalType.Unknown);
                    }
                }
                else
                {
                    if (currentType.BaseType != PascalType.Unknown)
                    {
                        _errorTable.AddError(fieldToken.Line, fieldToken.Column,
                            $"Попытка обратиться через точку к переменной" +
                            $" '{idToken.Value}', которая не является записью");
                    }
                    currentType = new TypeInfo(PascalType.Unknown);
                }
            }

            return currentType;
        }

        private void ParseAssignment()
        {
            Token startToken = _currentToken;
            TypeInfo targetType = ParseVariableAccess();

            Match(TokenType.Assign);

            TypeInfo valueType = ParseExpression();

            _semantic.CheckAssignment(targetType, valueType, startToken.Line, startToken.Column);
        }

        private void ParseWithStatement()
        {
            Match(TokenType.With);
            Token recordToken = _currentToken;

            TypeInfo recType = ParseVariableAccess();

            Match(TokenType.Do);

            _semantic.EnterWithContext(recType, recordToken.Line, recordToken.Column);

            ParseStatement();

            _semantic.ExitWithContext();
        }

        private TypeInfo ParseExpression()
        {
            TypeInfo type = ParseTerm();

            while (_currentToken.Type == TokenType.Plus || _currentToken.Type == TokenType.Minus)
            {
                Token opToken = _currentToken;
                Advance();
                TypeInfo rightType = ParseTerm();

                if (type.BaseType == PascalType.Unknown || rightType.BaseType == PascalType.Unknown)
                {
                    type = new TypeInfo(PascalType.Unknown);
                }
                else if (type.BaseType == PascalType.String || rightType.BaseType == PascalType.String)
                {
                    _errorTable.AddError(opToken.Line, opToken.Column, "Семантика: Арифметические операции не примемы к типу String.");
                    type = new TypeInfo(PascalType.Unknown);
                }

                else if (type.BaseType == PascalType.Real || rightType.BaseType == PascalType.Real)
                {
                    type = new TypeInfo(PascalType.Real);
                }
                else
                {
                    type = new TypeInfo(PascalType.Integer);
                }
            }
            return type;
        }

        private TypeInfo ParseTerm()
        {
            TypeInfo type = ParseFactor();

            while (_currentToken.Type == TokenType.Multiply || _currentToken.Type == TokenType.Divide)
            {
                Token opToken = _currentToken;
                Advance();
                TypeInfo rightType = ParseFactor();

                if (type.BaseType == PascalType.Unknown || rightType.BaseType == PascalType.Unknown)
                {
                    type = new TypeInfo(PascalType.Unknown);
                }

                else if (opToken.Type == TokenType.Divide || type.BaseType == PascalType.Real || rightType.BaseType == PascalType.Real)
                {
                    type = new TypeInfo(PascalType.Real);
                }
                else
                {
                    type = new TypeInfo(PascalType.Integer);
                }
            }
            return type;
        }

        private TypeInfo ParseFactor()
        {

            if (_currentToken.Type == TokenType.Number)
            {
                PascalType numType = _currentToken.Value.Contains(".") ? PascalType.Real : PascalType.Integer;
                Advance();
                return new TypeInfo(numType);
            }

            if (_currentToken.Type == TokenType.StringLiteral)
            {
                Advance();
                return new TypeInfo(PascalType.String);
            }

            if (_currentToken.Type == TokenType.Identifier)
            {
                return ParseVariableAccess();
            }

            if (_currentToken.Type == TokenType.LeftParen)
            {
                Match(TokenType.LeftParen);
                TypeInfo type = ParseExpression();
                Match(TokenType.RightParen);
                return type;
            }

            ReportError("Ожидался идентификатор, число или '('");
            Recover();
            return new TypeInfo(PascalType.Unknown);
        }
    }
}