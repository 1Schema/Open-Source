using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.StorageManagers.EntityFrameworkStorage;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Testing;

namespace Decia.Business.Domain.Formulas.Expressions
{
    public partial class Expression : IEfStorableObject
    {
        #region Entity Framework Mapper

        public class Expression_Mapper : EntityTypeConfiguration<Expression>
        {
            public Expression_Mapper()
            {
                Property(p => p.EF_ProjectGuid);
                Property(p => p.EF_RevisionNumber);
                Property(p => p.EF_FormulaGuid);
                Property(p => p.EF_ExpressionGuid);
                Property(p => p.EF_OperationGuid);
                Property(p => p.EF_ShowAsOperator);
                Property(p => p.EF_OuterParenthesesCount);
            }
        }

        #endregion

        public object[] EF_CombinedId
        {
            get
            {
                object[] combined = new object[] { EF_ProjectGuid, EF_RevisionNumber, EF_FormulaGuid, EF_ExpressionGuid };
                return combined;
            }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 0)]
        public Guid EF_ProjectGuid
        {
            get { return m_Key.ProjectGuid; }
            set { m_Key = new ExpressionId(value, m_Key.RevisionNumber_NonNull, m_Key.FormulaGuid, m_Key.ExpressionGuid); }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 1)]
        public long EF_RevisionNumber
        {
            get { return m_Key.RevisionNumber_NonNull; }
            set { m_Key = new ExpressionId(m_Key.ProjectGuid, value, m_Key.FormulaGuid, m_Key.ExpressionGuid); }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 2)]
        public Guid EF_FormulaGuid
        {
            get { return m_Key.FormulaGuid; }
            set { m_Key = new ExpressionId(m_Key.ProjectGuid, m_Key.RevisionNumber_NonNull, value, m_Key.ExpressionGuid); }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 3)]
        public Guid EF_ExpressionGuid
        {
            get { return m_Key.ExpressionGuid; }
            set { m_Key = new ExpressionId(m_Key.ProjectGuid, m_Key.RevisionNumber_NonNull, m_Key.FormulaGuid, value); }
        }

        [ForceMapped]
        internal Guid EF_OperationGuid
        {
            get { return m_OperationId.OperationGuid; }
            set { m_OperationId = new OperationId(value); }
        }

        [ForceMapped]
        internal bool EF_ShowAsOperator
        {
            get
            {
                ExecutionInfoRecorder.CurrentRecorder.RecordExecution(m_ShowAsOperator);
                return m_ShowAsOperator;
            }
            set
            {
                m_ShowAsOperator = value;
                ExecutionInfoRecorder.CurrentRecorder.RecordExecution(m_ShowAsOperator);
            }
        }

        [ForceMapped]
        internal int EF_OuterParenthesesCount
        {
            get { return m_OuterParenthesesCount; }
            set { m_OuterParenthesesCount = value; }
        }

        [NotMapped]
        internal Dictionary<int, Argument> EF_Arguments
        {
            get { return m_Arguments; }
            set { m_Arguments = value; }
        }
    }
}