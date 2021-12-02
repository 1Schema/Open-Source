using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Testing
{
    public class ExecutionInfoRecorder
    {
        #region Static Members

        public const string Property_Getter_Postfix = ".Get";
        public const string Property_Setter_Postfix = ".Set";

        static ExecutionInfoRecorder()
        {
            ResetCurrentRecorder();
        }

        public static ExecutionInfoRecorder CurrentRecorder { get; protected set; }

        public static void ResetCurrentRecorder()
        {
            CurrentRecorder = new ExecutionInfoRecorder();
        }

        #endregion

        #region Members

        private Dictionary<string, Dictionary<string, ExecutionInfo>> m_RecordedExecutionInfo;

        #endregion

        #region Constructors

        public ExecutionInfoRecorder()
        {
            m_RecordedExecutionInfo = new Dictionary<string, Dictionary<string, ExecutionInfo>>();
        }

        #endregion

        #region Properties

        public Dictionary<string, Dictionary<string, ExecutionInfo>> AllRecords
        {
            get { return m_RecordedExecutionInfo.ToDictionary(x => x.Key, x => x.Value.ToDictionary(y => y.Key, y => y.Value)); }
        }

        #endregion

        #region Methods

        public bool HasRecords()
        {
            if (!DeciaTestState.IsTesting)
            { throw new InvalidOperationException("The DeciaTestState is not set to \"Testing\"."); }

            return (m_RecordedExecutionInfo.Count > 0);
        }

        public bool HasRecords(Type classType)
        {
            if (!DeciaTestState.IsTesting)
            { throw new InvalidOperationException("The DeciaTestState is not set to \"Testing\"."); }
            if (classType == null)
            { throw new InvalidOperationException("The specified Class Type is null."); }

            var fakeExecutionInfo = new ExecutionInfo(classType, string.Empty);

            if (!m_RecordedExecutionInfo.ContainsKey(fakeExecutionInfo.ClassName))
            { return false; }
            return (m_RecordedExecutionInfo[fakeExecutionInfo.ClassName].Count > 0);
        }

        public bool HasRecord(Type classType, string propertyName)
        {
            var record = GetRecord(classType, propertyName);
            return (record != null);
        }

        public ExecutionInfo GetRecord(Type classType, string propertyName)
        {
            if (!DeciaTestState.IsTesting)
            { throw new InvalidOperationException("The DeciaTestState is not set to \"Testing\"."); }
            if (classType == null)
            { throw new InvalidOperationException("The specified Class Type is null."); }
            if (string.IsNullOrWhiteSpace(propertyName))
            { throw new InvalidOperationException("The specified Property Name is null."); }

            var fakeExecutionInfo = new ExecutionInfo(classType, propertyName);

            if (!m_RecordedExecutionInfo.ContainsKey(fakeExecutionInfo.ClassName))
            { return null; }

            if (m_RecordedExecutionInfo[fakeExecutionInfo.ClassName].ContainsKey(fakeExecutionInfo.PropertyName))
            { return m_RecordedExecutionInfo[fakeExecutionInfo.ClassName][fakeExecutionInfo.PropertyName]; }
            if (m_RecordedExecutionInfo[fakeExecutionInfo.ClassName].ContainsKey(fakeExecutionInfo.PropertyName + Property_Getter_Postfix))
            { return m_RecordedExecutionInfo[fakeExecutionInfo.ClassName][fakeExecutionInfo.PropertyName + Property_Getter_Postfix]; }
            if (m_RecordedExecutionInfo[fakeExecutionInfo.ClassName].ContainsKey(fakeExecutionInfo.PropertyName + Property_Setter_Postfix))
            { return m_RecordedExecutionInfo[fakeExecutionInfo.ClassName][fakeExecutionInfo.PropertyName + Property_Setter_Postfix]; }

            return null;
        }

        public void RecordExecution(object propertyValue)
        {
            if (!DeciaTestState.IsTesting)
            { return; }

            var stackTrace = new StackTrace();
            var executionFrame = stackTrace.GetFrame(1);
            var classType = executionFrame.GetMethod().ReflectedType;
            var propertyName = executionFrame.GetMethod().Name;

            if (propertyName.Contains("get_"))
            {
                propertyName = propertyName.Replace("get_", string.Empty);
                propertyName += Property_Getter_Postfix;
            }
            else if (propertyName.Contains("set_"))
            {
                propertyName = propertyName.Replace("set_", string.Empty);
                propertyName += Property_Setter_Postfix;
            }

            var newExecutionInfo = new ExecutionInfo(classType, propertyName);

            if (!m_RecordedExecutionInfo.ContainsKey(newExecutionInfo.ClassName))
            { m_RecordedExecutionInfo.Add(newExecutionInfo.ClassName, new Dictionary<string, ExecutionInfo>()); }

            if (!m_RecordedExecutionInfo[newExecutionInfo.ClassName].ContainsKey(newExecutionInfo.PropertyName))
            { m_RecordedExecutionInfo[newExecutionInfo.ClassName].Add(newExecutionInfo.PropertyName, newExecutionInfo); }

            m_RecordedExecutionInfo[newExecutionInfo.ClassName][newExecutionInfo.PropertyName].IncrementExecutionCount(propertyValue);
        }

        #endregion
    }
}