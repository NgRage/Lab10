using System;
using System.Collections.Generic;

namespace PascalCompiler.IO
{
    public class RandomErrorGenerator
    {
        private readonly Random _random;
        private readonly List<string> _errorPool;

        public RandomErrorGenerator()
        {
            this._random = new Random();

            this._errorPool = new List<string>
            {
                "Error 2: Identifier not found",
                "Error 3: Unknown identifier",
                "Error 4: Duplicate identifier",
                "Error 5: Syntax error",
                "Error 26: Type mismatch",
                "Error 36: BEGIN expected",
                "Error 42: Error in expression",
                "Error 54: OF expected",
                "Error 85: \";\" expected",
                "Error 89: \")\" expected",
                "Error 91: \":=\" expected",
                "Error 94: \".\" expected",
                "Error 113: Error in statement",
                "Error 200: Division by zero"
            };
        }

        public IReadOnlyList<string> ErrorPool
        {
            get
            {
                return this._errorPool.AsReadOnly();
            }
        }

        public bool TryGetRandomError(out string errorMessage, int probabilityPercent = 15)
        {
            errorMessage = string.Empty;
            int roll = this._random.Next(1, 101);
            int index = 0;

            if (roll <= probabilityPercent)
            {
                index = this._random.Next(this._errorPool.Count);
                errorMessage = this._errorPool[index];
                return true;
            }

            return false;
        }
    }
}