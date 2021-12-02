using System.Data;
using System.Data.SqlClient;

namespace Decia.Business.Common.Sql.Base
{
    [System.ComponentModel.DesignerCategory("Code")]
    public class JUNK_SolutionExplorer_TypeFix
    { }

    public partial class DeciaBase_DataSet : DataSet, System.ComponentModel.IComponent
    { }
}

namespace Decia.Business.Common.Sql.Base.DeciaBase_DataSetTableAdapters
{
    public partial class Decia_MetadataTableAdapter { public SqlDataAdapter InnerAdapter { get { return Adapter; } } }
    public partial class Decia_TimeDimensionSettingTableAdapter { public SqlDataAdapter InnerAdapter { get { return Adapter; } } }
    public partial class Decia_TimePeriodTypeTableAdapter { public SqlDataAdapter InnerAdapter { get { return Adapter; } } }
    public partial class Decia_DataTypeTableAdapter { public SqlDataAdapter InnerAdapter { get { return Adapter; } } }
    public partial class Decia_ObjectTypeTableAdapter { public SqlDataAdapter InnerAdapter { get { return Adapter; } } }
    public partial class Decia_TimePeriodTableAdapter { public SqlDataAdapter InnerAdapter { get { return Adapter; } } }
    public partial class Decia_StructuralTypeTableAdapter { public SqlDataAdapter InnerAdapter { get { return Adapter; } } }
    public partial class Decia_VariableTemplateTableAdapter { public SqlDataAdapter InnerAdapter { get { return Adapter; } } }
    public partial class Decia_VariableTemplateDependencyTableAdapter { public SqlDataAdapter InnerAdapter { get { return Adapter; } } }
    public partial class Decia_VariableTemplateGroupTableAdapter { public SqlDataAdapter InnerAdapter { get { return Adapter; } } }
    public partial class Decia_VariableTemplateGroupMemberTableAdapter { public SqlDataAdapter InnerAdapter { get { return Adapter; } } }
    public partial class Decia_ResultSetTableAdapter { public SqlDataAdapter InnerAdapter { get { return Adapter; } } }
    public partial class Decia_ResultSetTimeDimensionSettingTableAdapter { public SqlDataAdapter InnerAdapter { get { return Adapter; } } }
    public partial class Decia_ResultSetProcessingMemberTableAdapter { public SqlDataAdapter InnerAdapter { get { return Adapter; } } }
}