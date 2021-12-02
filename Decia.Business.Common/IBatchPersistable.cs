using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Decia.Business.Common
{
    public enum BatchState
    {
        Read = 0x0001,
        Added = 0x0010,
        Updated = 0x0100,
        Removed = 0x1000,
    }

    public interface IBatchPersistable
    {
        BatchState BatchState { get; set; }
    }

    public static class IBatchPersistableUtils
    {
        public static readonly BatchState DefaultBatchState = BatchState.Read;

        public static void SetBatchStateToAdded(this IBatchPersistable obj)
        {
            obj.BatchState = BatchState.Added;
        }

        public static void SetBatchStateToUpdated(this IBatchPersistable obj)
        {
            obj.BatchState = BatchState.Updated;
        }

        public static void SetBatchStateToRemoved(this IBatchPersistable obj)
        {
            obj.BatchState = BatchState.Removed;
        }

        public static DataRowState ConvertToRowState(this BatchState batchState)
        {
            if (batchState == BatchState.Read)
            { return DataRowState.Unchanged; }
            else if (batchState == BatchState.Added)
            { return DataRowState.Added; }
            else if (batchState == BatchState.Updated)
            { return DataRowState.Modified; }
            else if (batchState == BatchState.Removed)
            { return DataRowState.Deleted; }
            else
            { throw new InvalidOperationException("Unrecognized BatchState encountered."); }
        }

        public static DataRowState GetRowState(this IBatchPersistable obj)
        {
            return ConvertToRowState(obj.BatchState);
        }

        public static void ApplyBatchState(this DataRow dataRow, BatchState batchState)
        {
            if (batchState == BatchState.Read)
            { return; }
            else if (batchState == BatchState.Added)
            { dataRow.SetAdded(); }
            else if (batchState == BatchState.Updated)
            { dataRow.SetModified(); }
            else if (batchState == BatchState.Removed)
            { dataRow.Delete(); }
            else
            { throw new InvalidOperationException("Unsupported BatchState encountered."); }
        }

        public static void ApplyBatchState(this DataRow dataRow, IBatchPersistable obj)
        {
            ApplyBatchState(dataRow, obj.BatchState);
        }

        public static IEnumerable<T> GetValuesForState<T>(this IEnumerable<T> values, BatchState desiredState)
            where T : IBatchPersistable
        {
            if (values == null)
            { return new List<T>(); }

            var matchingValues = values.Where(x => x.BatchState.Bitwise_IsSet<BatchState>(desiredState)).ToList();
            return matchingValues;
        }
    }
}