using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.StorageManagers.EntityFrameworkStorage;
using Decia.Business.Common.Testing;

namespace Decia.Business.Domain.CompoundUnits.BaseUnitValues
{
    public partial class BaseUnitExponentiationValue : IEfStorableObject
    {
        #region Entity Framework Mapper

        public class BaseUnitExponentiationValue_Mapper : EntityTypeConfiguration<BaseUnitExponentiationValue>
        {
            public BaseUnitExponentiationValue_Mapper()
            {
                Property(p => p.EF_ProjectGuid);
                Property(p => p.EF_RevisionNumber);
                Property(p => p.EF_CompoundUnitGuid);
                Property(p => p.EF_BaseUnitTypeNumber);
                Property(p => p.EF_IsBaseUnitTypeScalar);
                Property(p => p.EF_NumeratorExponentiation);
                Property(p => p.EF_DenominatorExponention);
            }
        }

        #endregion

        public object[] EF_CombinedId
        {
            get
            {
                object[] combined = new object[] { EF_ProjectGuid, EF_RevisionNumber, EF_CompoundUnitGuid, EF_BaseUnitTypeNumber };
                return combined;
            }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 0)]
        public Guid EF_ProjectGuid
        {
            get { return m_ParentCompoundUnitId.ProjectGuid; }
            set { m_ParentCompoundUnitId = new CompoundUnitId(value, m_ParentCompoundUnitId.RevisionNumber_NonNull, m_ParentCompoundUnitId.CompoundUnitGuid); }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 1)]
        public long EF_RevisionNumber
        {
            get { return m_ParentCompoundUnitId.RevisionNumber_NonNull; }
            set { m_ParentCompoundUnitId = new CompoundUnitId(m_ParentCompoundUnitId.ProjectGuid, value, m_ParentCompoundUnitId.CompoundUnitGuid); }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 2)]
        public Guid EF_CompoundUnitGuid
        {
            get { return m_ParentCompoundUnitId.CompoundUnitGuid; }
            set { m_ParentCompoundUnitId = new CompoundUnitId(m_ParentCompoundUnitId.ProjectGuid, m_ParentCompoundUnitId.RevisionNumber_NonNull, value); }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 3)]
        public int EF_BaseUnitTypeNumber
        {
            get { return m_BaseUnitTypeNumber; }
            set { m_BaseUnitTypeNumber = value; }
        }

        [ForceMapped]
        internal bool EF_IsBaseUnitTypeScalar
        {
            get
            {
                ExecutionInfoRecorder.CurrentRecorder.RecordExecution(m_IsBaseUnitTypeScalar);
                return m_IsBaseUnitTypeScalar;
            }
            set
            {
                m_IsBaseUnitTypeScalar = value;
                ExecutionInfoRecorder.CurrentRecorder.RecordExecution(m_IsBaseUnitTypeScalar);
            }
        }

        [ForceMapped]
        internal int EF_NumeratorExponentiation
        {
            get { return m_NumeratorExponentiation; }
            set { m_NumeratorExponentiation = value; }
        }

        [ForceMapped]
        internal int EF_DenominatorExponention
        {
            get { return m_DenominatorExponention; }
            set { m_DenominatorExponention = value; }
        }
    }
}