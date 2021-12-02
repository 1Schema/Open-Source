using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Collections;
using DomainDriver.CommonUtilities.Reflectors;
using DomainDriver.DomainModeling.Constraints;
using DomainDriver.DomainModeling.DataProviders;
using DomainDriver.DomainModeling.DomainModels;
using Decia.Business.Common;
using Decia.Business.Common.Conversion;

namespace Decia.Business.Domain.Formulas
{
    public partial class Formula
    {
        #region Static Members - SQL Object Creation

        public static readonly string SP_ReadCurrent_Name = ClassReflector.GetPropertyName<string>(() => SqlResources.Decia_Formulas_ReadCurrent);
        public const string SP_ReadCurrent_Parameter0_Name = "projectGuid";
        public const string SP_ReadCurrent_Parameter1_Name = "revisionNumber";
        public const string SP_ReadCurrent_Parameter2_Name = "formulaGuidsAsText";

        internal static bool CreateStoredProcedures = true;

        public static void CreateSqlObjects(IDomainModel domainModel)
        { CreateSqlObjects(domainModel, false); }

        public static void CreateSqlObjects(IDomainModel domainModel, bool forceCreation)
        {
            if (!CreateStoredProcedures && !forceCreation)
            { return; }
            if (domainModel.PersistenceType != DataSourcePersistenceType.Database)
            { return; }

            using (var sqlConnection = domainModel.GetConnection())
            {
                sqlConnection.CreateOrUpdateStoredProcedure(SP_ReadCurrent_Name, SqlResources.Decia_Formulas_ReadCurrent);
            }
            CreateStoredProcedures = false;
        }

        #endregion
    }
}