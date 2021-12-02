using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using DomainDriver.CommonUtilities.Reflectors;
using Decia.Business.Common.JsonSets;

namespace Decia.Business.Common.NoSql.Base
{
    public static class DeciaBaseUtils
    {
        public const string MyDb_Name = "MyDb";
        public const string SysDb_Name = "SysDb";
        public const string SysDb_Value = "local";

        public static readonly string Config_OpLogListener_Start = DeciaBase_Resources.Config_OpLogListener_Start;

        public static string[] Clctn_Decia_Names { get { return new string[] { Clctn_Decia_Metadatas_Name, Clctn_Decia_TimeDimensionSettings_Name, Clctn_Decia_DataTypes_Name, Clctn_Decia_ObjectTypes_Name, Clctn_Decia_StructuralTypes_Name, Clctn_Decia_TimePeriodTypes_Name, Clctn_Decia_TimePeriods_Name, Clctn_Decia_VariableTemplates_Name, Clctn_Decia_VariableTemplateGroups_Name, Clctn_Decia_PropagableChanges_Name, Clctn_Decia_ResultSets_Name }; } }
        public static string[] Clctn_Decia_Names_Minimal { get { return new string[] { Clctn_Decia_PropagableChanges_Name }; } }
        public static string[] Clctn_Decia_Names_None { get { return new string[] { }; } }
        public static readonly string Clctn_Decia_DataTypes_Name = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.Decia_DataTypes);
        public static readonly string Clctn_Decia_Metadatas_Name = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.Decia_Metadatas);
        public static readonly string Clctn_Decia_ObjectTypes_Name = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.Decia_ObjectTypes);
        public static readonly string Clctn_Decia_PropagableChanges_Name = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.Decia_PropagableChanges);
        public static readonly string Clctn_Decia_ResultSets_Name = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.Decia_ResultSets);
        public static readonly string Clctn_Decia_StructuralTypes_Name = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.Decia_StructuralTypes);
        public static readonly string Clctn_Decia_TimeDimensionSettings_Name = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.Decia_TimeDimensionSettings);
        public static readonly string Clctn_Decia_TimePeriods_Name = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.Decia_TimePeriods);
        public static readonly string Clctn_Decia_TimePeriodTypes_Name = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.Decia_TimePeriodTypes);
        public static readonly string Clctn_Decia_VariableTemplateGroups_Name = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.Decia_VariableTemplateGroups);
        public static readonly string Clctn_Decia_VariableTemplates_Name = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.Decia_VariableTemplates);

        public static string Fnctn_Decia_Format_Specifier = "_Format";
        public static string[] Fnctn_Decia_Format_Placeholders { get { return new string[] { Fnctn_Decia_Format_Placeholder_MyDbName, Fnctn_Decia_Format_Placeholder_SysDbName, Fnctn_Decia_Format_Placeholder_DateOfExportName, Fnctn_Decia_Format_Placeholder_RelevantCollectionNames, Fnctn_Decia_Format_Placeholder_RelevantActionNames, Fnctn_Decia_Format_Placeholder_MaxConnectionUtilization, Fnctn_Decia_Format_Placeholder_MaxQueueSize, Fnctn_Decia_Format_Placeholder_MaxActiveSize, Fnctn_Decia_Format_Placeholder_MaxCursorCount, Fnctn_Decia_Format_Placeholder_MaxHandlingErrorCount, Fnctn_Decia_Format_Placeholder_MaxCachingDepth, Fnctn_Decia_Format_Placeholder_CollectionChangeHandlerName_Prefix, Fnctn_Decia_Format_Placeholder_CollectionChangeHandlerName_Postfix }; } }
        public static string Fnctn_Decia_Format_Placeholder_MyDbName = "<MY_DB_NAME>";
        public static string Fnctn_Decia_Format_Placeholder_SysDbName = "<SYS_DB_NAME>";
        public static string Fnctn_Decia_Format_Placeholder_DateOfExportName = "<DATE_OF_EXPORT>";
        public static string Fnctn_Decia_Format_Placeholder_RelevantCollectionNames = "<REL_COL_NAMES>";
        public static string Fnctn_Decia_Format_Placeholder_RelevantActionNames = "<REL_ACT_NAMES>";
        public static string Fnctn_Decia_Format_Placeholder_MaxConnectionUtilization = "<MAX_CONNECTION_UTILIZATION>";
        public static string Fnctn_Decia_Format_Placeholder_MaxQueueSize = "<MAX_QUEUE_SIZE>";
        public static string Fnctn_Decia_Format_Placeholder_MaxActiveSize = "<MAX_ACTIVE_SIZE>";
        public static string Fnctn_Decia_Format_Placeholder_MaxCursorCount = "<MAX_CURSOR_COUNT>";
        public static string Fnctn_Decia_Format_Placeholder_MaxHandlingErrorCount = "<MAX_HANDL_ERROR_COUNT>";
        public static string Fnctn_Decia_Format_Placeholder_MaxCachingDepth = "<MAX_CACHING_DEPTH>";
        public static string Fnctn_Decia_Format_Placeholder_CollectionChangeHandlerName_Prefix = "<COL_CHANGE_HNDLR_NAME_PREFIX>";
        public static string Fnctn_Decia_Format_Placeholder_CollectionChangeHandlerName_Postfix = "<COL_CHANGE_HNDLR_NAME_POSTFIX>";

        public static bool Fnctn_Decia_All_IncludeTests = true;
        public static string[] Fnctn_Decia_All_Names
        {
            get
            {
                var allNames_WithoutTest = Fnctn_Decia_Utility_Names.Union(Fnctn_Decia_ChangeState_Names).Union(Fnctn_Decia_Time_Names).Union(Fnctn_Decia_Consistency_Names).ToArray();
                if (!Fnctn_Decia_All_IncludeTests)
                { return allNames_WithoutTest; }
                return allNames_WithoutTest.Union(Fnctn_Decia_System_TestNames).Union(Fnctn_Decia_Utility_TestNames).ToArray();
            }
        }
        public static string[] Fnctn_Decia_All_Names_Minimal
        {
            get
            {
                var allNames_WithoutTest = Fnctn_Decia_Utility_Names.Union(Fnctn_Decia_Consistency_Names).ToArray();
                if (!Fnctn_Decia_All_IncludeTests)
                { return allNames_WithoutTest; }
                return allNames_WithoutTest.Union(Fnctn_Decia_System_TestNames).Union(Fnctn_Decia_Utility_TestNames).ToArray();
            }
        }
        public static string[] Fnctn_Decia_All_Names_None
        {
            get { return new string[] { }; }
        }

        public static string[] Fnctn_Decia_Utility_Names { get { return new string[] { Fnctn_Decia_Utility_IsNull_Name, Fnctn_Decia_Utility_IsNotNull_Name, Fnctn_Decia_Utility_HasProperty_Name, Fnctn_Decia_Utility_IsArray_Name, Fnctn_Decia_Utility_GetValueAsArray_Name, Fnctn_Decia_Utility_CopyJsonObject_Name, Fnctn_Decia_Utility_AreJsonObjectsEqual_Name, Fnctn_Decia_Utility_ContainsObjectId_Name, Fnctn_Decia_Utility_FindNestedJsonObjectForId_Name, Fnctn_Decia_Utility_GenerateObjectStateValue_Name, Fnctn_Decia_Utility_UpdateCachedReferences_Name, Fnctn_Decia_Utility_GetValueAsDate_Name, Fnctn_Decia_Utility_ConvertNumberToString_Name, Fnctn_Decia_Utility_ConvertDateToString_Name }; } }
        public static readonly string Fnctn_Decia_Utility_AreJsonObjectsEqual_Name = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.fnDecia_Utility_AreJsonObjectsEqual);
        public static readonly string Fnctn_Decia_Utility_ContainsObjectId_Name = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.fnDecia_Utility_ContainsObjectId);
        public static readonly string Fnctn_Decia_Utility_ConvertDateToString_Name = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.fnDecia_Utility_ConvertDateToString);
        public static readonly string Fnctn_Decia_Utility_ConvertNumberToString_Name = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.fnDecia_Utility_ConvertNumberToString);
        public static readonly string Fnctn_Decia_Utility_CopyJsonObject_Name = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.fnDecia_Utility_CopyJsonObject);
        public static readonly string Fnctn_Decia_Utility_FindNestedJsonObjectForId_Name = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.fnDecia_Utility_FindNestedJsonObjectForId);
        public static readonly string Fnctn_Decia_Utility_GenerateObjectStateValue_Name = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.fnDecia_Utility_GenerateObjectStateValue);
        public static readonly string Fnctn_Decia_Utility_GetValueAsArray_Name = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.fnDecia_Utility_GetValueAsArray);
        public static readonly string Fnctn_Decia_Utility_GetValueAsDate_Name = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.fnDecia_Utility_GetValueAsDate);
        public static readonly string Fnctn_Decia_Utility_HasProperty_Name = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.fnDecia_Utility_HasProperty);
        public static readonly string Fnctn_Decia_Utility_IsArray_Name = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.fnDecia_Utility_IsArray);
        public static readonly string Fnctn_Decia_Utility_IsNotNull_Name = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.fnDecia_Utility_IsNotNull);
        public static readonly string Fnctn_Decia_Utility_IsNull_Name = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.fnDecia_Utility_IsNull);
        public static readonly string Fnctn_Decia_Utility_UpdateCachedReferences_Name = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.fnDecia_Utility_UpdateCachedReferences);

        public static string[] Fnctn_Decia_System_TestNames { get { return new string[] { Fnctn_Decia_System_OpLog_TestName }; } }
        public static readonly string Fnctn_Decia_System_OpLog_TestName = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.fnDecia_System_OpLog_Tests);

        public static string[] Fnctn_Decia_Utility_TestNames { get { return new string[] { Fnctn_Decia_Utility_IsNotNull_TestName, Fnctn_Decia_Utility_IsNull_TestName, Fnctn_Decia_Utility_HasProperty_TestName, Fnctn_Decia_Utility_IsArray_TestName, Fnctn_Decia_Utility_AreJsonObjectsEqual_TestName, Fnctn_Decia_Utility_CopyJsonObject_TestName, Fnctn_Decia_Utility_FindNestedJsonObjectForId_TestName, Fnctn_Decia_Utility_GenerateObjectStateValue_TestName, Fnctn_Decia_Utility_UpdateCachedReferences_TestName, Fnctn_Decia_Utility_TestRunner_TestName }; } }
        public static readonly string Fnctn_Decia_Utility_AreJsonObjectsEqual_TestName = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.fnDecia_Utility_AreJsonObjectsEqual_Tests);
        public static readonly string Fnctn_Decia_Utility_CopyJsonObject_TestName = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.fnDecia_Utility_CopyJsonObject_Tests);
        public static readonly string Fnctn_Decia_Utility_FindNestedJsonObjectForId_TestName = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.fnDecia_Utility_FindNestedJsonObjectForId_Tests);
        public static readonly string Fnctn_Decia_Utility_GenerateObjectStateValue_TestName = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.fnDecia_Utility_GenerateObjectStateValue_Tests);
        public static readonly string Fnctn_Decia_Utility_HasProperty_TestName = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.fnDecia_Utility_HasProperty_Tests);
        public static readonly string Fnctn_Decia_Utility_IsArray_TestName = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.fnDecia_Utility_IsArray_Tests);
        public static readonly string Fnctn_Decia_Utility_IsNotNull_TestName = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.fnDecia_Utility_IsNotNull_Tests);
        public static readonly string Fnctn_Decia_Utility_IsNull_TestName = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.fnDecia_Utility_IsNull_Tests);
        public static readonly string Fnctn_Decia_Utility_TestRunner_TestName = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.fnDecia_Utility_TestRunner);
        public static readonly string Fnctn_Decia_Utility_UpdateCachedReferences_TestName = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.fnDecia_Utility_UpdateCachedReferences_Tests);

        public static string[] Fnctn_Decia_ChangeState_Names { get { return new string[] { Fnctn_Decia_ChangeState_GetLatest_Name, Fnctn_Decia_ChangeState_ResetLatest_Name, Fnctn_Decia_ChangeState_IncrementLatest_Name }; } }
        public static readonly string Fnctn_Decia_ChangeState_GetLatest_Name = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.fnDecia_ChangeState_GetLatest_Format);
        public static readonly string Fnctn_Decia_ChangeState_IncrementLatest_Name = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.fnDecia_ChangeState_IncrementLatest_Format);
        public static readonly string Fnctn_Decia_ChangeState_ResetLatest_Name = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.fnDecia_ChangeState_ResetLatest_Format);

        public static string[] Fnctn_Decia_Time_Names { get { return new string[] { Fnctn_Decia_Time_GetTimePeriodDatesForId_Name, Fnctn_Decia_Time_GetTimePeriodIdForDates_Name, Fnctn_Decia_Time_GetNextTimePeriodStartDate_Name, Fnctn_Decia_Time_SetTimeDimensionBounds_Name }; } }
        public static readonly string Fnctn_Decia_Time_GetTimePeriodDatesForId_Name = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.fnDecia_Time_GetTimePeriodDatesForId);
        public static readonly string Fnctn_Decia_Time_GetTimePeriodIdForDates_Name = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.fnDecia_Time_GetTimePeriodIdForDates);
        public static readonly string Fnctn_Decia_Time_GetNextTimePeriodStartDate_Name = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.fnDecia_Time_GetNextTimePeriodStartDate_Format);
        public static readonly string Fnctn_Decia_Time_SetTimeDimensionBounds_Name = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.fnDecia_Time_SetTimeDimensionBounds_Format);

        public static string[] Fnctn_Decia_Consistency_Names { get { return new string[] { Fnctn_Decia_Consistency_CanEnqueueChanges_Name, Fnctn_Decia_Consistency_CanPropagateChanges_Name, Fnctn_Decia_Consistency_EnqueueChanges_Name, Fnctn_Decia_Consistency_PropagateChanges_Name, Fnctn_Decia_Consistency_OplogChangeListener_Name }; } }
        public static readonly string Fnctn_Decia_Consistency_CanEnqueueChanges_Name = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.fnDecia_Consistency_CanEnqueueChanges_Format);
        public static readonly string Fnctn_Decia_Consistency_CanPropagateChanges_Name = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.fnDecia_Consistency_CanPropagateChanges_Format);
        public static readonly string Fnctn_Decia_Consistency_EnqueueChanges_Name = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.fnDecia_Consistency_EnqueueChanges_Format);
        public static readonly string Fnctn_Decia_Consistency_PropagateChanges_Name = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.fnDecia_Consistency_PropagateChanges_Format);
        public static readonly string Fnctn_Decia_Consistency_OplogChangeListener_Name = ClassReflector.GetPropertyName<string>(() => DeciaBase_Resources.fnDecia_Consistency_OplogChangeListener_Format);

        public static string GetAllInserts_AsJsText(this JsonDocument documentType)
        {
            return GetAllInserts_AsJsText(documentType, null);
        }

        public static string GetAllInserts_AsJsText(this JsonDocument documentType, GenericCollection collection)
        {
            var instances = documentType.Instances.ToList();

            if (instances.Count < 1)
            { return string.Empty; }

            var insertText = ExportRows(documentType, instances);

            if (collection == null)
            { return insertText; }

            return insertText;
        }

        private static string ExportRows(JsonDocument documentType, IEnumerable<JObject> instances)
        {
            var insertText = new StringBuilder();

            insertText.AppendLine(string.Format("var insertionClct = MyDb.getCollection(\"{0}\");", documentType.CollectionName));

            foreach (var instance in instances)
            {
                insertText.AppendLine(string.Format("insertionClct.save({0});", instance.ToString()));
            }

            return insertText.ToString();
        }
    }
}