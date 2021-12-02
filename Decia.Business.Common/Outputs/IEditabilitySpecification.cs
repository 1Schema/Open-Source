using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Outputs
{
    public interface IEditabilitySpecification
    {
        EditMode CurrentMode { get; }
        bool IsEditable(string propertyName);
        bool IsValueValid(string propertyName, object proposedValue, IDictionary<string, object> currentValues);
    }

    public static class IEditabilitySpecificationUtils
    {
        public const EditMode DefaultEditMode = EditMode.User;
    }

    public class NoOpEditabilitySpecification : IEditabilitySpecification
    {
        public EditMode CurrentMode { get { return IEditabilitySpecificationUtils.DefaultEditMode; } }

        public bool IsEditable(string propertyName)
        { return true; }

        public bool IsValueValid(string propertyName, object proposedValue, IDictionary<string, object> currentValues)
        { return true; }
    }

    public class ReadOnlyEditabilitySpecification : IEditabilitySpecification
    {
        public EditMode CurrentMode { get { return IEditabilitySpecificationUtils.DefaultEditMode; } }

        public bool IsEditable(string propertyName)
        { return false; }

        public bool IsValueValid(string propertyName, object proposedValue, IDictionary<string, object> currentValues)
        { return false; }
    }
}