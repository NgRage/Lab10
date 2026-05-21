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
                throw new ArgumentException("Путь не может быть пустым.",
                    nameof(filePath));
            }

            this._reader = new StreamReader(filePath);
            this._errorTable = errorTable ??
                throw new ArgumentNullException(nameof(errorTable));
            this._errorGenerator = new RandomErrorGenerator();
            this._currentLine = 1;
            this._currentColumn = 0;
            this._isEof = false;
        }

        public int CurrentLine
        {
            get
            {
                return this._currentLine;
            }
        }

        public int CurrentColumn
        {
            get
            {
                return this._currentColumn;
            }
        }

        public bool IsEof
        {
            get
            {
                return this._isEof;
            }
        }

        public char NextCh()
        {
            if (this._isEof)
            {
                return '\0';
            }

            int nextCharInt = this._reader.Read();

            if (nextCharInt == -1)
            {
                this._isEof = true;
                return '\0';
            }

            char ch = (char)nextCharInt;

            if (ch == '\n')
            {
                this._currentLine++;
                this._currentColumn = 0;
            }
            else if (ch != '\r')
            {
                this._currentColumn++;
            }

            if (ch != '\r' && ch != '\n' &&
                this._errorGenerator.TryGetRandomError
                (out string randomMessage, 10))
            {
                this._errorTable.AddError(this._currentLine,
                    this._currentColumn, randomMessage);
            }

            return ch;
        }

        public void Close()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    this._reader?.Dispose();
                }
                this._disposed = true;
            }
        }
    }
}