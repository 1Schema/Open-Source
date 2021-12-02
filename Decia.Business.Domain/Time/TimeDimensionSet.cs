using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Time;

namespace Decia.Business.Domain.Time
{
    public class TimeDimensionSet : ITimeDimensionSet
    {
        #region Static Members

        public const bool Default_ThrowExceptionOnError = TimeDimensionTypeUtils.Default_ThrowExceptionOnError;
        public static readonly TimeDimensionSet EmptyTimeDimensionSet = new TimeDimensionSet(TimeDimension.EmptyPrimaryTimeDimension, TimeDimension.EmptySecondaryTimeDimension);

        #endregion

        #region Members

        protected ITimeDimension m_PrimaryTimeDimension;
        protected ITimeDimension m_SecondaryTimeDimension;

        #endregion

        #region Constructors

        public TimeDimensionSet(ITimeDimension primaryTimeDimension, ITimeDimension secondaryTimeDimension)
        {
            if (primaryTimeDimension == null)
            { primaryTimeDimension = new TimeDimension(TimeDimensionType.Primary); }
            if (secondaryTimeDimension == null)
            { secondaryTimeDimension = new TimeDimension(TimeDimensionType.Secondary); }

            if (primaryTimeDimension.TimeDimensionType != TimeDimensionType.Primary)
            { throw new InvalidOperationException("Primary Time Dimension must be of type \"Primary\"."); }
            if (secondaryTimeDimension.TimeDimensionType != TimeDimensionType.Secondary)
            { throw new InvalidOperationException("Secondary Time Dimension must be of type \"Secondary\"."); }

            m_PrimaryTimeDimension = primaryTimeDimension;
            m_SecondaryTimeDimension = secondaryTimeDimension;
        }

        #endregion

        #region Properties

        public ITimeDimension PrimaryTimeDimension
        {
            get { return m_PrimaryTimeDimension; }
        }

        public ITimeDimension SecondaryTimeDimension
        {
            get { return m_SecondaryTimeDimension; }
        }

        public int MinimumDimensionNumber
        {
            get { return TimeDimensionTypeUtils.MinimumTimeDimensionNumber; }
        }

        public int MaximumDimensionNumber
        {
            get { return TimeDimensionTypeUtils.MaximumTimeDimensionNumber; }
        }

        public int UsedDimensionCount
        {
            get
            {
                var primaryContribution = m_PrimaryTimeDimension.HasTimeValue ? 1 : 0;
                var secondaryContribution = m_SecondaryTimeDimension.HasTimeValue ? 1 : 0;
                return (primaryContribution + secondaryContribution);
            }
        }

        #endregion

        #region Methods

        public ITimeDimension GetTimeDimension(int dimensionNumber)
        {
            return GetTimeDimension(dimensionNumber, Default_ThrowExceptionOnError);
        }

        public ITimeDimension GetTimeDimension(int dimensionNumber, bool throwExceptionOnError)
        {
            var timeDimensionType = dimensionNumber.GetTimeDimensionTypeForNumber(throwExceptionOnError);
            return GetTimeDimension(timeDimensionType, throwExceptionOnError);
        }

        public ITimeDimension GetTimeDimension(TimeDimensionType dimensionType)
        {
            return GetTimeDimension(dimensionType, Default_ThrowExceptionOnError);
        }

        public ITimeDimension GetTimeDimension(TimeDimensionType dimensionType, bool throwExceptionOnError)
        {
            if (m_PrimaryTimeDimension.TimeDimensionType == dimensionType)
            { return m_PrimaryTimeDimension; }
            if (m_SecondaryTimeDimension.TimeDimensionType == dimensionType)
            { return m_SecondaryTimeDimension; }

            if (throwExceptionOnError)
            { throw new InvalidOperationException("The requested Time Dimension Type is invalid."); }
            else
            { return null; }
        }

        public TimeComparisonResult CompareTimeTo(ITimeDimensionSet other)
        {
            return ITimeDimensionSetUtils.CompareTimeTo(this, other);
        }

        #endregion

        #region Overrides & Overloads

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }
            if (!(obj is ITimeDimensionSet))
            { return false; }

            var objAsTimeDimensionSet = (ITimeDimensionSet)obj;

            return ITimeDimensionSetUtils.Equals(this, objAsTimeDimensionSet);
        }

        #endregion
    }
}