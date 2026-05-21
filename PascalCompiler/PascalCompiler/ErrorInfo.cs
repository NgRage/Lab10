using System;

namespace PascalCompiler.IO
{
    public class ErrorInfo
    {
        private readonly int _line;
        private readonly int _column;
        private readonly string _message;

        public ErrorInfo(int line, int column, string message)
        {
            this._line = line;
            this._column = column;
            this._message = message ??
                throw new ArgumentNullException(nameof(message));
        }

        public int Line
        {
            get
            {
                return this._line;
            }
        }

        public int Column
        {
            get
            {
                return this._column;
            }
        }

        public string Message
        {
            get
            {
                return this._message;
            }
        }

        public override string ToString()
        {
            return $"Ошибка ({this._line}, {this._column}):" +
                $" {this._message}";
        }
    }
}