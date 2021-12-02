using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Schema;
using Decia.Business.Common.Collections;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Exports;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.NoSql.Base;

namespace Decia.Business.Common.NoSql.Functions
{
    public class CollectionChangeHandler_Function
    {
        public const string CollectionChangeHandler_Prefix = "fnDecia_Consistency_HandleChangeTo__";
        public const string CollectionChangeHandler_Postfix = "";
        public static readonly string NewLine = Environment.NewLine;

        public static readonly LocalCacheMode[] Invalid_LocalCacheModes = new LocalCacheMode[] { LocalCacheMode.IdOnly };
        public static readonly ForeignCacheMode[] Invalid_ForeignCacheModes = new ForeignCacheMode[] { ForeignCacheMode.None };

        #region Members

        private GenericCollection m_RootCollection;

        #endregion

        #region Constructors

        public CollectionChangeHandler_Function(GenericCollection rootCollection)
        {
            if (rootCollection == null)
            { throw new InvalidOperationException("The Root Collection must not be null."); }
            if (rootCollection.ParentCollection != null)
            { throw new InvalidOperationException("Change handling can only occur for a Root Collection."); }

            m_RootCollection = rootCollection;
        }

        #endregion

        #region Properties

        public GenericDatabase ParentDatabase { get { return RootCollection.ParentDatabase; } }
        public GenericCollection RootCollection { get { return m_RootCollection; } }
        public XmlCollectionMapping RootCollectionMapping { get { return RootCollection.XmlMapping; } }
        public XmlSchemaElement RootCollectionElement { get { return RootCollectionMapping.CollectionElement; } }

        #endregion

        #region Export Database Methods

