using System;
using System.Collections.Generic;
using System.IO;
using PascalCompiler.IO;
using PascalCompiler.Lexer;
using PascalCompiler.Syntax;

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
                string msg = $"Ошибка: Файл '{file1}' не найден!";
                Console.WriteLine(msg);
                Console.ReadLine();
                return;
            }

            string code = File.ReadAllText(file1);
            string[] sourceLines = File.ReadAllLines(file1);

            Console.WriteLine("Исходный код");
            Console.WriteLine(code);
            Console.WriteLine("\n");

            ErrorTable lexErrors = new ErrorTable();
            List<int> outputCodes = new List<int>();

            using (InputOutputModule io = new InputOutputModule(file1,
                lexErrors))
            {
                LexicalAnalyzer lexer = new LexicalAnalyzer(io, lexErrors);
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

            Console.WriteLine("\nФайл 2: ");
            Console.WriteLine(file2Content);

            Console.WriteLine("\nАнализ: ");

            ErrorTable allErrors = new ErrorTable();

            using (InputOutputModule io = new InputOutputModule(file1,
                allErrors))
            {
                LexicalAnalyzer lexer = new LexicalAnalyzer(io, allErrors);
                Parser parser = new Parser(lexer, allErrors);
                parser.ParseProgram();
            }

            allErrors.PrintErrors(sourceLines);
        }
    }
}