using System;
using System.Collections.Generic;
using PascalCompiler.IO;

namespace PascalCompiler.Semantics
{
    public class SemanticAnalyzer
    {
        private readonly ErrorTable _errorTable;
        private readonly Dictionary<string, TypeInfo> _symbolTable;
        private TypeInfo _currentWithRecord = null;

        public SemanticAnalyzer(ErrorTable errorTable)
        {
            _errorTable = errorTable ??
                throw new ArgumentNullException(nameof(errorTable));
            _symbolTable = new Dictionary<string, TypeInfo>(
                StringComparer.OrdinalIgnoreCase);
        }
        public void DeclareVariable(string name, TypeInfo type,
            int line, int col)
        {
            if (_symbolTable.ContainsKey(name))
            {
                _errorTable.AddError(line, col,
                    $"Дублирование имени. '{name}' уже объявлен.");
            }
            else
            {
                _symbolTable[name] = type;
            }
        }
        public TypeInfo GetVariableType(string name, int line, int col)
        {
            if (_currentWithRecord != null &&
                _currentWithRecord.RecordFields.ContainsKey(name))
            {
                return _currentWithRecord.RecordFields[name];
            }

            if (_symbolTable.ContainsKey(name))
            {
                return _symbolTable[name];
            }

            _errorTable.AddError(line, col,
                $"Неизвестный идентификатор '{name}'.");

            return new TypeInfo(PascalType.Unknown);
        }

        public void CheckAssignment(TypeInfo target, TypeInfo value,
            int line, int col)
        {
            if (target.BaseType == PascalType.Unknown ||
                value.BaseType == PascalType.Unknown)
            {
                return;
            }

            if (target.BaseType == PascalType.Real &&
                value.BaseType == PascalType.Integer)
            {
                return;
            }

            if (target.BaseType != value.BaseType)
            {
                string msg = $"Несовпадение типов. Нельзя " +
                             $"присвоить {value.BaseType} в {target.BaseType}.";
                _errorTable.AddError(line, col, msg);
            }
        }
        public void EnterWithContext(TypeInfo recordType, int line, int col)
        {
            if (recordType.BaseType != PascalType.Record)
            {
                _errorTable.AddError(line, col,
                    "В 'with' можно передавать только тип record.");
                return;
            }
            _currentWithRecord = recordType;
        }

        public void ExitWithContext()
        {
            _currentWithRecord = null;
        }
    }
}