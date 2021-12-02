using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.DomainModels;
using DomainDriver.DomainModeling.Repositories;
using Decia.Business.Common;

namespace Decia.Business.Domain.Exports
{
    public static class DomainModelExtensions
    {
        #region Methods - DomainModel Initializers

        public static void Decia_Exports_LoadConstraints(this IDomainModel domainModel)
        {
            ExportHistoryItem.AddConstraints(domainModel);
        }

        #endregion

        #region Methods - Repository Getters

        public static IRepository<ExportHistoryItemId, ExportHistoryItem> ExportHistoryItems(this IDomainModel domainModel)
        {
            return domainModel.GetRepository<ExportHistoryItemId, ExportHistoryItem>();
        }

        #endregion
    }
}