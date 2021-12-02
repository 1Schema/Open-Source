using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.NoSql.Base;

namespace Decia.Business.Common.NoSql
{
    public static class GenericDatabaseUtils
    {
        public static readonly string ScriptSpacer_Minor = Environment.NewLine + Environment.NewLine + Environment.NewLine;
        public static readonly string ScriptSpacer_Major = ScriptSpacer_Minor + ScriptSpacer_Minor;
        public const string NamePartSpacer = "___";
        public const string Indent_L0 = "";
        public const string Indent_L1 = "  ";
        public const string Indent_L2 = Indent_L1 + Indent_L1;
        public const string Indent_L3 = Indent_L2 + Indent_L1;
        public const string Indent_L4 = Indent_L3 + Indent_L1;
        public const string Indent_L5 = Indent_L4 + Indent_L1;
        public const string Indent_L6 = Indent_L5 + Indent_L1;
        public const string Indent_L7 = Indent_L6 + Indent_L1;
        public const string Indent_L8 = Indent_L7 + Indent_L1;
        public const string Indent_L9 = Indent_L8 + Indent_L1;

        public const string DocId_Name = "_id";
        public const string CachedDoc_Name = "OS_CACHED_DOC";
        public const string DoNotUpdate_Name = "OS_DO_NOT_UPDATE";
        public const string LastUpdateDate_Name = "OS_LAST_UPDATE_DATE";
        public const string MaxCachingDepth_Name = "OS_MAX_CACHING_DEPTH";

        #region Database Object Model Methods

        public static GenericCollection GetRootCollection(this GenericCollection collection)
        {
            var currentCollection = collection;
            while (currentCollection.ParentCollection != null)
            { currentCollection = currentCollection.ParentCollection; }
            return currentCollection;
        }

        public static List<GenericCollection> GetCollectionSubTree(this GenericCollection rootCollection, bool includeRoot)
        {
            Func<GenericCollection, List<GenericField>> fieldsToProcessGetter = ((col) => col.LocalFields.Where(x => x.FieldMode == FieldMode.NestedCollection).ToList());
            var fieldsToProcess = fieldsToProcessGetter(rootCollection);

            var collectionSubTree = new List<GenericCollection>();
            if (includeRoot)
            { collectionSubTree.Add(rootCollection); }

            while (fieldsToProcess.Count > 0)
            {
                var field = fieldsToProcess[0];
                fieldsToProcess.RemoveAt(0);

                var nestedCollection = field.NestedCollection;
                collectionSubTree.Add(nestedCollection);

                fieldsToProcess.AddRange(fieldsToProcessGetter(nestedCollection));
            }
            return collectionSubTree;
        }

        public static string GetCollectionPathToRootAsText(this GenericCollection leafCollection, bool includeLeaf)
        {
            string rootCollectionName, nestedCollectionPath;
            var path = GetCollectionPathToRoot(leafCollection, includeLeaf, out rootCollectionName, out nestedCollectionPath);
            return nestedCollectionPath;
        }

        public static List<GenericCollection> GetCollectionPathToRoot(this GenericCollection leafCollection, bool includeLeaf, out string rootCollectionName, out string nestedCollectionPath)
        {
            var parentCollections = new List<GenericCollection>();
            var currentCollection = (includeLeaf) ? leafCollection : leafCollection.ParentCollection;

            rootCollectionName = string.Empty;
            nestedCollectionPath = string.Empty;

            while (currentCollection != null)
            {
                parentCollections.Insert(0, currentCollection);
                var collectionName = currentCollection.XmlMapping.CollectionElement.Name;

                if (currentCollection.ParentCollection == null)
                { rootCollectionName = collectionName; }
                else
                { nestedCollectionPath = (collectionName + "." + nestedCollectionPath); }

                currentCollection = currentCollection.ParentCollection;
            }
            return parentCollections;
        }

