using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DomainDriver.CommonUtilities.Reflectors;
using System.Reflection;

namespace Decia.Business.Common
{
    public static class PropertyManager
    {
        #region Methods - Property has Attributes

        public static bool HasAttribute<T>(this PropertyInfo propertyInfo)
            where T : Attribute
        {
            var attributes = new Type[] { typeof(T) };
            return HasAttributes(propertyInfo, attributes);
        }

        public static bool HasAttribute(this PropertyInfo propertyInfo, Type attributeType)
        {
            var attributes = new Type[] { attributeType };
            return HasAttributes(propertyInfo, attributes);
        }

        public static bool HasAttributes(this PropertyInfo propertyInfo, IEnumerable<Type> attributeTypes)
        {
            var customAttributes = propertyInfo.GetCustomAttributes(true);

            foreach (var attribute in customAttributes)
            {
                var currentAttributeType = attribute.GetType();

                if (attributeTypes.Contains(currentAttributeType))
                { return true; }
            }
            return false;
        }

        #endregion

        #region Methods - GetPropertyDisplayData for PropertyInfo

        public static PropertyDisplayDataAttribute GetPropertyDisplayData(this PropertyInfo propertyInfo)
        {
            var customAttributes = propertyInfo.GetCustomAttributes(true);
            var propertyDisplayData = customAttributes.Where(x => (x is PropertyDisplayDataAttribute)).Select(x => (x as PropertyDisplayDataAttribute)).FirstOrDefault();

            if (propertyDisplayData == null)
            { return propertyDisplayData; }

            if (propertyDisplayData.PropertyInfo == null)
            { propertyDisplayData.SetPropertyInfo(propertyInfo); }
            return propertyDisplayData;
        }

        #endregion

        #region Methods - GetPropertyDisplayData for Type

        public static PropertyDisplayDataAttribute GetPropertyDisplayData<TReturn>(this Type type, PropertyGetterDelegate<TReturn> propertyGetter)
        {
            string propertyName = ClassReflector.GetNameForProperty(propertyGetter);
            return GetPropertyDisplayData(type, propertyName);
        }

        public static PropertyDisplayDataAttribute GetPropertyDisplayData<T, TReturn>(this Type type, Expression<Func<T, TReturn>> propertyGetter)
        {
            string propertyName = ClassReflector.GetPropertyName(propertyGetter);
            return GetPropertyDisplayData(type, propertyName);
        }

        public static PropertyDisplayDataAttribute GetPropertyDisplayData<TReturn>(this Type type, Expression<Func<TReturn>> propertyGetter)
        {
            string propertyName = ClassReflector.GetPropertyName(propertyGetter);
            return GetPropertyDisplayData(type, propertyName);
        }

        public static PropertyDisplayDataAttribute GetPropertyDisplayData<TReturn>(this Type type, Expression propertyGetter)
        {
            string propertyName = ClassReflector.GetPropertyNameForExpression(propertyGetter);
            return GetPropertyDisplayData(type, propertyName);
        }

        public static PropertyDisplayDataAttribute GetPropertyDisplayData(this Type type, string propertyName)
        {
            var propertyInfo = type.GetProperty(propertyName);
            return GetPropertyDisplayData(propertyInfo);
        }

        #endregion

        #region Methods - GetPropertyDisplayData for Object

        public static PropertyDisplayDataAttribute GetPropertyDisplayData<TReturn>(this object obj, PropertyGetterDelegate<TReturn> propertyGetter)
        {
            string propertyName = ClassReflector.GetNameForProperty(propertyGetter);
            return GetPropertyDisplayData(obj, propertyName);
        }

        public static PropertyDisplayDataAttribute GetPropertyDisplayData<T, TReturn>(this object obj, Expression<Func<T, TReturn>> propertyGetter)
        {
            string propertyName = ClassReflector.GetPropertyName(propertyGetter);
            return GetPropertyDisplayData(obj, propertyName);
        }

        public static PropertyDisplayDataAttribute GetPropertyDisplayData<TReturn>(this object obj, Expression<Func<TReturn>> propertyGetter)
        {
            string propertyName = ClassReflector.GetPropertyName(propertyGetter);
            return GetPropertyDisplayData(obj, propertyName);
        }

        public static PropertyDisplayDataAttribute GetPropertyDisplayData<TReturn>(this object obj, Expression propertyGetter)
        {
            string propertyName = ClassReflector.GetPropertyNameForExpression(propertyGetter);
            return GetPropertyDisplayData(obj, propertyName);
        }

        public static PropertyDisplayDataAttribute GetPropertyDisplayData(this object obj, string propertyName)
        {
            var objType = obj.GetType();
            var propertyInfo = objType.GetProperty(propertyName);
            return GetPropertyDisplayData(propertyInfo);
        }

        #endregion

        #region Methods - GetAllPropertyDisplayData for Type

        public static Dictionary<string, PropertyDisplayDataAttribute> GetAllPropertyDisplayData(this Type type)
        {
            return GetAllPropertyDisplayData(type, PropertyHost_ViewType.All);
        }

        public static Dictionary<string, PropertyDisplayDataAttribute> GetAllPropertyDisplayData(this Type type, PropertyHost_ViewType allowedViewTypes)
        {
            var propertyInfos = type.GetProperties();
            var results = new Dictionary<string, PropertyDisplayDataAttribute>();

            foreach (var propertyInfo in propertyInfos)
            {
                var propertyName = propertyInfo.Name;
                var displayData = GetPropertyDisplayData(propertyInfo);

                if (displayData == null)
                { continue; }
                if ((displayData.SupportedViewTypes & allowedViewTypes) == PropertyHost_ViewType.None)
                { continue; }

                results.Add(propertyName, displayData);
            }
            return results;
        }

        #endregion

        #region Methods - GetAllPropertyDisplayData for Object

        public static Dictionary<string, PropertyDisplayDataAttribute> GetAllPropertyDisplayData(this object obj)
        {
            return GetAllPropertyDisplayData(obj, PropertyHost_ViewType.All);
        }

        public static Dictionary<string, PropertyDisplayDataAttribute> GetAllPropertyDisplayData(this object obj, PropertyHost_ViewType allowedViewTypes)
        {
            var objType = obj.GetType();
            var propertyInfos = objType.GetProperties();
            var results = new Dictionary<string, PropertyDisplayDataAttribute>();

            foreach (var propertyInfo in propertyInfos)
            {
                var propertyName = propertyInfo.Name;
                var displayData = GetPropertyDisplayData(propertyInfo);

                if (displayData == null)
                { continue; }
                if ((displayData.SupportedViewTypes & allowedViewTypes) == PropertyHost_ViewType.None)
                { continue; }

                results.Add(propertyName, displayData);
            }
            return results;
        }

        #endregion
    }
}