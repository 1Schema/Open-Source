using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.DataStructures;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Structure;

namespace Decia.Business.Common.Dependencies
{
    public partial interface IDependencyMap
    {
        #region Properties

        bool IsValid { get; }
        ModelObjectReference ModelTemplateRef { get; }
        IEnumerable<ModelObjectReference> VariableTemplateRefs { get; }
        IEnumerable<ModelObjectReference> StructuralTypeRefs { get; }
        INetwork<ModelObjectReference> VariableTypeNetwork { get; }

        #endregion

        #region General Methods

        ModelObjectReference GetStructuralType(ModelObjectReference variableTemplateRef);
        ModelObjectReference GetIdVariableTemplate(ModelObjectReference structuralTypeRef);
        ModelObjectReference GetNameVariableTemplate(ModelObjectReference structuralTypeRef);
        ModelObjectReference GetOrderVariableTemplate(ModelObjectReference structuralTypeRef);
        IEnumerable<ModelObjectReference> GetVariableTemplates(ModelObjectReference structuralTypeRef);
        IEnumerable<IEdge<ModelObjectReference>> GetReducedEdges(IEnumerable<IEdge<ModelObjectReference>> edgesToIgnore);

        #endregion

        #region Navigation Methods

        IList<ModelObjectReference> GetImpliedNavigationVariableReferences(IStructuralMap structuralMap, ModelObjectReference variableTemplateRef);
        IList<ModelObjectReference> GetImpliedNavigationVariableReferences(IStructuralMap structuralMap, ModelObjectReference variableTemplateRef, bool useExtendedStructure);
        void AddImpliedNavigationDependencies(IStructuralMap structuralMap);
        void AddImpliedNavigationDependencies(IStructuralMap structuralMap, bool useExtendedStructure);

        #endregion

        #region Computation Methods

        IList<ICycleGroup<ModelObjectReference>> GetCycleGroups();
        IList<ICycleGroup<ModelObjectReference>> GetCycleGroups(IEnumerable<IEdge<ModelObjectReference>> edgesToIgnore);
        IDictionary<Guid, IComputationGroup> GetComputationGroups();
        IDictionary<Guid, IComputationGroup> GetComputationGroups(IEnumerable<IEdge<ModelObjectReference>> edgesToIgnore);
        INetwork<Guid> GetComputationNetwork();
        INetwork<Guid> GetComputationNetwork(IEnumerable<IEdge<ModelObjectReference>> edgesToIgnoreForGrouping);
        INetwork<Guid> GetComputationNetwork(IEnumerable<IEdge<ModelObjectReference>> edgesToIgnoreForGrouping, bool useAllEdgesInNetwork);

        #endregion
    }
}