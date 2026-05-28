using System;
using System.Collections.Generic;
using System.IO;
using PascalCompiler.IO;
using PascalCompiler.Lexer;

namespace PascalCompiler
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string file1 = "file1_input.txt";
            string file2 = "file2_output.txt";

            if (!File.Exists(file1))
            {
                Console.WriteLine($"Ошибка: Файл '{file1}' не найден!");
                return;
            }

            string code = File.ReadAllText(file1);
            Console.WriteLine("Файл 1:");
            Console.WriteLine(code);
            Console.WriteLine("\n");

            ErrorTable errorTable = new ErrorTable();
            List<int> outputCodes = new List<int>();


            using (InputOutputModule io = new InputOutputModule(file1, errorTable))
            {
                LexicalAnalyzer lexer = new LexicalAnalyzer(io, errorTable);
                Token token = lexer.GetNextToken();

                while (token.Type != TokenType.Eof)
                {
                    Console.WriteLine(token.ToString());
                    outputCodes.Add(token.Code);
                    token = lexer.GetNextToken();
                }
            }

            string file2Content = string.Join(" ", outputCodes);
            File.WriteAllText(file2, file2Content);

            Console.WriteLine("\nФайл 2:");
            Console.WriteLine(file2Content);
            Console.WriteLine("\n");

            string[] sourceLines = File.ReadAllLines(file1);
            errorTable.PrintErrors(sourceLines);
        }
    }
}