        public string ExportCollectionChangeHandler(NoSqlDb_TargetType dbType)
        {
            var myDbName = DeciaBaseUtils.MyDb_Name;
            var collectionName = RootCollection.XmlMapping.CollectionElement.Name;
            var functionName = CollectionChangeHandler_Prefix + collectionName + CollectionChangeHandler_Postfix;

            var thisSubTreeState = new CollectionSubTreeState(RootCollection, "thisCollection", "thisModified");

            if (dbType == NoSqlDb_TargetType.MongoDb)
            {
                var requiredDef = string.Empty;
                var functionDef = string.Empty;

                requiredDef += string.Format("{0}.system.js.save(", myDbName) + NewLine;
                requiredDef += "  {" + NewLine;
                requiredDef += string.Format("    _id: \"{0}\",", functionName) + NewLine;
                requiredDef += "    value: function(changeItem, writeConcern) {" + NewLine;

                functionDef += requiredDef;
                functionDef += "      var MyDb = db.getSiblingDB(\"<MY_DB_NAME>\");" + NewLine;
                functionDef += "      var thisModified = null;" + NewLine;
                functionDef += "      var thisOriginal = null;" + NewLine;
                functionDef += "      var performDelete = true;" + NewLine;
                functionDef += NewLine;

                functionDef += "      " + string.Format("var thisCollection = MyDb.getCollection(\"{0}\");", RootCollectionElement.Name) + NewLine;
                functionDef += "      var thisCursor = thisCollection.find({ _id: changeItem.ObjectId });" + NewLine;
                functionDef += "      if (thisCursor.hasNext()) {" + NewLine;
                functionDef += "        thisModified = thisCursor.next();" + NewLine;
                functionDef += "        thisOriginal = fnDecia_Utility_CopyJsonObject(thisModified);" + NewLine;
                functionDef += "        performDelete = false;" + NewLine;
                functionDef += "      }" + NewLine;
                functionDef += NewLine;

                functionDef += "      var isExpectingDeletion = (fnDecia_Utility_IsNotNull(changeItem.IsDeleted) && (changeItem.IsDeleted == true));" + NewLine;
                functionDef += "      if (!isExpectingDeletion && fnDecia_Utility_IsNull(thisModified))" + NewLine;
                functionDef += "      {" + NewLine;
                functionDef += "        return true;" + NewLine;
                functionDef += "      }" + NewLine;
                functionDef += NewLine;

                functionDef += "      var currentObjectToUseForState = null;" + NewLine;
                functionDef += "      var originalObjectToUseForState = null;" + NewLine;
                functionDef += "      if (performDelete) {" + NewLine;
                functionDef += "        var hasOpLogObject = (fnDecia_Utility_HasProperty(changeItem.DeletedLogItem) && fnDecia_Utility_HasProperty(changeItem.DeletedLogItem.o));" + NewLine;
                functionDef += "        var hasDirectObject = (fnDecia_Utility_HasProperty(changeItem.DeletedObject));" + NewLine;
                functionDef += NewLine;
                functionDef += "        currentObjectToUseForState = (hasOpLogObject) ? changeItem.DeletedLogItem.o : (hasDirectObject ? changeItem.DeletedObject : null);" + NewLine;
                functionDef += "        originalObjectToUseForState = null;" + NewLine;
                functionDef += "      }" + NewLine;
                functionDef += "      else {" + NewLine;
                functionDef += "        var hasOpLogObject = (fnDecia_Utility_HasProperty(changeItem.UpdatedLogItem) && fnDecia_Utility_HasProperty(changeItem.UpdatedLogItem.o));" + NewLine;
                functionDef += "        var hasDirectObject = (fnDecia_Utility_HasProperty(changeItem.UpdatedObject));" + NewLine;
                functionDef += NewLine;
                functionDef += "        currentObjectToUseForState = thisModified;" + NewLine;
                functionDef += "        originalObjectToUseForState = (hasOpLogObject) ? changeItem.UpdatedLogItem.o : (hasDirectObject ? changeItem.UpdatedObject : null);" + NewLine;
                functionDef += "      }" + NewLine;
                functionDef += NewLine;

                functionDef += "      var pathsToNestedCollections = " + thisSubTreeState.Var_AllPaths_PlusRefPaths_AsArray + ";" + NewLine;
                functionDef += "      var objectStateValue = fnDecia_Utility_GenerateObjectStateValue(currentObjectToUseForState, originalObjectToUseForState, pathsToNestedCollections, performDelete);" + NewLine;
                functionDef += NewLine;

                var nestedDocUpdateCode = ExportCachedDocumentSetters(dbType, RootCollection, thisSubTreeState, 1, "        ");
                var externalDocUpdateCode = string.Empty;
                bool lastHadCode = false;

                foreach (var otherRootCollection in this.ParentDatabase.RootCollections_Ordered)
                {
                    if (lastHadCode)
                    {
                        externalDocUpdateCode += NewLine;
                        lastHadCode = false;
                    }

                    var otherSubTreeState = new CollectionSubTreeState(otherRootCollection, "otherCollection", "otherModified");
                    var otherUpdateCode = ExportExternalDocumentUpdatersForSubTree(dbType, otherSubTreeState, thisSubTreeState, "      ");
                    externalDocUpdateCode += otherUpdateCode;

                    if (!string.IsNullOrWhiteSpace(otherUpdateCode))
                    { lastHadCode = true; }
                }

                var hasNestedCode = (!string.IsNullOrWhiteSpace(nestedDocUpdateCode));
                var hasExternalCode = (!string.IsNullOrWhiteSpace(externalDocUpdateCode));

                if (!hasNestedCode && !hasExternalCode)
                {
                    requiredDef += "      return true;" + NewLine;
                    requiredDef += "    }" + NewLine;
                    requiredDef += "  }" + NewLine;
                    requiredDef += ");" + NewLine;
                    return requiredDef;
                }

                if (hasNestedCode)
                {
                    functionDef += "      if (!performDelete) {" + NewLine;
                    functionDef += nestedDocUpdateCode;
                    functionDef += NewLine;

                    functionDef += "        thisModified = fnDecia_Utility_CopyJsonObject(thisModified);" + NewLine;
                    functionDef += NewLine;
                    functionDef += "        if (!fnDecia_Utility_AreJsonObjectsEqual(thisModified, thisOriginal)) {" + NewLine;
                    functionDef += "          thisCollection.save(thisModified);" + NewLine;
                    functionDef += "          return false;" + NewLine;
                    functionDef += "        }" + NewLine;
                    functionDef += "      }" + NewLine;
                    functionDef += NewLine;
                }

                if (hasExternalCode)
                {
                    functionDef += externalDocUpdateCode;
                    functionDef += NewLine;
                }

                functionDef += "      return true;" + NewLine;

                functionDef += "    }" + NewLine;
                functionDef += "  }" + NewLine;
                functionDef += ");" + NewLine;

                return functionDef;
            }
            else
            { throw new NotImplementedException("The specified Database Type is not supported yet."); }
        }

