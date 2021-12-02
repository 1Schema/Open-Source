using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.DomainModels;
using DomainDriver.DomainModeling.Repositories;

namespace Decia.Business.Domain.Formulas
{
    public static class DomainModelExtensions
    {
        #region Methods - DomainModel Initializers

        public static void Decia_Formulas_LoadData(this IDomainModel domainModel)
        {
            // do nothing
        }

        public static void Decia_Formulas_LoadConstraints(this IDomainModel domainModel)
        {
            Formula.AddConstraints(domainModel);
        }

        #endregion

        #region Methods - Repository Getters

        public static IRepository<FormulaId, Formula> Formulas(this IDomainModel domainModel)
        {
            return domainModel.GetRepository<FormulaId, Formula>();
        }

        #endregion
    }
}