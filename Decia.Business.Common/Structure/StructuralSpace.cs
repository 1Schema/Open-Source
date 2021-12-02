using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Structure
{
    public struct StructuralSpace
    {
        public static readonly StructuralSpace GlobalStructuralSpace = new StructuralSpace(new StructuralDimension[] { });

        private HashSet<StructuralDimension> m_Dimensions;

        public StructuralSpace(StructuralDimension dimension, Nullable<int> alternateDimensionNumber)
            : this(new StructuralDimension[] { dimension }, alternateDimensionNumber)
        { }

        public StructuralSpace(StructuralDimension dimension)
            : this(new StructuralDimension[] { dimension })
        { }

        public StructuralSpace(IEnumerable<StructuralDimension> dimensions, Nullable<int> alternateDimensionNumber)
        {
            if (alternateDimensionNumber.HasValue)
            {
                IEnumerable<int> existingDimensionNumbers = dimensions.Select(dim => dim.EntityDimensionNumber).Distinct().ToList();
                if (existingDimensionNumbers.Count() > 1)
                { throw new InvalidOperationException("Dimension Numbers can only be overridden when all Dimension Nubmers are the same."); }

                dimensions = dimensions.Select(dim => new StructuralDimension(dim, (alternateDimensionNumber.HasValue) ? alternateDimensionNumber.Value : dim.EntityDimensionNumber)).ToList();
            }

            HashSet<StructuralDimension> tempDimensions = new HashSet<StructuralDimension>();
            foreach (StructuralDimension axis in dimensions)
            {
                tempDimensions.Add(axis);
            }
            m_Dimensions = tempDimensions;
        }

        public StructuralSpace(IEnumerable<StructuralDimension> dimensions)
            : this(dimensions, null)
        { }

        public StructuralSpace(StructuralSpace space, Nullable<int> alternateDimensionNumber)
            : this(space, new StructuralDimension[] { }, alternateDimensionNumber)
        { }

        public StructuralSpace(StructuralSpace space, StructuralDimension extendedDimension, Nullable<int> alternateDimensionNumber)
            : this(space, new StructuralDimension[] { extendedDimension }, alternateDimensionNumber)
        { }

        public StructuralSpace(StructuralSpace space, StructuralDimension extendedDimension)
            : this(space, new StructuralDimension[] { extendedDimension })
        { }

        public StructuralSpace(StructuralSpace space, IEnumerable<StructuralDimension> extendedDimensions, Nullable<int> alternateDimensionNumber)
            : this(space.Dimensions.Union(extendedDimensions).ToList(), alternateDimensionNumber)
        { }

        public StructuralSpace(StructuralSpace space, IEnumerable<StructuralDimension> extendedDimensions)
            : this(space.Dimensions.Union(extendedDimensions).ToList())
        { }

        public ICollection<StructuralDimension> Dimensions
        {
            get { return m_Dimensions.ToList(); }
        }

        public IDictionary<int, StructuralDimension> DimensionsById
        {
            get { return m_Dimensions.ToDictionary(d => d.DimensionId, d => d); }
        }

        public int SpaceId
        {
            get { return this.GetHashCode(); }
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }

            Type thisType = this.GetType();
            if (!obj.GetType().Equals(thisType))
            { return false; }

            StructuralSpace otherKey = (StructuralSpace)obj;

            if (this.m_Dimensions.Count != otherKey.m_Dimensions.Count)
            { return false; }

            foreach (StructuralDimension axis in this.m_Dimensions)
            {
                if (!otherKey.m_Dimensions.Contains(axis))
                { return false; }
            }
            return true;
        }

        public override string ToString()
        {
            string value = string.Empty;
            var axes_AsOrderedStrings = m_Dimensions.Select(x => x.ToString()).OrderBy(x => x).ToList();

            foreach (var axis_AsString in axes_AsOrderedStrings)
            {
                if (string.IsNullOrWhiteSpace(value))
                { value += axis_AsString; }
                else
                { value += "; " + axis_AsString; }
            }
            return value;
        }

        public static bool operator ==(StructuralSpace a, StructuralSpace b)
        { return a.Equals(b); }

        public static bool operator !=(StructuralSpace a, StructuralSpace b)
        { return !(a == b); }

        public static StructuralSpace operator +(StructuralSpace a, StructuralSpace b)
        { return a.Merge(b); }

        public static StructuralSpace operator +(StructuralSpace a, StructuralDimension b)
        { return new StructuralSpace(a, b); }
    }
}