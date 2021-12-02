using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Common.Sql.Programmatics
{
    public class GenericParameter
    {
        public GenericParameter(int index, string name, DeciaDataType dataType)
            : this(index, name, dataType, string.Empty)
        { }

        public GenericParameter(int index, string name, DeciaDataType dataType, string defaultValueAsText)
        {
            Index = index;
            Name = name;
            DataType = dataType;
            DefaultValue = defaultValueAsText;
        }

        public int Index { get; protected set; }
        public string Name { get; protected set; }
        public DeciaDataType DataType { get; protected set; }

        public bool HasSourceTableName { get { return !string.IsNullOrWhiteSpace(SourceTableName); } }
        public string SourceTableName { get; set; }
        public bool HasDefaultValue { get { return !string.IsNullOrWhiteSpace(DefaultValue); } }
        public string DefaultValue { get; set; }
    }
}