using System;
using System.Collections.Generic;

namespace PascalCompiler.Semantics
{
    public class TypeInfo
    {
        private readonly PascalType _baseType;
        private readonly Dictionary<string, TypeInfo> _recordFields;

        public TypeInfo(PascalType type)
        {
            _baseType = type;
            _recordFields = new Dictionary<string, TypeInfo>(
                StringComparer.OrdinalIgnoreCase);
        }

        public PascalType BaseType => _baseType;
        public Dictionary<string, TypeInfo> RecordFields => _recordFields;
    }
}