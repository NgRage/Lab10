using System;
using System.IO;

namespace PascalCompiler.IO
{
    public class InputOutputModule : IDisposable
    {
        private readonly StreamReader _reader;
        private readonly ErrorTable _errorTable;
        private readonly RandomErrorGenerator _errorGenerator;
        private int _currentLine;
        private int _currentColumn;
        private bool _isEof;
        private bool _disposed;

        public InputOutputModule(string filePath, ErrorTable errorTable)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                string msg = "Путь к файлу не может быть пустым.";
                throw new ArgumentException(msg, nameof(filePath));
            }

            _reader = new StreamReader(filePath);
            _errorTable = errorTable ??
                throw new ArgumentNullException(nameof(errorTable));
            _errorGenerator = new RandomErrorGenerator();

            _currentLine = 1;
            _currentColumn = 0;
            _isEof = false;
        }

        public int CurrentLine => _currentLine;
        public int CurrentColumn => _currentColumn;
        public bool IsEof => _isEof;

        public char NextCh()
        {
            if (_isEof) return '\0';

            int nextCharInt = _reader.Read();

            if (nextCharInt == -1)
            {
                _isEof = true;
                return '\0';
            }

            char ch = (char)nextCharInt;

            if (ch == '\n')
            {
                _currentLine++;
                _currentColumn = 0;
            }
            else if (ch != '\r')
            {
                _currentColumn++;
            }

            if (ch != '\r' && ch != '\n' &&
                _errorGenerator.TryGetRandomError(out string rndMsg, 10))
            {
                _errorTable.AddError(_currentLine, _currentColumn, rndMsg);
            }

            return ch;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing) _reader?.Dispose();
                _disposed = true;
            }
        }
    }
}