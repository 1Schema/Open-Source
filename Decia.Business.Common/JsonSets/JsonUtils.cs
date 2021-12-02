using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Common.JsonSets
{
    public static class JsonUtils
    {
        public static readonly string[] RefSplitter = new string[] { "\"title\":" };

        #region GetSchemaAsText Methods

        public static string GetSchemaAsText(this JSchema jsonSchema)
        {
            return GetSchemaAsText(jsonSchema, false);
        }

        public static string GetSchemaAsText(this JSchema jsonSchema, bool isDocument)
        {
            var textWriter = new StringWriter();
            var jsonWriter = new JsonTextWriter(textWriter);

            jsonWriter.Formatting = Formatting.Indented;
            jsonSchema.WriteTo(jsonWriter);

            var schemaAsText = textWriter.GetStringBuilder().ToString();

            if (!isDocument)
            { return schemaAsText; }

            var titleParts = schemaAsText.Split(RefSplitter, StringSplitOptions.RemoveEmptyEntries);

            if (titleParts.Length < 1)
            { return schemaAsText; }

            var skipFirst = (titleParts[0].Trim() == "{");
            var hasSkipped = false;
            var schemaAsText_WithRefs = string.Empty;

            foreach (var titlePart in titleParts)
            {
                if (string.IsNullOrWhiteSpace(schemaAsText_WithRefs))
                { schemaAsText_WithRefs += titlePart; }
                else if (skipFirst && !hasSkipped)
                {
                    schemaAsText_WithRefs += ("\"title\":" + titlePart);
                    hasSkipped = true;
                }
                else
                {
                    var indexOfNextComma = titlePart.IndexOf(',');
                    var indexOfNextEndBrace = titlePart.IndexOf('}');
                    var titlePartAdj = string.Empty;

                    if (indexOfNextEndBrace < 0)
                    { titlePartAdj = titlePart; }
                    else if ((indexOfNextComma < 0) || (indexOfNextComma > indexOfNextEndBrace))
                    { titlePartAdj = titlePart; }
                    else
                    {
                        while ((indexOfNextEndBrace > 0) && string.IsNullOrWhiteSpace(titlePart[indexOfNextEndBrace - 1].ToString()))
                        { indexOfNextEndBrace--; }

                        titlePartAdj = titlePart.Remove(indexOfNextComma, (indexOfNextEndBrace - indexOfNextComma));
                    }

                    schemaAsText_WithRefs += ("\"$ref\":" + titlePartAdj);
                }
            }

            return schemaAsText_WithRefs;
        }

        #endregion

        #region GetSchemaAsCode Methods

        public const string SchemaAsCode_Indent = "  ";
        public static readonly string InternalChangeMgmtCode_Format = "var {0} = {1};";
        public static readonly string MongooseSchemaCode_Format = "var {0} = Schema({1});";
        public const string MongooseModelCode_Format = "var {0} = mongoose.model('{0}', {1});";

        public static string GetSchemaAsCode_ForInternalChangeMgmt(this JSchema jSchema)
        {
            var documentName = jSchema.Title;
            var schemaName = (string.Empty + Char.ToLower(documentName[0]) + documentName.Substring(1, documentName.Length - 1));
            var schemaBody = string.Empty;
            var currentIndent = string.Empty;

            documentName = documentName.Replace(" ", "_");
            schemaName = schemaName.Replace(" ", "_") + "_Schema";

            var outerJSchema = new JSchema();
            outerJSchema.Properties.Add(string.Empty, jSchema);

            schemaBody = GetSchemaAsCode_Recursor(outerJSchema, currentIndent, true);

            var internalCode = string.Empty;
            internalCode = string.Format(InternalChangeMgmtCode_Format, schemaName, schemaBody);
            return internalCode;
        }

        public static string GetSchemaAsCode_ForMongoose(this JSchema jSchema)
        {
            var documentName = jSchema.Title;
            var schemaName = (string.Empty + Char.ToLower(documentName[0]) + documentName.Substring(1, documentName.Length - 1));
            var schemaBody = string.Empty;
            var currentIndent = SchemaAsCode_Indent;

            documentName = documentName.Replace(" ", "_");
            schemaName = schemaName.Replace(" ", "_") + "_Schema";

            schemaBody = GetSchemaAsCode_Recursor(jSchema, currentIndent, false);
            schemaBody = "{" + Environment.NewLine + schemaBody + Environment.NewLine + "}";

            var mongooseCode = string.Empty;
            mongooseCode = string.Format(MongooseSchemaCode_Format, schemaName, schemaBody);
            mongooseCode += Environment.NewLine;
            mongooseCode += string.Format(MongooseModelCode_Format, documentName, schemaName);
            return mongooseCode;
        }

        private static string GetSchemaAsCode_Recursor(this JSchema jSchema, string indent, bool compliantMode)
        {
            var isArray = (jSchema.Type == JSchemaType.Array);
            var code = string.Empty;

            var jPropertyBuckets = (isArray) ? jSchema.Items.Select(x => new KeyValuePair<string, JSchema>(string.Empty, x)).ToList() : jSchema.Properties.ToList();
            var jPropertyBucket_Last = jPropertyBuckets.LastOrDefault();

            foreach (var jPropertyBucket in jPropertyBuckets)
            {
                var jPropertyName = jPropertyBucket.Key;
                var jProperty_HasNamePart = (!string.IsNullOrWhiteSpace(jPropertyName));
                var jProperty_NamePart = (jProperty_HasNamePart) ? ("\"" + jPropertyName + "\": ") : string.Empty;

                var jProperty = jPropertyBucket.Value;
                var jPropertyUri = jProperty.Id;
                var jPropertyTitle = jProperty.Title;
                var requiresComma = (jProperty != jPropertyBucket_Last.Value);

                if (jProperty.Type == JSchemaType.Array)
                {
                    if (compliantMode)
                    {
                        var type_Extended = JsonSchemaType_Extended.Array;
                        var type_Extended_AsText = type_Extended.GetJsonType_Extended_AsText();

                        var innerIndent = (indent + SchemaAsCode_Indent + SchemaAsCode_Indent);
                        var innerCode = GetSchemaAsCode_Recursor(jProperty, innerIndent, compliantMode);

                        code += indent + jProperty_NamePart + "{" + Environment.NewLine;
                        code += indent + SchemaAsCode_Indent + "\"type\": \"" + type_Extended_AsText + "\"," + Environment.NewLine;
                        code += indent + SchemaAsCode_Indent + "\"items\": {" + Environment.NewLine;
                        code += innerCode + Environment.NewLine;
                        code += indent + SchemaAsCode_Indent + "}" + Environment.NewLine;
                        code += indent + "}";
                    }
                    else
                    {
                        var innerIndent = (indent + SchemaAsCode_Indent);
                        var innerCode = GetSchemaAsCode_Recursor(jProperty, innerIndent, compliantMode);

                        code += indent + jProperty_NamePart + "[" + Environment.NewLine;
                        code += innerCode + Environment.NewLine;
                        code += indent + "]";
                    }
                }
                else if (jProperty.Type == JSchemaType.Object)
                {
                    if (compliantMode)
                    {
                        var type_Extended = JsonSchemaType_Extended.Object;
                        var type_Extended_AsText = type_Extended.GetJsonType_Extended_AsText();
                        var type_HasTitle = (!string.IsNullOrWhiteSpace(jProperty.Title));
                        var type_HasProperties = (jProperty.Properties.Count > 0);
                        var type_HasMore = ((jProperty_HasNamePart && type_HasTitle) || type_HasProperties);

                        var innerIndent = (indent + SchemaAsCode_Indent + SchemaAsCode_Indent);
                        var innerCode = GetSchemaAsCode_Recursor(jProperty, innerIndent, compliantMode);

                        code += indent + jProperty_NamePart + "{" + Environment.NewLine;
                        if (!jProperty_HasNamePart && type_HasTitle)
                        {
                            code += indent + SchemaAsCode_Indent + "\"title\": \"" + jProperty.Title + "\"" + "," + Environment.NewLine;
                        }
                        code += indent + SchemaAsCode_Indent + "\"type\": \"" + type_Extended_AsText + "\"" + (type_HasMore ? "," : "") + Environment.NewLine;
                        if (jProperty_HasNamePart && type_HasTitle)
                        {
                            code += indent + SchemaAsCode_Indent + "\"$ref\": \"" + jProperty.Title + "\"" + (type_HasProperties ? "," : "") + Environment.NewLine;
                        }
                        if (type_HasProperties)
                        {
                            code += indent + SchemaAsCode_Indent + "\"properties\": {" + Environment.NewLine;
                            code += innerCode + Environment.NewLine;
                            code += indent + SchemaAsCode_Indent + "}" + Environment.NewLine;
                        }
                        code += indent + "}";
                    }
                    else
                    {
                        var innerIndent = (indent + SchemaAsCode_Indent);
                        var innerCode = GetSchemaAsCode_Recursor(jProperty, innerIndent, compliantMode);

                        code += indent + jProperty_NamePart + "{" + Environment.NewLine;
                        code += innerCode + Environment.NewLine;
                        code += indent + "}";
                    }
                }
                else
                {
                    var type_Extended = (JsonSchemaType_Extended)jProperty.Type;
                    var type_Extended_AsText = type_Extended.GetJsonType_Extended_AsText();
                    var type_HasTitle = (!string.IsNullOrWhiteSpace(jProperty.Title));

                    var jProperty_ValuePart = string.Empty;

                    jProperty_ValuePart += " { ";
                    jProperty_ValuePart += ("type: \"" + type_Extended_AsText + "\"");
                    if (type_HasTitle)
                    { jProperty_ValuePart += (", ref: \"" + jProperty.Title + "\" "); }
                    else
                    { jProperty_ValuePart += " "; }
                    jProperty_ValuePart += "}";

                    code += indent + jProperty_NamePart + jProperty_ValuePart;
                }

                if (requiresComma)
                { code += ("," + Environment.NewLine); }
            }
            return code;
        }

        #endregion

        #region GetJsonType Methods

        public static JSchemaType GetJsonType(this DeciaComplexType complexType)
        {
            if (complexType == DeciaComplexType.Array)
            { return JSchemaType.Array; }
            else if (complexType == DeciaComplexType.Object)
            { return JSchemaType.Object; }
            else
            { throw new InvalidOperationException("Unrecognized Complex  Type encountered."); }
        }

        public static JSchemaType GetJsonType(this DeciaDataType dataType)
        {
            var type_Extended = GetJsonType_Extended(dataType);
            return (JSchemaType)((int)type_Extended);
        }

        public static JsonSchemaType_Extended GetJsonType_Extended(this DeciaDataType dataType)
        {
            if (dataType == DeciaDataType.Boolean)
            { return JsonSchemaType_Extended.Boolean; }
            else if (dataType == DeciaDataType.Integer)
            { return JsonSchemaType_Extended.Number; }
            else if (dataType == DeciaDataType.Decimal)
            { return JsonSchemaType_Extended.Number; }
            else if (dataType == DeciaDataType.UniqueID)
            { return JsonSchemaType_Extended.ObjectIdOrString; }
            else if (dataType == DeciaDataType.DateTime)
            { return JsonSchemaType_Extended.DateOrString; }
            else if (dataType == DeciaDataType.TimeSpan)
            { return JsonSchemaType_Extended.Number; }
            else if (dataType == DeciaDataType.Text)
            { return JsonSchemaType_Extended.String; }
            else
            { throw new InvalidOperationException("Unrecognized Simple Type encountered."); }
        }

        public static string GetJsonType_Extended_AsText(this JsonSchemaType_Extended dataType)
        {
            if (EnumUtils.Bitwise_IsSet(dataType, JsonSchemaType_Extended.Array))
            { return "Array"; }
            else if (EnumUtils.Bitwise_IsSet(dataType, JsonSchemaType_Extended.Object))
            { return "Object"; }
            else if (EnumUtils.Bitwise_IsSet(dataType, JsonSchemaType_Extended.Null))
            { return "Null"; }
            else if (EnumUtils.Bitwise_IsSet(dataType, JsonSchemaType_Extended.None))
            { return "None"; }
            else if (EnumUtils.Bitwise_IsSet(dataType, JsonSchemaType_Extended.Boolean))
            { return "Boolean"; }
            else if (EnumUtils.Bitwise_IsSet(dataType, JsonSchemaType_Extended.Integer))
            { return "Number"; }
            else if (EnumUtils.Bitwise_IsSet(dataType, JsonSchemaType_Extended.Number))
            { return "Number"; }
            else if (EnumUtils.Bitwise_IsSet(dataType, JsonSchemaType_Extended.Buffer))
            { return "Buffer"; }
            else if (EnumUtils.Bitwise_IsSet(dataType, JsonSchemaType_Extended.Date))
            { return "Date"; }
            else if (EnumUtils.Bitwise_IsSet(dataType, JsonSchemaType_Extended.ObjectId))
            { return "ObjectId"; }
            else
            { return "String"; }
        }

        #endregion
    }
}