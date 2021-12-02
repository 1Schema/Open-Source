using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Conversion;

namespace Decia.Business.Common.NoSql.Functions
{
    public class CollectionSubTreeState
    {
        private GenericCollection m_RootCollection;
        private string m_RootCollection_VarName;
        private string m_RootDocument_VarName;
        private Dictionary<GenericCollection, CollectionState> m_SubTreeState;

        public CollectionSubTreeState(GenericCollection rootCollection, string rootCollectionVarName, string rootDocumentVarName)
        {
            if (rootCollection == null)
            { throw new InvalidOperationException("The Root Collection must not be null."); }
            if (string.IsNullOrWhiteSpace(rootCollectionVarName))
            { throw new InvalidOperationException("The Root Collection Var Name must not be null."); }
            if (string.IsNullOrWhiteSpace(rootDocumentVarName))
            { throw new InvalidOperationException("The Root Document Var Name must not be null."); }

            m_RootCollection = rootCollection;
            m_RootCollection_VarName = rootCollectionVarName;
            m_RootDocument_VarName = rootDocumentVarName;
            m_SubTreeState = new Dictionary<GenericCollection, CollectionState>();

            var subTreeMembers = RootCollection.GetCollectionSubTree(true);
            foreach (var member in subTreeMembers)
            {
                var state = new CollectionState(this, member);
                m_SubTreeState.Add(state.Collection, state);
            }

            m_SubTreeState[RootCollection].Var_Object = RootDocument_VarName;
        }

        public GenericCollection RootCollection { get { return m_RootCollection; } }
        public string RootCollectionName { get { return RootCollection.XmlMapping.CollectionElement.Name; } }
        public string RootCollection_VarName { get { return m_RootCollection_VarName; } }
        public string RootDocument_VarName { get { return m_RootDocument_VarName; } }
        public string RootDocument_FieldBase { get { return (RootDocument_VarName + "."); } }

        public Dictionary<GenericCollection, CollectionState> SubTreeCollectionStates { get { return new Dictionary<GenericCollection, CollectionState>(m_SubTreeState); } }
        public List<GenericCollection> MemberCollections { get { return new List<GenericCollection>(m_SubTreeState.Keys); } }

        public List<string> Var_AllPaths { get { return SubTreeCollectionStates.Values.Select(x => x.Var_Path_NoDot).ToList(); } }
        public string Var_AllPaths_AsString { get { return Var_AllPaths.ConvertToCollectionAsString(","); } }
        public string Var_AllPaths_AsArray { get { return "[" + Var_AllPaths_AsString + "]"; } }

        public List<string> Var_AllPaths_PlusRefPaths
        {
            get
            {
                var completeList = Var_AllPaths;

                foreach (var collectionState in SubTreeCollectionStates.Values)
                {
                    var refIdFields = collectionState.Collection.LocalFields.Where(x => x.FieldMode == FieldMode.CachedDocument).ToList();

                    foreach (var refIdField in refIdFields)
                    {
                        var collectionPath = collectionState.CollectionPath;
                        var refIdFieldName = refIdField.XmlMapping.RootFieldElement.Name;

                        var refIdPath = (!string.IsNullOrWhiteSpace(collectionPath)) ? (collectionPath + refIdFieldName) : refIdFieldName;
                        completeList.Add("\"" + refIdPath + "\"");
                    }
                }
                return completeList;
            }
        }
        public string Var_AllPaths_PlusRefPaths_AsString { get { return Var_AllPaths_PlusRefPaths.ConvertToCollectionAsString(","); } }
        public string Var_AllPaths_PlusRefPaths_AsArray { get { return "[" + Var_AllPaths_PlusRefPaths_AsString + "]"; } }

        public CollectionState GetState(GenericCollection collection)
        {
            return m_SubTreeState[collection];
        }
    }

    public class CollectionState
    {
        private CollectionSubTreeState m_SubTreeState;
        private GenericCollection m_Collection;
        private List<GenericCollection> m_Path;

        public CollectionState(CollectionSubTreeState subTreeState, GenericCollection collection)
        {
            if (subTreeState == null)
            { throw new InvalidOperationException("The Sub-Tree State must not be null."); }
            if (collection == null)
            { throw new InvalidOperationException("The Collection must not be null."); }

            m_SubTreeState = subTreeState;
            m_Collection = collection;

            string rootCollectionName, nestedCollectionPath;
            m_Path = Collection.GetCollectionPathToRoot(true, out rootCollectionName, out nestedCollectionPath);

            CollectionPath = nestedCollectionPath;
            Var_Object = null;
        }

        public GenericCollection Collection { get { return m_Collection; } }
        public List<GenericCollection> Path { get { return new List<GenericCollection>(m_Path); } }

        public string CollectionName { get { return Collection.XmlMapping.CollectionElement.Name; } }
        public string CollectionPath { get; protected set; }

        public string Var_Path { get { return "\"" + CollectionPath + "\""; } }
        public string Var_Path_NoDot { get { return "\"" + (CollectionPath.EndsWith(".") ? CollectionPath.Substring(0, CollectionPath.Length - 1) : CollectionPath) + "\""; } }
        public string Var_Object { get; set; }
        public string Var_Object_FieldBase { get { return (Var_Object + "."); } }
        public string Var_Object_IdField { get { return (Var_Object_FieldBase + "_id"); } }
    }
}