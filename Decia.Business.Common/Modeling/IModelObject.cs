using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Conversion;

namespace Decia.Business.Common.Modeling
{
    public interface IModelObject
    {
        ModelObjectType ModelObjectType { get; }
        Guid ModelObjectId { get; }
        bool IdIsInt { get; }
        int ModelObjectIdAsInt { get; }

        string ComplexId { get; }
    }

    public interface IDimensionalModelObject : IModelObject
    {
        Nullable<int> AlternateDimensionNumber { get; }
        int NonNullAlternateDimensionNumber { get; }
    }

    public interface IModelObjectWithRef : IModelObject
    {
        ModelObjectReference ModelObjectRef { get; }
    }

    public interface IDimensionalModelObjectWithRef : IModelObjectWithRef, IDimensionalModelObject
    { }

    public static class IModelElementUtils
    {
        public static string GetComplexId(this IModelObject modelObject)
        {
            return ConversionUtils.ConvertComplexIdToString(modelObject.ModelObjectType, modelObject.ModelObjectId);
        }

        public static string GetDimensionalComplexId(this IDimensionalModelObject modelObject)
        {
            return ConversionUtils.ConvertDimensionalComplexIdToString(modelObject.ModelObjectType, modelObject.ModelObjectId, modelObject.NonNullAlternateDimensionNumber);
        }
    }
}