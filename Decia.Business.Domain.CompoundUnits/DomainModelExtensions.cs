using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.DomainModels;
using DomainDriver.DomainModeling.Repositories;

namespace Decia.Business.Domain.CompoundUnits
{
    public static class DomainModelExtensions
    {
        #region Methods - DomainModel Initializers

        public static void Decia_CompoundUnits_LoadData(this IDomainModel domainModel)
        {
            // do nothing
        }

        public static void Decia_CompoundUnits_LoadConstraints(this IDomainModel domainModel)
        {
            domainModel.AddModelConstraint(new ProjectDomainObject_ManagementConstraint<CompoundUnitId, CompoundUnit>(domainModel));
        }

        #endregion

        #region Methods - Repository Getters

        public static IRepository<CompoundUnitId, CompoundUnit> CompoundUnits(this IDomainModel domainModel)
        {
            return domainModel.GetRepository<CompoundUnitId, CompoundUnit>();
        }

        #endregion
    }
}