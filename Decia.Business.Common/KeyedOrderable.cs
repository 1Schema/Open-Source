using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Common
{
    public class KeyedOrderable<T> : IOrderable
        where T : struct
    {
        public KeyedOrderable(T key, long? orderNumber, string name)
        {
            Key = key;
            OrderNumber = orderNumber;
            Name = name;
        }

        public T Key { get; protected set; }
        public long? OrderNumber { get; protected set; }
        public string Name { get; protected set; }
        public object Tag { get; set; }

        string IOrderable.OrderValue { get { return Name; } }

        #region Object Method Overrides

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }

            var objAsKeyedOrderable = (obj as KeyedOrderable<T>);

            if (objAsKeyedOrderable == null)
            { return false; }

            if (!objAsKeyedOrderable.Key.Equals(this.Key))
            { return false; }
            if (objAsKeyedOrderable.OrderNumber != this.OrderNumber)
            { return false; }
            if (objAsKeyedOrderable.Name != this.Name)
            { return false; }
            return true;
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }

        #endregion
    }

    public static class KeyedOrderableUtils
    {
        public static Dictionary<string, string> ConvertToOptionsList(this IEnumerable<KeyedOrderable<ModelObjectReference>> keyedOrderables)
        {
            return ConvertToOptionsList(keyedOrderables, ConversionUtils.ItemPartSeparator);
        }

        public static Dictionary<string, string> ConvertToOptionsList(this IEnumerable<KeyedOrderable<ModelObjectReference>> keyedOrderables, char separator)
        {
            return ConvertToOptionsList_WithPostfix(keyedOrderables, new HashSet<ModelObjectReference>(), string.Empty, separator);
        }

        public static Dictionary<string, string> ConvertToOptionsList_WithPostfix(this IEnumerable<KeyedOrderable<ModelObjectReference>> keyedOrderables, IEnumerable<ModelObjectReference> refsForPostfix, string postfix)
        {
            return ConvertToOptionsList_WithPostfix(keyedOrderables, refsForPostfix, postfix, ConversionUtils.ItemPartSeparator);
        }

        public static Dictionary<string, string> ConvertToOptionsList_WithPostfix(this IEnumerable<KeyedOrderable<ModelObjectReference>> keyedOrderables, IEnumerable<ModelObjectReference> refsForPostfix, string postfix, char separator)
        {
            var options = new Dictionary<string, string>();
            foreach (var keyedOrderable in keyedOrderables.GetOrderedList())
            {
                var key = ConversionUtils.ConvertComplexIdToString(keyedOrderable.Key.ModelObjectType, keyedOrderable.Key.ModelObjectId, separator);
                var value = keyedOrderable.Name;

                if (refsForPostfix.Contains(keyedOrderable.Key))
                { value += postfix; }

                options.Add(key, value);
            }
            return options;
        }
    }
}