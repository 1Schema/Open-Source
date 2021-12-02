using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using DomainDriver.CommonUtilities.Reflectors;

namespace Decia.Business.Common
{
    public static class ClassManager
    {
        #region Methods - Property has Attributes

        public static bool HasAttribute<T>(this Type classType)
            where T : Attribute
        {
            var attributes = new Type[] { typeof(T) };
            return HasAttributes(classType, attributes);
        }

        public static bool HasAttribute(this Type classType, Type attributeType)
        {
            var attributes = new Type[] { attributeType };
            return HasAttributes(classType, attributes);
        }

        public static bool HasAttributes(this Type classType, IEnumerable<Type> attributeTypes)
        {
            var customAttributes = classType.GetCustomAttributes(true);

            foreach (var attribute in customAttributes)
            {
                var currentAttributeType = attribute.GetType();

                if (attributeTypes.Contains(currentAttributeType))
                { return true; }
            }
            return false;
        }

        #endregion

        #region Methods - GetClassDisplayData for Type

        public static ClassDisplayDataAttribute GetClassDisplayData(this Type classType)
        {
            return GetClassDisplayData(classType, false);
        }

        public static ClassDisplayDataAttribute GetClassDisplayDataOrDefault(this Type classType)
        {
            return GetClassDisplayData(classType, true);
        }

        private static ClassDisplayDataAttribute GetClassDisplayData(this Type classType, bool useDefaultIfNull)
        {
            var customAttributes = classType.GetCustomAttributes(true);
            var classDisplayData = customAttributes.Where(x => (x is ClassDisplayDataAttribute)).Select(x => (x as ClassDisplayDataAttribute)).FirstOrDefault();

            if (classDisplayData == null)
            {
                if (useDefaultIfNull)
                { classDisplayData = new ClassDisplayDataAttribute(); }
                else
                { return classDisplayData; }
            }

            if (classDisplayData.ClassType == null)
            { classDisplayData.SetClassType(classType); }
            return classDisplayData;
        }

        #endregion

        #region Methods - GetClassDisplayData for Object

        public static ClassDisplayDataAttribute GetClassDisplayData(this object obj)
        {
            var classType = obj.GetType();
            return GetClassDisplayData(classType);
        }

        public static ClassDisplayDataAttribute GetClassDisplayDataOrDefault(this object obj)
        {
            var classType = obj.GetType();
            return GetClassDisplayDataOrDefault(classType);
        }

        #endregion
    }
}