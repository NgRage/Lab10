using System;
using System.IO;
using PascalCompiler.IO;

namespace PascalCompiler
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string testFileName = "Test.txt";

            ErrorTable errorTable = new ErrorTable();

            using (InputOutputModule ioModule = new InputOutputModule(testFileName, errorTable))
            {
                while (!ioModule.IsEof)
                {
                    ioModule.NextCh();
                }
            }

            string[] sourceLines = File.ReadAllLines(testFileName);

            errorTable.PrintErrors(sourceLines);

            Console.ReadLine();
        }
    }
}