        public static Dictionary<FieldMode, List<GenericField>> GetFieldsByMode(this GenericCollection collection)
        {
            var fieldsByMode = new Dictionary<FieldMode, List<GenericField>>();

            foreach (var fieldMode in EnumUtils.GetEnumValues<FieldMode>())
            {
                var fieldList = collection.LocalFields.Where(x => x.FieldMode == fieldMode).ToList();
                fieldsByMode.Add(fieldMode, fieldList);
            }

            return fieldsByMode;
        }

        #endregion

        #region Naming Methods

        public const string TypeName_Postfix = "_Type";

        public static string GetTypeName_FromName(this string name)
        {
            var typeName = (name + TypeName_Postfix);
            return typeName;
        }

        public static string GetName_FromTypeName(this string typeName)
        {
            var name = typeName.Replace(TypeName_Postfix, string.Empty);
            return name;
        }

        #endregion

        #region Schema Export Methods

        public const bool Use_MultiLine_Annotations = false;

        public static XmlQualifiedName UniqueId_XmlSimpleTypeName { get { return DeciaDataType.UniqueID.GetXmlSimpleTypeQualdName(); } }
        public static XmlSchemaSimpleType UniqueId_XmlSimpleType { get { return DeciaDataType.UniqueID.ToXmlSimpleType(); } }

        public static string ExportSchemaAsText(this XmlSchemaObject schemaObject)
        {
            var schemaObjects = new XmlSchemaObject[] { schemaObject };
            return ExportSchemaAsText(schemaObjects);
        }

        public static string ExportSchemaAsText(this IEnumerable<XmlSchemaObject> schemaObjects)
        {
            var schema = new XmlSchema();

            foreach (var schemaObject in schemaObjects)
            { schema.Items.Add(schemaObject); }

            return ExportSchemaAsText(schema);
        }

        public static string ExportSchemaAsText(this XmlSchema schema)
        {
            var xsdSettings = new XmlWriterSettings();
            xsdSettings.ConformanceLevel = ConformanceLevel.Document;
            xsdSettings.Indent = true;
            xsdSettings.Encoding = Encoding.UTF8;
            xsdSettings.NewLineHandling = NewLineHandling.Entitize;
            xsdSettings.NewLineOnAttributes = false;

            var xsdStringBuilder = new StringBuilder();
            var xsdWriter = XmlWriter.Create(xsdStringBuilder, xsdSettings);
            schema.Write(xsdWriter);

            var xsdAsText = xsdStringBuilder.ToString();
            return xsdAsText;
        }

        public static string GetXmlSimpleTypeName(this DeciaDataType dataType)
        {
            return dataType.ToString();
        }

        public static XmlQualifiedName GetXmlSimpleTypeQualdName(this DeciaDataType dataType)
        {
            var typeName = dataType.GetXmlSimpleTypeName();
            return new XmlQualifiedName(typeName);
        }

        public static XmlTypeCode ToXmlTypeCode(this DeciaDataType dataType)
        {
            if (dataType == DeciaDataType.Boolean)
            { return XmlTypeCode.Boolean; }
            else if (dataType == DeciaDataType.DateTime)
            { return XmlTypeCode.DateTime; }
            else if (dataType == DeciaDataType.Decimal)
            { return XmlTypeCode.Double; }
            else if (dataType == DeciaDataType.Integer)
            { return XmlTypeCode.Long; }
            else if (dataType == DeciaDataType.Text)
            { return XmlTypeCode.String; }
            else if (dataType == DeciaDataType.TimeSpan)
            { return XmlTypeCode.Duration; }
            else if (dataType == DeciaDataType.UniqueID)
            { return XmlTypeCode.Id; }
            else
            { throw new NotImplementedException("The specified Data Type is not supported yet."); }
        }

