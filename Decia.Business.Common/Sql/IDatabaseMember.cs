using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Exports;

namespace Decia.Business.Common.Sql
{
    public interface IDatabaseMember
    {
        Guid Id { get; }
        string Name { get; }

        string ExportSchema(SqlDb_TargetType dbType);
        string ExportProgrammatics(SqlDb_TargetType dbType);
    }
}