        private string ExportCachedDocumentSetters(NoSqlDb_TargetType dbType, GenericCollection currentCollection, CollectionSubTreeState subTreeState, int currentNestLevel, string currentIndent)
        {
            var rootState = subTreeState.GetState(subTreeState.RootCollection);
            var currentState = subTreeState.GetState(currentCollection);
            var loopVar = "i" + currentNestLevel;
            var itemVar = "item" + currentNestLevel;

            var codeDef = string.Empty;
            var hasQueries = false;

            if (dbType == NoSqlDb_TargetType.MongoDb)
            {
                var nestedColFields = currentCollection.LocalFields.Where(x => (x.FieldMode == FieldMode.NestedCollection)).ToList();
                var cachedDocFields = currentCollection.LocalFields.Where(x => ((x.FieldMode == FieldMode.CachedDocument) && (!Invalid_LocalCacheModes.Contains(x.LocalCacheMode)))).ToList();

                foreach (var cachedDocField in cachedDocFields)
                {
                    if (hasQueries)
                    { codeDef += NewLine; }

                    var fieldPath = (currentState.Var_Object_FieldBase + cachedDocField.XmlMapping.RootFieldElement.Name);

                    string foreignRootCollectionName, foreignCollectionPathWithDot;
                    var foreignCollectionPathToRoot = cachedDocField.CachedDocumentSource.GetCollectionPathToRoot(true, out foreignRootCollectionName, out foreignCollectionPathWithDot);
                    var foreignCollectionPath = (!string.IsNullOrWhiteSpace(foreignCollectionPathWithDot)) ? foreignCollectionPathWithDot.Substring(0, foreignCollectionPathWithDot.Length - 1) : string.Empty;

                    var hasLocalArray = cachedDocField.XmlMapping.HasArrayItemElement;
                    var localArrayPath = (hasLocalArray) ? (fieldPath) : currentState.Var_Object;
                    var localRefPath_NoDot = (hasLocalArray) ? (itemVar) : (fieldPath);
                    var localRefPath = (localRefPath_NoDot + ".");
                    var localRefIndent = (hasLocalArray) ? (currentIndent + "  ") : currentIndent;
                    var localRefId = (localRefPath + cachedDocField.XmlMapping.ValueElement.Name);
                    var localRefCache = (localRefPath + cachedDocField.XmlMapping.CacheElement.Name);
                    var localRefCacheId = (localRefCache + "." + GenericDatabaseUtils.DocId_Name);
                    var localNoUpdateFlag = (localRefPath + GenericDatabaseUtils.DoNotUpdate_Name);
                    var localLastUpdateDate = (localRefPath + GenericDatabaseUtils.LastUpdateDate_Name);

                    var foreignCollection = cachedDocField.CachedDocumentSource;
                    var typeMatches = subTreeState.MemberCollections.Contains(foreignCollection);
                    var typeMatchesCond = (typeMatches) ? (" && (objectStateValue.IdsByPath[\"" + foreignCollectionPath + "\"].indexOf(" + localRefId + ") < 0)") : string.Empty;
                    var foreignRootCollectionVarName = (typeMatches) ? subTreeState.RootCollection_VarName : (GenericCollection.CollectionPrefix + foreignRootCollectionName);
                    var foreignRootCollectionGetter = (typeMatches) ? string.Empty : "var " + foreignRootCollectionVarName + " = MyDb.getCollection(\"" + foreignRootCollectionName + "\");";

                    if (hasLocalArray)
                    {
                        codeDef += currentIndent + "for (var " + loopVar + " = 0; " + loopVar + " < " + localArrayPath + ".length; " + loopVar + "++) {" + NewLine;
                        codeDef += currentIndent + "  var " + itemVar + " = " + localArrayPath + "[" + loopVar + "];" + NewLine;
                        codeDef += NewLine;
                    }

                    codeDef += localRefIndent + "if (fnDecia_Utility_HasProperty(" + localRefPath_NoDot + ") && !fnDecia_Utility_HasProperty(" + localNoUpdateFlag + ")) {" + NewLine;

                    codeDef += localRefIndent + "  " + localRefCache + " = null;" + NewLine;
                    codeDef += localRefIndent + "  " + localLastUpdateDate + " = ISODate();" + NewLine;
                    codeDef += NewLine;

                    codeDef += localRefIndent + "  if (fnDecia_Utility_IsNotNull(" + localRefId + ")" + typeMatchesCond + ") {" + NewLine;
                    if (!string.IsNullOrWhiteSpace(foreignRootCollectionGetter)) { codeDef += localRefIndent + "    " + foreignRootCollectionGetter + NewLine; }
                    codeDef += localRefIndent + "    var refCursor = " + foreignRootCollectionVarName + ".find({ \"" + foreignCollectionPathWithDot + "_id\": " + localRefId + " });" + NewLine;
                    codeDef += localRefIndent + "    var refDoc = (refCursor.hasNext()) ? refCursor.next() : null;" + NewLine;
                    codeDef += NewLine;
                    codeDef += localRefIndent + "    var stopForIds = [thisModified._id];" + NewLine;
                    codeDef += localRefIndent + "    " + localRefCache + " = fnDecia_Utility_FindNestedJsonObjectForId(refDoc, " + localRefId + ", \"" + foreignCollectionPathWithDot + "\", stopForIds);" + NewLine;
                    codeDef += localRefIndent + "    " + localRefId + " = fnDecia_Utility_IsNotNull(" + localRefCache + ") ? " + localRefCacheId + " : null;" + NewLine;
                    codeDef += localRefIndent + "  }" + NewLine;

                    codeDef += localRefIndent + "}" + NewLine;

                    if (hasLocalArray)
                    {
                        codeDef += currentIndent + "}" + NewLine;
                    }

                    hasQueries = true;
                }

                foreach (var nestedColField in nestedColFields)
                {
                    if (hasQueries)
                    { codeDef += NewLine; }

                    var nestedCollection = nestedColField.NestedCollection;
                    var nestedState = subTreeState.GetState(nestedCollection);

                    var collectionName = nestedState.CollectionName;
                    var arrayPath = (currentState.Var_Object_FieldBase + collectionName);

                    nestedState.Var_Object = itemVar;

                    var nestedCode = ExportCachedDocumentSetters(dbType, nestedCollection, subTreeState, currentNestLevel + 1, currentIndent + "  ");
                    var hasNestedCode = (!string.IsNullOrWhiteSpace(nestedCode));

                    if (hasNestedCode)
                    {
                        codeDef += currentIndent + "for (var " + loopVar + " = 0; " + loopVar + " < " + arrayPath + ".length; " + loopVar + "++) {" + NewLine;
                        codeDef += currentIndent + "  var " + itemVar + " = " + arrayPath + "[" + loopVar + "];" + NewLine;
                        codeDef += NewLine;

                        codeDef += nestedCode;
                        codeDef += currentIndent + "}" + NewLine;

                        hasQueries = true;
                    }
                }
                return (hasQueries) ? codeDef : string.Empty;
            }
            else
            { throw new NotImplementedException("The specified Database Type is not supported yet."); }
        }

