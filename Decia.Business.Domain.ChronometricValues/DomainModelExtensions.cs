using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.DomainModels;
using DomainDriver.DomainModeling.Repositories;
using Decia.Business.Common;

namespace Decia.Business.Domain.ChronometricValues
{
    public static class DomainModelExtensions
    {
        #region Methods - DomainModel Initializers

        public static void Decia_ChronometricValues_LoadData(this IDomainModel domainModel)
        {
            // do nothing
        }

        public static void Decia_ChronometricValues_LoadConstraints(this IDomainModel domainModel)
        {
            ChronometricValue.AddConstraints(domainModel);
        }

        #endregion

        #region Methods - Repository Getters

        public static IRepository<ChronometricValueId, ChronometricValue> ChronometricValues(this IDomainModel domainModel)
        {
            return domainModel.GetRepository<ChronometricValueId, ChronometricValue>();
        }

        #endregion
    }
}