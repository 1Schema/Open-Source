using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.NoSql
{
    public interface IDatabaseMember
    {
        Guid Id { get; }
        string Name { get; }
    }
}