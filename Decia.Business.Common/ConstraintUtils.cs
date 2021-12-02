using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Reflectors;
using DomainDriver.DomainModeling.Constraints;

namespace Decia.Business.Common
{
    public static class ConstraintUtils
    {
        public static readonly string NameFormat = "{0}_{1}";

        public static string SetName(this ConstraintBase constraint)
        {
            return SetName(constraint, string.Empty);
        }

        public static string SetName(this ConstraintBase constraint, string postFix)
        {
            var constraintType = constraint.GetType();
            var constraintNameBase = ClassReflector.GetClassNameWithArgs(constraintType);
            var fullName = string.Format(NameFormat, constraintNameBase, postFix);
            constraint.Name = fullName;
            return constraint.Name;
        }
    }
}