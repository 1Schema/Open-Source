using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Testing
{
    public class ExecutionInfo
    {
        public ExecutionInfo(Type classType, string propertyName)
        {
            ClassType = classType;
            PropertyName = propertyName;
            ExecutionCount = 0;
            PropertyValues = new List<object>();
        }

        public Type ClassType { get; protected set; }
        public string ClassName { get { return ClassType.Name; } }
        public string PropertyName { get; protected set; }
        public int ExecutionCount { get; protected set; }
        public List<object> PropertyValues { get; protected set; }

        public void IncrementExecutionCount(object propertyValue)
        {
            ExecutionCount++;
            PropertyValues.Add(propertyValue);
        }
    }
}