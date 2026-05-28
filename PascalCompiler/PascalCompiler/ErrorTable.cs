using System;
using System.Collections.Generic;

namespace PascalCompiler.IO
{
    public class ErrorTable
    {
        private readonly List<ErrorInfo> _errors;

        public ErrorTable()
        {
            this._errors = new List<ErrorInfo>();
        }

        public IReadOnlyList<ErrorInfo> Errors
        {
            get
            {
                return this._errors.AsReadOnly();
            }
        }

        public void AddError(int line, int column, string message)
        {
            this._errors.Add(new ErrorInfo(line, column, message));
        }

        public void PrintErrors(string[] sourceLines)
        {
            if (this._errors.Count == 0)
            {
                Console.WriteLine("\nОшибок не обнаружено.");
                return;
            }

            foreach (var error in this._errors)
            {
                Console.WriteLine(error.ToString());
                if (error.Line > 0 && error.Line <= sourceLines.Length)
                {
                    Console.WriteLine(sourceLines[error.Line - 1]);
                    if (error.Column > 0)
                    {
                        Console.WriteLine(new string(' ', error.Column - 1) + "^");
                    }
                }
                Console.WriteLine();
            }
            Console.WriteLine($"Найдено ошибок: {this._errors.Count}.");
        }
    }
}