        private string ExportExternalDocumentUpdatersForSubTree(NoSqlDb_TargetType dbType, CollectionSubTreeState externalSubTreeState, CollectionSubTreeState localSubTreeState, string currentIndent)
        {
            var subTreeCodeDef = string.Empty;

            foreach (var externalCollection in externalSubTreeState.MemberCollections)
            {
                var collectionCodeDef = ExportExternalDocumentUpdatersForCollection(dbType, externalCollection, externalSubTreeState, localSubTreeState, currentIndent);

                if (!string.IsNullOrWhiteSpace(collectionCodeDef))
                {
                    if (string.IsNullOrWhiteSpace(subTreeCodeDef))
                    { subTreeCodeDef += collectionCodeDef; }
                    else
                    { subTreeCodeDef += NewLine + collectionCodeDef; }
                }
            }
            return subTreeCodeDef;
        }

        private string ExportExternalDocumentUpdatersForCollection(NoSqlDb_TargetType dbType, GenericCollection externalCollection, CollectionSubTreeState externalSubTreeState, CollectionSubTreeState localSubTreeState, string currentIndent)
        {
            var codeDef = string.Empty;

            if (dbType == NoSqlDb_TargetType.MongoDb)
            {
                var cachedDocFields = externalCollection.LocalFields.Where(x => ((x.FieldMode == FieldMode.CachedDocument) && (localSubTreeState.MemberCollections.Contains(x.CachedDocumentSource)) && (!Invalid_LocalCacheModes.Contains(x.LocalCacheMode)))).ToList();
                var replicatedListFields = externalCollection.LocalFields.Where(x => ((x.FieldMode == FieldMode.ReplicatedList) && (localSubTreeState.MemberCollections.Contains(x.CachedDocumentSource)) && (!Invalid_ForeignCacheModes.Contains(x.ForeignCacheMode)))).ToList();

                foreach (var cachedDocField in cachedDocFields)
                {
                    if (!string.IsNullOrWhiteSpace(codeDef))
                    { codeDef += NewLine; }

                    var externalRootCollection = externalCollection.GetRootCollection();
                    var externalCollectionPath = externalCollection.GetCollectionPathToRootAsText(true);
                    var externalPropertyName = cachedDocField.XmlMapping.RootFieldElement.Name;
                    var externalPropertyIdPath = externalPropertyName + "." + cachedDocField.XmlMapping.DocIdElement.Name;
                    var externalPropertyNoUpdatePath = externalPropertyName + "." + GenericDatabaseUtils.DoNotUpdate_Name;
                    var externalPath_ForId = externalCollectionPath + externalPropertyIdPath;
                    var externalPath_ForNoUpdate = externalCollectionPath + externalPropertyNoUpdatePath;

                    var localCollection = cachedDocField.CachedDocumentSource;
                    var localRootCollection = localCollection.GetRootCollection();
                    var localPathWithDot = localCollection.GetCollectionPathToRootAsText(true);
                    var localPath = (!string.IsNullOrWhiteSpace(localPathWithDot)) ? localPathWithDot.Substring(0, localPathWithDot.Length - 1) : localPathWithDot;

                    codeDef += currentIndent + "if (true) {" + NewLine;
                    codeDef += currentIndent + "  var externalCollection = MyDb.getCollection(\"" + externalRootCollection.XmlMapping.CollectionElement.Name + "\");" + NewLine;
                    codeDef += currentIndent + "  var externalCursor = externalCollection.find( { $and: [ { \"" + externalPath_ForNoUpdate + "\": { $exists: false } }, { \"" + externalPath_ForId + "\": { $in: objectStateValue.IdsByPath[\"" + localPath + "\"] } } ] } );" + NewLine;
                    codeDef += NewLine;
                    codeDef += currentIndent + "  while (externalCursor.hasNext()) {" + NewLine;
                    codeDef += currentIndent + "    var externalModified = externalCursor.next();" + NewLine;
                    codeDef += currentIndent + "    var externalOriginal = fnDecia_Utility_CopyJsonObject(externalModified);" + NewLine;
                    codeDef += NewLine;
                    codeDef += currentIndent + "    var stopForIds = [externalModified._id];" + NewLine;
                    codeDef += currentIndent + "    fnDecia_Utility_UpdateCachedReferences(externalModified, \"" + externalCollectionPath + "\", \"" + externalPropertyName + "\", objectStateValue, \"" + localPath + "\", stopForIds);" + NewLine;
                    codeDef += NewLine;
                    codeDef += currentIndent + "    externalModified = fnDecia_Utility_CopyJsonObject(externalModified);" + NewLine;
                    codeDef += NewLine;
                    codeDef += currentIndent + "    if (!fnDecia_Utility_AreJsonObjectsEqual(externalModified, externalOriginal)) {" + NewLine;
                    codeDef += currentIndent + "      externalCollection.save(externalModified);" + NewLine;
                    codeDef += currentIndent + "    }" + NewLine;
                    codeDef += currentIndent + "  }" + NewLine;
                    codeDef += currentIndent + "}" + NewLine;
                }

                foreach (var replicatedListField in replicatedListFields)
                {
                    if (!string.IsNullOrWhiteSpace(codeDef))
                    { codeDef += NewLine; }

                    var externalRootCollection = externalCollection.GetRootCollection();
                    var externalCollectionPath = externalCollection.GetCollectionPathToRootAsText(true);
                    var externalPropertyName = replicatedListField.XmlMapping.RootFieldElement.Name;
                    var externalPropertyIdPath = externalPropertyName + "." + replicatedListField.XmlMapping.DocIdElement.Name;
                    var externalPath_ForDocumentId = externalCollectionPath + "_id";
                    var externalPath_ForReplicatedId = externalCollectionPath + externalPropertyIdPath;

                    var typeMatches = localSubTreeState.MemberCollections.Contains(externalRootCollection);
                    var localCollection = replicatedListField.CachedDocumentSource;
                    var localRootCollection = localCollection.GetRootCollection();
                    var localPathWithDot = localCollection.GetCollectionPathToRootAsText(true);
                    var localPropertyIsArray = (replicatedListField.MemberType == MemberType.Array);
                    var localPropertyName = replicatedListField.SourceField.XmlMapping.RootFieldElement.Name;
                    var localPropertyIdPath = localPropertyName + "." + replicatedListField.SourceField.XmlMapping.DocIdElement.Name;
                    var localPath_ForDocumentId = localPathWithDot;
                    var localPath_ForRefId = localPathWithDot + localPropertyName;

                    codeDef += currentIndent + "if (true) {" + NewLine;
                    codeDef += currentIndent + "  var externalCollection = MyDb.getCollection(\"" + externalRootCollection.XmlMapping.CollectionElement.Name + "\");" + NewLine;
                    codeDef += currentIndent + "  var externalCursor = externalCollection.find( { $or: [{ \"" + externalPath_ForReplicatedId + "\": { $in: objectStateValue.IdsByPath[\"" + localPath_ForDocumentId + "\"] } }, { \"" + externalPath_ForDocumentId + "\": { $in: objectStateValue.IdsByPath[\"" + localPath_ForRefId + "\"] } }] } );" + NewLine;
                    codeDef += NewLine;
                    codeDef += currentIndent + "  while (externalCursor.hasNext()) {" + NewLine;
                    codeDef += currentIndent + "    var externalModified = externalCursor.next();" + NewLine;
                    codeDef += currentIndent + "    var externalOriginal = fnDecia_Utility_CopyJsonObject(externalModified);" + NewLine;
                    codeDef += NewLine;
                    codeDef += currentIndent + "    var stopForIds = [externalModified._id];" + NewLine;
                    codeDef += currentIndent + "    fnDecia_Utility_UpdateCachedReferences(externalModified, \"" + externalCollectionPath + "\", \"" + externalPropertyName + "\", objectStateValue, \"" + localPath_ForDocumentId + "\", stopForIds);" + NewLine;
                    codeDef += NewLine;
                    codeDef += currentIndent + "    externalModified = fnDecia_Utility_CopyJsonObject(externalModified);" + NewLine;
                    codeDef += NewLine;
                    codeDef += currentIndent + "    if (!fnDecia_Utility_AreJsonObjectsEqual(externalModified, externalOriginal)) {" + NewLine;
                    codeDef += currentIndent + "      externalCollection.save(externalModified);" + NewLine;
                    codeDef += currentIndent + "    }" + NewLine;
                    codeDef += currentIndent + "  }" + NewLine;
                    codeDef += currentIndent + "}" + NewLine;
                }

                return codeDef;
            }
            else
            { throw new NotImplementedException("The specified Database Type is not supported yet."); }
        }

        #endregion
    }
}