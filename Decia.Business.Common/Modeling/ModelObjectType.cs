using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Modeling
{
    public enum ModelObjectType
    {
        None = 0,

        ModelTemplate,
        EntityTypesList,
        RelationTypesList,
        ScenarioTypesList,
        BaseUnitType,
        TimeType,
        GlobalType,
        EntityType,
        RelationType,
        ScenarioType,
        VariableTemplate,
        AnonymousVariableTemplate,

        ModelInstance,
        TimeInstance,
        GlobalInstance,
        EntityInstance,
        RelationInstance,
        ScenarioInstance,
        VariableInstance,
        AnonymousVariableInstance,

        ReportTemplate,
        ReportElementTemplate,

        Unit,
        TimeMatrix,
        Formula,
        Operation,
        Parameter,
    }
}