using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Decia.Business.Common.JsonSets;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Common.NoSql.Base
{
    public class DeciaBase_JsonSet : JsonSet
    {
        #region Members

        private Decia_DataType_JsonDocument m_DataType_Document;
        private Decia_Metadata_JsonDocument m_Metadata_Document;
        private Decia_ObjectType_JsonDocument m_ObjectType_Document;
        private Decia_ResultSet_JsonDocument m_ResultSet_Document;
        private Decia_StructuralType_JsonDocument m_StructuralType_Document;
        private Decia_TimeDimensionSetting_JsonDocument m_TimeDimensionSetting_Document;
        private Decia_TimePeriod_JsonDocument m_TimePeriod_Document;
        private Decia_TimePeriodType_JsonDocument m_TimePeriodType_Document;
        private Decia_VariableTemplateGroup_JsonDocument m_VariableTemplateGroup_Document;
        private Decia_VariableTemplate_JsonDocument m_VariableTemplate_Document;

        #endregion

        #region Constructors

        public DeciaBase_JsonSet()
            : base("DeciaBase_JsonSet")
        {
            m_DataType_Document = AddDocument(new Decia_DataType_JsonDocument(this));
            m_Metadata_Document = AddDocument(new Decia_Metadata_JsonDocument(this));
            m_ObjectType_Document = AddDocument(new Decia_ObjectType_JsonDocument(this));
            m_ResultSet_Document = AddDocument(new Decia_ResultSet_JsonDocument(this));
            m_StructuralType_Document = AddDocument(new Decia_StructuralType_JsonDocument(this));
            m_TimeDimensionSetting_Document = AddDocument(new Decia_TimeDimensionSetting_JsonDocument(this));
            m_TimePeriod_Document = AddDocument(new Decia_TimePeriod_JsonDocument(this));
            m_TimePeriodType_Document = AddDocument(new Decia_TimePeriodType_JsonDocument(this));
            m_VariableTemplateGroup_Document = AddDocument(new Decia_VariableTemplateGroup_JsonDocument(this));
            m_VariableTemplate_Document = AddDocument(new Decia_VariableTemplate_JsonDocument(this));
        }

        #endregion

        #region Properties

        public Decia_DataType_JsonDocument DataType_Document { get { return m_DataType_Document; } }
        public Decia_Metadata_JsonDocument Metadata_Document { get { return m_Metadata_Document; } }
        public Decia_ObjectType_JsonDocument ObjectType_Document { get { return m_ObjectType_Document; } }
        public Decia_ResultSet_JsonDocument ResultSet_Document { get { return m_ResultSet_Document; } }
        public Decia_StructuralType_JsonDocument StructuralType_Document { get { return m_StructuralType_Document; } }
        public Decia_TimeDimensionSetting_JsonDocument TimeDimensionSetting_Document { get { return m_TimeDimensionSetting_Document; } }
        public Decia_TimePeriod_JsonDocument TimePeriod_Document { get { return m_TimePeriod_Document; } }
        public Decia_TimePeriodType_JsonDocument TimePeriodType_Document { get { return m_TimePeriodType_Document; } }
        public Decia_VariableTemplateGroup_JsonDocument VariableTemplateGroup_Document { get { return m_VariableTemplateGroup_Document; } }
        public Decia_VariableTemplate_JsonDocument VariableTemplate_Document { get { return m_VariableTemplate_Document; } }

        #endregion

        #region Methods

        public Decia_JsonObject<T> CreateJsonObject<T>()
            where T : JsonDocument
        {
            var schemaDocs = this.DocumentsById.Values.Where(x => typeof(T) == x.GetType()).ToList();
            if (schemaDocs.Count != 1)
            { throw new InvalidOperationException("Unique Schema Doc could not be found."); }

            var schemaDoc = (T)schemaDocs.First();

            var jsonObject = new Decia_JsonObject<T>(schemaDoc);
            schemaDoc.AddInstance(jsonObject);

            return jsonObject;
        }

        #endregion

        #region Nested Classes

        public class Decia_JsonObject<T> : JObject
            where T : JsonDocument
        {
            public Decia_JsonObject(T schemaDoc)
                : base()
            { SchemaDoc = schemaDoc; }

            public T SchemaDoc { get; protected set; }
        }

        public class Decia_DataType_JsonDocument : JsonDocument
        {
            public const string DefaultDocumentName = "Decia_DataType";
            public const string DefaultCollectionName = "Decia_DataTypes";

            #region Members

            private JsonProperty m_Id;
            private JsonProperty m_Name;
            private JsonProperty m_Description;

            #endregion

            #region Constructors

            public Decia_DataType_JsonDocument(DeciaBase_JsonSet parentSet)
                : base(parentSet, DefaultDocumentName)
            {
                CollectionName = DefaultCollectionName;

                m_Id = new JsonProperty(this.Root, "_id", DeciaDataType.Integer);
                m_Name = new JsonProperty(this.Root, "Name", DeciaDataType.Text);
                m_Description = new JsonProperty(this.Root, "Description", DeciaDataType.Text);
            }

            #endregion

            #region Properties

            public JsonProperty Id { get { return m_Id; } }
            public JsonProperty Name { get { return m_Name; } }
            public JsonProperty Description { get { return m_Description; } }

            #endregion
        }

        public class Decia_Metadata_JsonDocument : JsonDocument
        {
            public const string DefaultDocumentName = "Decia_Metadata";
            public const string DefaultCollectionName = "Decia_Metadatas";

            #region Members

            private JsonProperty m_Id;
            private JsonProperty m_ProjectId;
            private JsonProperty m_RevisionNumber;
            private JsonProperty m_ModelTemplateId;
            private JsonProperty m_Name;
            private JsonProperty m_MongoName;
            private JsonProperty m_Description;
            private JsonProperty m_ConciseRevisionNumber;
            private JsonProperty m_Latest_ChangeCount;
            private JsonProperty m_Latest_ChangeDate;

            #endregion

            #region Constructors

            public Decia_Metadata_JsonDocument(DeciaBase_JsonSet parentSet)
                : base(parentSet, DefaultDocumentName)
            {
                CollectionName = DefaultCollectionName;

                m_Id = new JsonProperty(this.Root, "_id", DeciaDataType.Integer);
                m_ProjectId = new JsonProperty(this.Root, "ProjectId", DeciaDataType.UniqueID);
                m_RevisionNumber = new JsonProperty(this.Root, "RevisionNumber", DeciaDataType.Integer);
                m_ModelTemplateId = new JsonProperty(this.Root, "ModelTemplateId", DeciaDataType.UniqueID);
                m_Name = new JsonProperty(this.Root, "Name", DeciaDataType.Text);
                m_MongoName = new JsonProperty(this.Root, "MongoName", DeciaDataType.Text);
                m_Description = new JsonProperty(this.Root, "Description", DeciaDataType.Text);
                m_ConciseRevisionNumber = new JsonProperty(this.Root, "ConciseRevisionNumber", DeciaDataType.Integer);
                m_Latest_ChangeCount = new JsonProperty(this.Root, "Latest_ChangeCount", DeciaDataType.Integer);
                m_Latest_ChangeDate = new JsonProperty(this.Root, "Latest_ChangeDate", DeciaDataType.DateTime);
            }

            #endregion

            #region Properties

            public JsonProperty Id { get { return m_Id; } }
            public JsonProperty ProjectId { get { return m_ProjectId; } }
            public JsonProperty RevisionNumber { get { return m_RevisionNumber; } }
            public JsonProperty ModelTemplateId { get { return m_ModelTemplateId; } }
            public JsonProperty Name { get { return m_Name; } }
            public JsonProperty MongoName { get { return m_MongoName; } }
            public JsonProperty Description { get { return m_Description; } }
            public JsonProperty ConciseRevisionNumber { get { return m_ConciseRevisionNumber; } }
            public JsonProperty Latest_ChangeCount { get { return m_Latest_ChangeCount; } }
            public JsonProperty Latest_ChangeDate { get { return m_Latest_ChangeDate; } }

            #endregion
        }

        public class Decia_ObjectType_JsonDocument : JsonDocument
        {
            public const string DefaultDocumentName = "Decia_ObjectType";
            public const string DefaultCollectionName = "Decia_ObjectTypes";

            #region Members

            private JsonProperty m_Id;
            private JsonProperty m_Name;
            private JsonProperty m_Description;

            #endregion

            #region Constructors

            public Decia_ObjectType_JsonDocument(DeciaBase_JsonSet parentSet)
                : base(parentSet, DefaultDocumentName)
            {
                CollectionName = DefaultCollectionName;

                m_Id = new JsonProperty(this.Root, "_id", DeciaDataType.Integer);
                m_Name = new JsonProperty(this.Root, "Name", DeciaDataType.Text);
                m_Description = new JsonProperty(this.Root, "Description", DeciaDataType.Text);
            }

            #endregion

            #region Properties

            public JsonProperty Id { get { return m_Id; } }
            public JsonProperty Name { get { return m_Name; } }
            public JsonProperty Description { get { return m_Description; } }

            #endregion
        }

        public class Decia_ResultSet_JsonDocument : JsonDocument
        {
            public const string DefaultDocumentName = "Decia_ResultSet";
            public const string DefaultCollectionName = "Decia_ResultSets";

            #region Members

            private JsonProperty m_Id;
            private JsonProperty m_Name;
            private JsonProperty m_Description;
            private JsonProperty m_Metadata_ChangeCount;

            private JsonProperty m_TimeDimensionSetting_Object;
            private JsonProperty m_TimeDimensionSetting_Object_StartDate;
            private JsonProperty m_TimeDimensionSetting_Object_EndDate;

            private JsonProperty m_ProcessedGroups_Array;
            private JsonProperty m_ProcessedGroup_Object;
            private JsonProperty m_ProcessedGroup_Object_VariableTemplateGroupId;
            private JsonProperty m_ProcessedGroup_Object_OrderIndex;
            private JsonProperty m_ProcessedGroup_Object_Computation_Succeeded;

            #endregion

            #region Constructors

            public Decia_ResultSet_JsonDocument(DeciaBase_JsonSet parentSet)
                : base(parentSet, DefaultDocumentName)
            {
                CollectionName = DefaultCollectionName;

                m_Id = new JsonProperty(this.Root, "_id", DeciaDataType.UniqueID);
                m_Name = new JsonProperty(this.Root, "Name", DeciaDataType.Text);
                m_Description = new JsonProperty(this.Root, "Description", DeciaDataType.Text);
                m_Metadata_ChangeCount = new JsonProperty(this.Root, "Metadata_ChangeCount", DeciaDataType.Integer);

                m_TimeDimensionSetting_Object = new JsonProperty(this.Root, "TimeDimensionSetting", DeciaComplexType.Object);
                m_TimeDimensionSetting_Object_StartDate = new JsonProperty(m_TimeDimensionSetting_Object, "StartDate", DeciaDataType.DateTime);
                m_TimeDimensionSetting_Object_EndDate = new JsonProperty(m_TimeDimensionSetting_Object, "EndDate", DeciaDataType.DateTime);

                m_ProcessedGroups_Array = new JsonProperty(this.Root, "ProcessedGroups", DeciaComplexType.Array);
                m_ProcessedGroup_Object = new JsonProperty(m_ProcessedGroups_Array);
                m_ProcessedGroup_Object_VariableTemplateGroupId = new JsonProperty(m_ProcessedGroup_Object, "VariableTemplateGroupId", DeciaDataType.UniqueID);
                m_ProcessedGroup_Object_OrderIndex = new JsonProperty(m_ProcessedGroup_Object, "OrderIndex", DeciaDataType.Integer);
                m_ProcessedGroup_Object_Computation_Succeeded = new JsonProperty(m_ProcessedGroup_Object, "Computation_Succeeded", DeciaDataType.Boolean);
            }

            #endregion

            #region Properties

            public JsonProperty Id { get { return m_Id; } }
            public JsonProperty Name { get { return m_Name; } }
            public JsonProperty Description { get { return m_Description; } }
            public JsonProperty Metadata_ChangeCount { get { return m_Metadata_ChangeCount; } }

            public JsonProperty TimeDimensionSetting_Object { get { return m_TimeDimensionSetting_Object; } }
            public JsonProperty TimeDimensionSetting_Object_StartDate { get { return m_TimeDimensionSetting_Object_StartDate; } }
            public JsonProperty TimeDimensionSetting_Object_EndDate { get { return m_TimeDimensionSetting_Object_EndDate; } }

            public JsonProperty ProcessedGroups_Array { get { return m_ProcessedGroups_Array; } }
            public JsonProperty ProcessedGroup_Object { get { return m_ProcessedGroup_Object; } }
            public JsonProperty ProcessedGroup_Object_VariableTemplateGroupId { get { return m_ProcessedGroup_Object_VariableTemplateGroupId; } }
            public JsonProperty ProcessedGroup_Object_OrderIndex { get { return m_ProcessedGroup_Object_OrderIndex; } }
            public JsonProperty ProcessedGroup_Object_Computation_Succeeded { get { return m_ProcessedGroup_Object_Computation_Succeeded; } }

            #endregion
        }

        public class Decia_StructuralType_JsonDocument : JsonDocument
        {
            public const string DefaultDocumentName = "Decia_StructuralType";
            public const string DefaultCollectionName = "Decia_StructuralTypes";

            #region Members

            private JsonProperty m_Id;
            private JsonProperty m_Name;
            private JsonProperty m_MongoName;
            private JsonProperty m_Description;
            private JsonProperty m_ObjectTypeId;
            private JsonProperty m_TreeLevel_Basic;
            private JsonProperty m_TreeLevel_Extended;
            private JsonProperty m_Parent_StructuralTypeId;
            private JsonProperty m_Parent_IsNullable;
            private JsonProperty m_Parent_Default_InstanceId;
            private JsonProperty m_Instance_Collection_Name;
            private JsonProperty m_Instance_Id_VariableTemplateId;
            private JsonProperty m_Instance_Name_VariableTemplateId;
            private JsonProperty m_Instance_Sorting_VariableTemplateId;
            private JsonProperty m_Instance_ParentId_VariableTemplateId;

            #endregion

            #region Constructors

            public Decia_StructuralType_JsonDocument(DeciaBase_JsonSet parentSet)
                : base(parentSet, DefaultDocumentName)
            {
                CollectionName = DefaultCollectionName;

                m_Id = new JsonProperty(this.Root, "_id", DeciaDataType.UniqueID);
                m_Name = new JsonProperty(this.Root, "Name", DeciaDataType.Text);
                m_MongoName = new JsonProperty(this.Root, "MongoName", DeciaDataType.Text);
                m_Description = new JsonProperty(this.Root, "Description", DeciaDataType.Text);
                m_ObjectTypeId = new JsonProperty(this.Root, "ObjectTypeId", DeciaDataType.Integer);
                m_TreeLevel_Basic = new JsonProperty(this.Root, "TreeLevel_Basic", DeciaDataType.Integer);
                m_TreeLevel_Extended = new JsonProperty(this.Root, "TreeLevel_Extended", DeciaDataType.Integer);
                m_Parent_StructuralTypeId = new JsonProperty(this.Root, "Parent_StructuralTypeId", DeciaDataType.Integer);
                m_Parent_IsNullable = new JsonProperty(this.Root, "Parent_IsNullable", DeciaDataType.Boolean);
                m_Parent_Default_InstanceId = new JsonProperty(this.Root, "Parent_Default_InstanceId", DeciaDataType.UniqueID);
                m_Instance_Collection_Name = new JsonProperty(this.Root, "Instance_Collection_Name", DeciaDataType.Text);
                m_Instance_Id_VariableTemplateId = new JsonProperty(this.Root, "Instance_Id_VariableTemplateId", DeciaDataType.UniqueID);
                m_Instance_Name_VariableTemplateId = new JsonProperty(this.Root, "Instance_Name_VariableTemplateId", DeciaDataType.UniqueID);
                m_Instance_Sorting_VariableTemplateId = new JsonProperty(this.Root, "Instance_Sorting_VariableTemplateId", DeciaDataType.UniqueID);
                m_Instance_ParentId_VariableTemplateId = new JsonProperty(this.Root, "Instance_ParentId_VariableTemplateId", DeciaDataType.UniqueID);

            }

            #endregion

            #region Properties

            public JsonProperty Id { get { return m_Id; } }
            public JsonProperty Name { get { return m_Name; } }
            public JsonProperty MongoName { get { return m_MongoName; } }
            public JsonProperty Description { get { return m_Description; } }
            public JsonProperty ObjectTypeId { get { return m_ObjectTypeId; } }
            public JsonProperty TreeLevel_Basic { get { return m_TreeLevel_Basic; } }
            public JsonProperty TreeLevel_Extended { get { return m_TreeLevel_Extended; } }
            public JsonProperty Parent_StructuralTypeId { get { return m_Parent_StructuralTypeId; } }
            public JsonProperty Parent_IsNullable { get { return m_Parent_IsNullable; } }
            public JsonProperty Parent_Default_InstanceId { get { return m_Parent_Default_InstanceId; } }
            public JsonProperty Instance_Collection_Name { get { return m_Instance_Collection_Name; } }
            public JsonProperty Instance_Id_VariableTemplateId { get { return m_Instance_Id_VariableTemplateId; } }
            public JsonProperty Instance_Name_VariableTemplateId { get { return m_Instance_Name_VariableTemplateId; } }
            public JsonProperty Instance_Sorting_VariableTemplateId { get { return m_Instance_Sorting_VariableTemplateId; } }
            public JsonProperty Instance_ParentId_VariableTemplateId { get { return m_Instance_ParentId_VariableTemplateId; } }

            #endregion
        }

        public class Decia_TimeDimensionSetting_JsonDocument : JsonDocument
        {
            public const string DefaultDocumentName = "Decia_TimeDimensionSetting";
            public const string DefaultCollectionName = "Decia_TimeDimensionSettings";

            #region Members

            private JsonProperty m_Id;
            private JsonProperty m_StartDate;
            private JsonProperty m_EndDate;

            #endregion

            #region Constructors

            public Decia_TimeDimensionSetting_JsonDocument(DeciaBase_JsonSet parentSet)
                : base(parentSet, DefaultDocumentName)
            {
                CollectionName = DefaultCollectionName;

                m_Id = new JsonProperty(this.Root, "_id", DeciaDataType.Integer);
                m_StartDate = new JsonProperty(this.Root, "StartDate", DeciaDataType.DateTime);
                m_EndDate = new JsonProperty(this.Root, "EndDate", DeciaDataType.DateTime);
            }

            #endregion

            #region Properties

            public JsonProperty Id { get { return m_Id; } }
            public JsonProperty StartDate { get { return m_StartDate; } }
            public JsonProperty EndDate { get { return m_EndDate; } }

            #endregion
        }

        public class Decia_TimePeriod_JsonDocument : JsonDocument
        {
            public const string DefaultDocumentName = "Decia_TimePeriod";
            public const string DefaultCollectionName = "Decia_TimePeriods";

            #region Members

            private JsonProperty m_Id;
            private JsonProperty m_TimePeriodTypeId;
            private JsonProperty m_StartDate;
            private JsonProperty m_EndDate;
            private JsonProperty m_IsForever;

            #endregion

            #region Constructors

            public Decia_TimePeriod_JsonDocument(DeciaBase_JsonSet parentSet)
                : base(parentSet, DefaultDocumentName)
            {
                CollectionName = DefaultCollectionName;

                m_Id = new JsonProperty(this.Root, "_id", DeciaDataType.Text);
                m_TimePeriodTypeId = new JsonProperty(this.Root, "TimePeriodTypeId", DeciaDataType.Integer);
                m_StartDate = new JsonProperty(this.Root, "StartDate", DeciaDataType.DateTime);
                m_EndDate = new JsonProperty(this.Root, "EndDate", DeciaDataType.DateTime);
                m_IsForever = new JsonProperty(this.Root, "IsForever", DeciaDataType.Boolean);
            }

            #endregion

            #region Properties

            public JsonProperty Id { get { return m_Id; } }
            public JsonProperty TimePeriodTypeId { get { return m_TimePeriodTypeId; } }
            public JsonProperty StartDate { get { return m_StartDate; } }
            public JsonProperty EndDate { get { return m_EndDate; } }
            public JsonProperty IsForever { get { return m_IsForever; } }

            #endregion
        }

        public class Decia_TimePeriodType_JsonDocument : JsonDocument
        {
            public const string DefaultDocumentName = "Decia_TimePeriodType";
            public const string DefaultCollectionName = "Decia_TimePeriodTypes";

            #region Members

            private JsonProperty m_Id;
            private JsonProperty m_Name;
            private JsonProperty m_Description;
            private JsonProperty m_IsForever;
            private JsonProperty m_EstimateInDays;
            private JsonProperty m_MinValidDays;
            private JsonProperty m_MaxValidDays;
            private JsonProperty m_DatePart_Value;
            private JsonProperty m_DatePart_Multiplier;

            #endregion

            #region Constructors

            public Decia_TimePeriodType_JsonDocument(DeciaBase_JsonSet parentSet)
                : base(parentSet, DefaultDocumentName)
            {
                CollectionName = DefaultCollectionName;

                m_Id = new JsonProperty(this.Root, "_id", DeciaDataType.Integer);
                m_Name = new JsonProperty(this.Root, "Name", DeciaDataType.Text);
                m_Description = new JsonProperty(this.Root, "Description", DeciaDataType.Text);
                m_IsForever = new JsonProperty(this.Root, "IsForever", DeciaDataType.Boolean);
                m_EstimateInDays = new JsonProperty(this.Root, "EstimateInDays", DeciaDataType.Decimal);
                m_MinValidDays = new JsonProperty(this.Root, "MinValidDays", DeciaDataType.Decimal);
                m_MaxValidDays = new JsonProperty(this.Root, "MaxValidDays", DeciaDataType.Decimal);
                m_DatePart_Value = new JsonProperty(this.Root, "DatePart_Value", DeciaDataType.Text);
                m_DatePart_Multiplier = new JsonProperty(this.Root, "DatePart_Multiplier", DeciaDataType.Decimal);
            }

            #endregion

            #region Properties

            public JsonProperty Id { get { return m_Id; } }
            public JsonProperty Name { get { return m_Name; } }
            public JsonProperty Description { get { return m_Description; } }
            public JsonProperty IsForever { get { return m_IsForever; } }
            public JsonProperty EstimateInDays { get { return m_EstimateInDays; } }
            public JsonProperty MinValidDays { get { return m_MinValidDays; } }
            public JsonProperty MaxValidDays { get { return m_MaxValidDays; } }
            public JsonProperty DatePart_Value { get { return m_DatePart_Value; } }
            public JsonProperty DatePart_Multiplier { get { return m_DatePart_Multiplier; } }

            #endregion
        }

        public class Decia_VariableTemplateGroup_JsonDocument : JsonDocument
        {
            public const string DefaultDocumentName = "Decia_VariableTemplateGroup";
            public const string DefaultCollectionName = "Decia_VariableTemplateGroups";

            #region Members

            private JsonProperty m_Id;
            private JsonProperty m_ProcessingIndex;
            private JsonProperty m_HasCycles;
            private JsonProperty m_HasUnresolvableCycles;

            private JsonProperty m_Members_Array;
            private JsonProperty m_Member_Object;
            private JsonProperty m_Member_Object_VariableTemplateId;
            private JsonProperty m_Member_Object_Priority;

            #endregion

            #region Constructors

            public Decia_VariableTemplateGroup_JsonDocument(DeciaBase_JsonSet parentSet)
                : base(parentSet, DefaultDocumentName)
            {
                CollectionName = DefaultCollectionName;

                m_Id = new JsonProperty(this.Root, "_id", DeciaDataType.UniqueID);
                m_ProcessingIndex = new JsonProperty(this.Root, "ProcessingIndex", DeciaDataType.Integer);
                m_HasCycles = new JsonProperty(this.Root, "HasCycles", DeciaDataType.Boolean);
                m_HasUnresolvableCycles = new JsonProperty(this.Root, "HasUnresolvableCycles", DeciaDataType.Boolean);

                m_Members_Array = new JsonProperty(this.Root, "Members", DeciaComplexType.Array);
                m_Member_Object = new JsonProperty(m_Members_Array);
                m_Member_Object_VariableTemplateId = new JsonProperty(m_Member_Object, "VariableTemplateId", DeciaDataType.UniqueID);
                m_Member_Object_Priority = new JsonProperty(m_Member_Object, "Priority", DeciaDataType.Integer);
            }

            #endregion

            #region Properties

            public JsonProperty Id { get { return m_Id; } }
            public JsonProperty ProcessingIndex { get { return m_ProcessingIndex; } }
            public JsonProperty HasCycles { get { return m_HasCycles; } }
            public JsonProperty HasUnresolvableCycles { get { return m_HasUnresolvableCycles; } }

            public JsonProperty Members_Array { get { return m_Members_Array; } }
            public JsonProperty Member_Object { get { return m_Member_Object; } }
            public JsonProperty Member_Object_VariableTemplateId { get { return m_Member_Object_VariableTemplateId; } }
            public JsonProperty Member_Object_Priority { get { return m_Member_Object_Priority; } }

            #endregion
        }

        public class Decia_VariableTemplate_JsonDocument : JsonDocument
        {
            public const string DefaultDocumentName = "Decia_VariableTemplate";
            public const string DefaultCollectionName = "Decia_VariableTemplates";

            #region Members

            private JsonProperty m_Id;
            private JsonProperty m_Name;
            private JsonProperty m_MongoName;
            private JsonProperty m_Description;
            private JsonProperty m_StructuralTypeId;
            private JsonProperty m_Related_StructuralTypeId;
            private JsonProperty m_Related_StructuralDimensionNumber;
            private JsonProperty m_IsComputed;
            private JsonProperty m_TimeDimensionCount;
            private JsonProperty m_PrimaryTimePeriodTypeId;
            private JsonProperty m_SecondaryTimePeriodTypeId;
            private JsonProperty m_DataTypeId;
            private JsonProperty m_Instance_Field_Name;
            private JsonProperty m_Instance_Field_DefaultValue;

            private JsonProperty m_Dependencies_Array;
            private JsonProperty m_Dependency_Object;
            private JsonProperty m_Dependency_Object_VariableTemplateId;
            private JsonProperty m_Dependency_Object_StructuralDimensionNumber;
            private JsonProperty m_Dependency_Object_IsStrict;

            #endregion

            #region Constructors

            public Decia_VariableTemplate_JsonDocument(DeciaBase_JsonSet parentSet)
                : base(parentSet, DefaultDocumentName)
            {
                CollectionName = DefaultCollectionName;

                m_Id = new JsonProperty(this.Root, "_id", DeciaDataType.UniqueID);
                m_Name = new JsonProperty(this.Root, "Name", DeciaDataType.Text);
                m_MongoName = new JsonProperty(this.Root, "MongoName", DeciaDataType.Text);
                m_Description = new JsonProperty(this.Root, "Description", DeciaDataType.Text);
                m_StructuralTypeId = new JsonProperty(this.Root, "StructuralTypeId", DeciaDataType.UniqueID);
                m_Related_StructuralTypeId = new JsonProperty(this.Root, "Related_StructuralTypeId", DeciaDataType.UniqueID);
                m_Related_StructuralDimensionNumber = new JsonProperty(this.Root, "Related_StructuralDimensionNumber", DeciaDataType.Integer);
                m_IsComputed = new JsonProperty(this.Root, "IsComputed", DeciaDataType.Boolean);
                m_TimeDimensionCount = new JsonProperty(this.Root, "TimeDimensionCount", DeciaDataType.Integer);
                m_PrimaryTimePeriodTypeId = new JsonProperty(this.Root, "PrimaryTimePeriodTypeId", DeciaDataType.Integer);
                m_SecondaryTimePeriodTypeId = new JsonProperty(this.Root, "SecondaryTimePeriodTypeId", DeciaDataType.Integer);
                m_DataTypeId = new JsonProperty(this.Root, "DataTypeId", DeciaDataType.Integer);
                m_Instance_Field_Name = new JsonProperty(this.Root, "Instance_Field_Name", DeciaDataType.Text);
                m_Instance_Field_DefaultValue = new JsonProperty(this.Root, "Instance_Field_DefaultValue", DeciaDataType.Text);

                m_Dependencies_Array = new JsonProperty(this.Root, "Dependencies", DeciaComplexType.Array);
                m_Dependency_Object = new JsonProperty(m_Dependencies_Array);
                m_Dependency_Object_VariableTemplateId = new JsonProperty(m_Dependency_Object, "VariableTemplateId", DeciaDataType.UniqueID);
                m_Dependency_Object_StructuralDimensionNumber = new JsonProperty(m_Dependency_Object, "StructuralDimensionNumber", DeciaDataType.Integer);
                m_Dependency_Object_IsStrict = new JsonProperty(m_Dependency_Object, "IsStrict", DeciaDataType.Boolean);
            }

            #endregion

            #region Properties

            public JsonProperty Id { get { return m_Id; } }
            public JsonProperty Name { get { return m_Name; } }
            public JsonProperty MongoName { get { return m_MongoName; } }
            public JsonProperty Description { get { return m_Description; } }
            public JsonProperty StructuralTypeId { get { return m_StructuralTypeId; } }
            public JsonProperty Related_StructuralTypeId { get { return m_Related_StructuralTypeId; } }
            public JsonProperty Related_StructuralDimensionNumber { get { return m_Related_StructuralDimensionNumber; } }
            public JsonProperty IsComputed { get { return m_IsComputed; } }
            public JsonProperty TimeDimensionCount { get { return m_TimeDimensionCount; } }
            public JsonProperty PrimaryTimePeriodTypeId { get { return m_PrimaryTimePeriodTypeId; } }
            public JsonProperty SecondaryTimePeriodTypeId { get { return m_SecondaryTimePeriodTypeId; } }
            public JsonProperty DataTypeId { get { return m_DataTypeId; } }
            public JsonProperty Instance_Field_Name { get { return m_Instance_Field_Name; } }
            public JsonProperty Instance_Field_DefaultValue { get { return m_Instance_Field_DefaultValue; } }

            public JsonProperty Dependencies_Array { get { return m_Dependencies_Array; } }
            public JsonProperty Dependency_Object { get { return m_Dependency_Object; } }
            public JsonProperty Dependency_Object_VariableTemplateId { get { return m_Dependency_Object_VariableTemplateId; } }
            public JsonProperty Dependency_Object_StructuralDimensionNumber { get { return m_Dependency_Object_StructuralDimensionNumber; } }
            public JsonProperty Dependency_Object_IsStrict { get { return m_Dependency_Object_IsStrict; } }

            #endregion
        }

        #endregion
    }
}