        public static XmlQualifiedName ToXmlBaseTypeName(this DeciaDataType dataType)
        {
            if (dataType == DeciaDataType.Boolean)
            { return new XmlQualifiedName("boolean", "http://www.w3.org/2001/XMLSchema"); }
            else if (dataType == DeciaDataType.DateTime)
            { return new XmlQualifiedName("dateTime", "http://www.w3.org/2001/XMLSchema"); }
            else if (dataType == DeciaDataType.Decimal)
            { return new XmlQualifiedName("double", "http://www.w3.org/2001/XMLSchema"); }
            else if (dataType == DeciaDataType.Integer)
            { return new XmlQualifiedName("long", "http://www.w3.org/2001/XMLSchema"); }
            else if (dataType == DeciaDataType.Text)
            { return new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema"); }
            else if (dataType == DeciaDataType.TimeSpan)
            { return new XmlQualifiedName("duration", "http://www.w3.org/2001/XMLSchema"); }
            else if (dataType == DeciaDataType.UniqueID)
            { return new XmlQualifiedName("ID", "http://www.w3.org/2001/XMLSchema"); }
            else
            { throw new NotImplementedException("The specified Data Type is not supported yet."); }
        }

        public static XmlSchemaSimpleType ToXmlSimpleType(this DeciaDataType dataType)
        {
            var xmlSimpleTypeRestriction = new XmlSchemaSimpleTypeRestriction();
            xmlSimpleTypeRestriction.BaseTypeName = dataType.ToXmlBaseTypeName();

            var xmlSimpleType = new XmlSchemaSimpleType();
            xmlSimpleType.Name = dataType.GetXmlSimpleTypeName();
            xmlSimpleType.Content = xmlSimpleTypeRestriction;

            return xmlSimpleType;
        }

        public static XmlSchemaComplexType GetComplexType(this string refName)
        {
            var complexType = new XmlSchemaComplexType();
            complexType.Name = refName;
            return complexType;
        }

        #endregion

        #region Schema Comment Methods

        public static void AddSchemaComments(this XmlSchemaElement schemaElement, string comment)
        {
            var comments = new string[] { comment };
            AddSchemaComments(schemaElement, comments);
        }

        public static void AddSchemaComments(this XmlSchemaElement schemaElement, IEnumerable<string> comments)
        {
            var commentNodes = new List<XmlNode>();
            var isFirst = true;
            var newLine = Use_MultiLine_Annotations ? "\n" : string.Empty;
            var indentAsSpaces = Use_MultiLine_Annotations ? "                " : "    ";

            var startDoc = new XmlDocument();
            var startNode = startDoc.CreateTextNode(newLine + indentAsSpaces);
            commentNodes.Add(startNode);

            var endLineDoc = new XmlDocument();
            var endLineNode = endLineDoc.CreateTextNode("," + newLine + indentAsSpaces);

            foreach (var comment in comments)
            {
                if (isFirst)
                { isFirst = false; }
                else
                { commentNodes.Add(endLineNode); }

                var commentDoc = new XmlDocument();
                var commentNode = commentDoc.CreateTextNode(comment);
                commentNodes.Add(commentNode);
            }

            var finishDoc = new XmlDocument();
            var finishNode = finishDoc.CreateTextNode(newLine + indentAsSpaces);
            commentNodes.Add(finishNode);

            var commentsElement = new XmlSchemaDocumentation();
            commentsElement.Markup = commentNodes.ToArray();

            var commentsSection = new XmlSchemaAnnotation();
            commentsSection.Items.Add(commentsElement);

            schemaElement.Annotation = commentsSection;
        }

        #endregion

        #region Export Database Methods

        public static string InsertPlaceholderValues(this string functionDef, IDictionary<string, string> placeholderValues)
        {
            foreach (var placeholderName in DeciaBaseUtils.Fnctn_Decia_Format_Placeholders)
            {
                if (!functionDef.Contains(placeholderName))
                { continue; }

                var placeholderValue = placeholderValues[placeholderName];
                functionDef = functionDef.Replace(placeholderName, placeholderValue);
            }
            return functionDef;
        }

        #endregion
    }
}