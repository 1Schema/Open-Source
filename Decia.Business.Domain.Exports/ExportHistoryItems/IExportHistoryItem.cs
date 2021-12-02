using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using DomainDriver.CommonUtilities.Collections;

namespace Decia.Business.Domain.Exports
{
    public interface IExportHistoryItem<T> : IProjectMember<T>, IExportHistoryItem
        where T : IExportHistoryItem<T>
    { }

    public interface IExportHistoryItem : IProjectMember, IPermissionable, IKeyedObject<ExportHistoryItemId>
    { }
}