using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Conversion;

namespace Decia.Business.Common
{
    public class ClassDisplayDataAttribute : Attribute
    {
        public const string Default_DisplayName = "";

        public ClassDisplayDataAttribute()
            : this(Default_DisplayName)
        { }

        public ClassDisplayDataAttribute(string displayName)
            : base()
        {
            Id = Guid.NewGuid();

            IsVisible = true;
            DisplayName = displayName;
        }

        public Guid Id { get; protected set; }
        public Type ClassType { get; protected set; }

        public bool IsVisible { get; protected set; }
        public string DisplayName { get; protected set; }

        internal void SetClassType(Type classType)
        {
            if (ClassType != null)
            { throw new InvalidOperationException("This method should only be called on Attributes with no ClassType set."); }

            ClassType = classType;

            if (!IsVisible)
            { return; }

            if (string.IsNullOrWhiteSpace(DisplayName))
            { DisplayName = classType.GetTextForType(); }
        }
    }
}