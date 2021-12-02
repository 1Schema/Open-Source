MyDb.system.js.save(
  {
    _id: "fnDecia_Consistency_CanPropagateChanges",
    value: function () {
      var MyDb = db.getSiblingDB("<MY_DB_NAME>");

      var changePropsCollection = MyDb.getCollection("Decia_PropagableChanges");
      var changePropsCursor = changePropsCollection.find();
      var changeCount = changePropsCursor.count();

      if (changeCount < 1)
      { return false; }

      var maxConnUtilRate = <MAX_CONNECTION_UTILIZATION>;
      var maxQueueSize = <MAX_QUEUE_SIZE>;
      var maxActiveSize = <MAX_ACTIVE_SIZE>;
      var maxCursorCount = <MAX_CURSOR_COUNT>;

      var status = db.serverStatus();
      var connUtilRate = (status.connections.current / (status.connections.current + status.connections.available));
      var queueSize = status.globalLock.currentQueue.total;
      var activeSize = status.globalLock.activeClients.total;
      var openCursorCount = status.metrics.cursor.open.total;

      var doPropagation = ((maxConnUtilRate > connUtilRate) &&
                         (maxQueueSize > queueSize) &&
                         (maxActiveSize > activeSize) &&
                         (maxCursorCount > openCursorCount));
      return doPropagation;
    }
  }
);