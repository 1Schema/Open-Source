MyDb.system.js.save(
  {
    _id: "fnDecia_System_OpLog_Tests",
    value: function (myDb, sysDb) {
      var oplogItemsCollection = SysDb.getCollection("oplog.rs");
      var changePropsCollection = MyDb.getCollection("Decia_PropagableChanges");

      var oplogItemsCursor = oplogItemsCollection.find();
      var lastTimestamp = null;
      var wasLastGreater = null;

      while (oplogItemsCursor.hasNext()) {
        var oplogItem = oplogItemsCursor.next();

        if (lastTimestamp != null) {
          var isGreater = (oplogItem.ts > lastTimestamp);

          if ((isGreater != wasLastGreater) && (wasLastGreater != null)) {
            var lastText = wasLastGreater ? "GREATER" : "LESSER";
            var currText = isGreater ? "GREATER" : "LESSER";
          }
          wasLastGreater = isGreater;
        }
        lastTimestamp = oplogItem.ts;
      }
    }
  }
);