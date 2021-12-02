using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Decia.Business.Common;
using Decia.Business.Common.Modeling;
using Decia.Business.Domain.Formulas.Expressions;

namespace Decia.Business.Domain.Formulas.Parsing
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SymbolTable
    {
        public const char Symbol_ForRefs = '§';
        public const char Symbol_ForValues = '¶';

        public const string MarkerFormat_ForRefs = "§({0})";
        public const string MarkerFormat_ForValues = "¶({0})";

        public SymbolTable()
        {
            ProcessedRefs = new Dictionary<int, ProcessedRef>();
            ProcessedValues = new Dictionary<int, ProcessedValue>();
            ProcessedRefsByMarker = new Dictionary<string, ProcessedRef>();
            ProcessedValuesByMarker = new Dictionary<string, ProcessedValue>();
        }

        [JsonProperty]
        public Dictionary<int, ProcessedRef> ProcessedRefs { get; protected set; }
        [JsonProperty]
        public Dictionary<int, ProcessedValue> ProcessedValues { get; protected set; }
        [JsonProperty]
        public Dictionary<string, ProcessedRef> ProcessedRefsByMarker { get; protected set; }
        [JsonProperty]
        public Dictionary<string, ProcessedValue> ProcessedValuesByMarker { get; protected set; }

        public ProcessedRef AddRef(char refOperator, ProcessedValue part1, ProcessedValue part2, ProcessedValue part3)
        {
            var parts = new List<ProcessedValue>(new ProcessedValue[] { part1, part2, part3 });
            return AddRef(refOperator, parts);
        }

        public ProcessedRef AddRef(char refOperator, List<ProcessedValue> parts)
        {
            var index = ProcessedRefs.Count;
            var processedRef = new ProcessedRef(index, refOperator, parts);

            ProcessedRefs.Add(index, processedRef);
            ProcessedRefsByMarker.Add(processedRef.Marker, processedRef);
            return processedRef;
        }

        public ProcessedValue AddValue(DeciaDataType dataType, string textValue)
        {
            var index = ProcessedValues.Count;
            var processedValue = new ProcessedValue(index, dataType, textValue);

            ProcessedValues.Add(index, processedValue);
            ProcessedValuesByMarker.Add(processedValue.Marker, processedValue);
            return processedValue;
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class ProcessedRef
    {
        public ProcessedRef(int index, char refOperator, List<ProcessedValue> processedValues)
        {
            Index = index;
            Marker = string.Format(SymbolTable.MarkerFormat_ForRefs, Index);
            RefOperator = refOperator;
            ContainedValues = processedValues.ToList();
        }

        [JsonProperty]
        public int Index { get; protected set; }
        [JsonProperty]
        public string Marker { get; protected set; }
        [JsonProperty]
        public char RefOperator { get; protected set; }

        [JsonProperty]
        public bool HasStructuralTypeName { get { return (RefOperator == Argument.Reference_Full_Operator); } }
        [JsonProperty]
        public bool HasAlternateDimensionNumber { get { return (RefOperator == Argument.Reference_Full_Operator) ? (ContainedValues.Count > 2) : (ContainedValues.Count > 1); } }

        [JsonProperty]
        public string StructuralTypeName { get { return (HasStructuralTypeName) ? ContainedValues[0].TextValue : string.Empty; } }
        [JsonProperty]
        public string VariableTemplateName { get { return (HasStructuralTypeName) ? ContainedValues[1].TextValue : ContainedValues[0].TextValue; } }
        [JsonProperty]
        public string AlternateDimensionText { get { return (!HasAlternateDimensionNumber) ? string.Empty : (HasStructuralTypeName ? ContainedValues[2].TextValue : ContainedValues[1].TextValue); } }
        [JsonProperty]
        public int? AlternateDimensionNumber { get { return (!string.IsNullOrWhiteSpace(AlternateDimensionText)) ? int.Parse(AlternateDimensionText) : (int?)null; } }

        [JsonProperty]
        public List<ProcessedValue> ContainedValues { get; protected set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class ProcessedValue
    {
        public ProcessedValue(int index, DeciaDataType dataType, string textValue)
        {
            Index = index;
            Marker = string.Format(SymbolTable.MarkerFormat_ForValues, Index);
            DataType = dataType;
            TextValue = textValue;
        }

        [JsonProperty]
        public int Index { get; protected set; }
        [JsonProperty]
        public string Marker { get; protected set; }
        [JsonProperty]
        public DeciaDataType DataType { get; protected set; }
        [JsonProperty]
        public string TextValue { get; protected set; }
    }
}