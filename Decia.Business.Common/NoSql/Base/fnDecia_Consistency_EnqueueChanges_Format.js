MyDb.system.js.save(
  {
    _id: "fnDecia_Consistency_EnqueueChanges",
    value: function (lastTimestampEnqueued, writeConcern) {
      var includeOpLogItemsForDebugging = true;

      var SysDb = db.getSiblingDB("<SYS_DB_NAME>");
      var MyDb = db.getSiblingDB("<MY_DB_NAME>");

      var oplogItemsCollection = SysDb.getCollection("oplog.rs");
      var changePropsCollection = MyDb.getCollection("Decia_PropagableChanges");

      var isLastTimestampNull = fnDecia_Utility_IsNull(lastTimestampEnqueued);
      var oplogItemsToEnqueueCursor = null;

      if (isLastTimestampNull)
      { oplogItemsToEnqueueCursor = oplogItemsCollection.find(); }
      else
      { oplogItemsToEnqueueCursor = oplogItemsCollection.find({ ts: { $gt: lastTimestampEnqueued } }); }

      while (oplogItemsToEnqueueCursor.hasNext()) {
        var itemToEnqueue = oplogItemsToEnqueueCursor.next();

        var itemNamespace = itemToEnqueue.ns;
        var itemAction = itemToEnqueue.op;
        var itemTimestamp = itemToEnqueue.ts;

        var itemNamespaceParts = itemNamespace.split(".");
        var itemDatabaseName = (itemNamespaceParts.length > 0) ? itemNamespaceParts[0] : null;
        var itemCollectionName = (itemNamespaceParts.length > 1) ? itemNamespaceParts[1] : null;

        if (!fnDecia_Consistency_CanEnqueueChanges(itemDatabaseName, itemCollectionName, itemAction)) {
          lastTimestampEnqueued = itemTimestamp;
          continue;
        }

        var hasObject2 = (fnDecia_Utility_HasProperty(itemToEnqueue.o2));
        var itemObjectForId = (hasObject2) ? itemToEnqueue.o2 : itemToEnqueue.o;
        var itemObjectId = itemObjectForId._id;

        var itemObject = itemToEnqueue.o;
        if (hasObject2) {
          var usesSysFuncInObject = false;

          for (var propName in itemObject) {
            if (propName[0] == "$") {
              usesSysFuncInObject = true;
              break;
            }
          }

          if (usesSysFuncInObject) {
            itemObject = MyDb.getCollection(itemCollectionName).findOne({ _id: itemObjectId });

            if (fnDecia_Utility_IsNull(itemObject))
            { itemObject = { _id: itemObjectId }; }

            itemToEnqueue.o = itemObject;
          }
        }

        var changeItemId = itemCollectionName + "_" + itemObjectId;
        var changePropsCursor = changePropsCollection.find({ _id: { $eq: changeItemId } });
        var changeItem = null;

        if (!changePropsCursor.hasNext())
        { changeItem = { _id: changeItemId, CollectionName: itemCollectionName, ObjectId: itemObjectId, LatestTimestamp: itemTimestamp, ErrorCount: 0 }; }
        else
        { changeItem = changePropsCursor.next(); }

        if (changeItem.LatestTimestamp < itemTimestamp) {
          changeItem.LatestTimestamp = itemTimestamp;
          changeItem.ErrorCount = 0;
        }

        if (itemAction == "i") {
          changeItem.IsCreated = true;
          changeItem.CreatedObject = itemObject;

          if (includeOpLogItemsForDebugging)
          { changeItem.CreatedLogItem = itemToEnqueue; }
        }
        else if (itemAction == "u") {
          changeItem.IsUpdated = true;
          changeItem.UpdatedObject = itemObject;

          if (includeOpLogItemsForDebugging)
          { changeItem.UpdatedLogItem = itemToEnqueue; }
        }
        else if (itemAction == "d") {
          changeItem.IsDeleted = true;
          changeItem.DeletedObject = itemObject;

          if (includeOpLogItemsForDebugging)
          { changeItem.DeletedLogItem = itemToEnqueue; }
        }
        else {
          throw "Unexpected change action encountered."
        }

        changePropsCollection.save(changeItem, writeConcern);
        lastTimestampEnqueued = itemTimestamp;
      }

      return lastTimestampEnqueued;
    }
  }
);