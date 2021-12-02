using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Common.Structure
{
    public class JoinedDimensionValue
    {
        protected IList<ModelObjectReference> m_ExtendedJoinPath;

        public JoinedDimensionValue(ModelObjectReference localRef)
        {
            LocalRef = localRef;
        }

        public JoinedDimensionValue(ModelObjectReference localRef, JoinedDimensionValue otherValue)
            : this(localRef)
        {
            Dimension = otherValue.Dimension;
            KeySourceRef = otherValue.KeySourceRef;
            m_ExtendedJoinPath = (otherValue.m_ExtendedJoinPath != null) ? otherValue.m_ExtendedJoinPath.ToList() : null;
        }

        public ModelObjectReference LocalRef { get; protected set; }
        public StructuralDimension Dimension { get; set; }
        public ModelObjectReference KeySourceRef { get; set; }
        public bool RequiresDimensionalJoin { get { return !ModelObjectReference.DimensionalComparer.Equals(LocalRef, KeySourceRef); } }
        public bool UseExistingJoinSetting { get { return ((ExtendedJoinPath == null) || (ExtendedJoinPath.Count < 1)); } }
        public IList<ModelObjectReference> ExtendedJoinPath
        {
            get
            {
                if (!RequiresDimensionalJoin)
                { return null; }
                return m_ExtendedJoinPath;
            }
            set
            { m_ExtendedJoinPath = value; }
        }
    }
}