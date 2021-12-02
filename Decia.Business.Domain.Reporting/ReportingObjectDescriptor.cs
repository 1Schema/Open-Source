using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.DomainObjects;
using Decia.Business.Common;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Domain.Reporting
{
    public class ReportingObjectDescriptor<KEY, KEYED_DOMAIN_OBJECT> : IKeyedDomainObjectDescriptor<KEY, KEYED_DOMAIN_OBJECT>, IOrderable
        where KEYED_DOMAIN_OBJECT : class, IKeyedDomainObject<KEY, KEYED_DOMAIN_OBJECT>
    {
        #region Members

        private KEYED_DOMAIN_OBJECT m_DescribedObject;

        #endregion

        #region Constructors

        public ReportingObjectDescriptor()
        {
            m_DescribedObject = null;
        }

        #endregion

        #region IKeyedDomainObjectDescriptor<KEY, KEYED_DOMAIN_OBJECT> Implementation

        public KEYED_DOMAIN_OBJECT DescribedObject
        {
            get { return m_DescribedObject; }
            set
            {
                if (IsReadyForUse)
                { throw new InvalidOperationException("The DescribedObject can only be set once."); }

                var typeValidityCheck = GetName(value);

                m_DescribedObject = value;
            }
        }

        #endregion

        #region IOrderable Implementation

        public Nullable<long> OrderNumber
        {
            get { return GetOrderNumber(m_DescribedObject); }
        }

        public string OrderValue
        {
            get { return Name; }
        }

        #endregion

        #region Properties

        public bool IsReadyForUse
        {
            get { return (m_DescribedObject != null); }
        }

        public KEY Key
        {
            get { return m_DescribedObject.Key; }
        }

        public string Name
        {
            get { return GetName(m_DescribedObject); }
        }

        public bool IsType
        {
            get { return !IsInstance; }
        }

        public bool IsInstance
        {
            get { return ModelMemberId.IsInstance; }
        }

        public ModelMemberId ModelMemberId
        {
            get
            {
                var modelMemberId = GetModelMemberId(m_DescribedObject);
                return modelMemberId;
            }
        }

        public StructuralMemberId? StructuralMemberId
        {
            get
            {
                var structuralMemberId = GetStructuralMemberId(m_DescribedObject);
                return structuralMemberId;
            }
        }

        #endregion

        #region Methods

        protected string GetName(KEYED_DOMAIN_OBJECT describedObject)
        {
            AssertIsReadyForUse();

            if (describedObject is IReport)
            { return (describedObject as IReport).Name; }
            else if (describedObject is IReportElement)
            { return (describedObject as IReportElement).Name; }
            else
            { throw new InvalidOperationException("The DescribedObject is not of a valid type."); }
        }

        protected Nullable<long> GetOrderNumber(KEYED_DOMAIN_OBJECT describedObject)
        {
            AssertIsReadyForUse();

            if (describedObject is IReport)
            { return (describedObject as IReport).OrderNumber; }
            else if (describedObject is IReportElement)
            { return null; }
            else
            { throw new InvalidOperationException("The DescribedObject is not of a valid type."); }
        }

        protected ModelMemberId GetModelMemberId(KEYED_DOMAIN_OBJECT describedObject)
        {
            AssertIsReadyForUse();

            if (describedObject is IReport)
            { return (describedObject as IReport).Key.ModelMemberId; }
            else if (describedObject is IReportElement)
            { return (describedObject as IReportElement).Key.ModelMemberId; }
            else
            { throw new InvalidOperationException("The DescribedObject is not of a valid type."); }
        }

        protected StructuralMemberId? GetStructuralMemberId(KEYED_DOMAIN_OBJECT describedObject)
        {
            AssertIsReadyForUse();

            if (describedObject is IReport)
            {
                var report = (describedObject as IReport);
                var structuralTypeOption = report.StructuralTypeRef.ModelObjectType.GetStructuralType();
                var structuralTypeNumber = report.StructuralTypeRef.ModelObjectIdAsInt;
                return new StructuralMemberId(report.ProjectGuid, report.RevisionNumber_NonNull, report.ModelTemplateNumber, structuralTypeOption, structuralTypeNumber);
            }
            else if (describedObject is IReportElement)
            { return null; }
            else
            { throw new InvalidOperationException("The DescribedObject is not of a valid type."); }
        }

        protected void AssertIsReadyForUse()
        {
            if (!IsReadyForUse)
            { throw new InvalidOperationException("The current Descriptor does not have its DescribedObject set."); }
        }

        #endregion
    }
}