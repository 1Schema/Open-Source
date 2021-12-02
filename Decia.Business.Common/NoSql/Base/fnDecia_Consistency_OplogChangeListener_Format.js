MyDb.system.js.save(
  {
    _id: "fnDecia_Consistency_OplogChangeListener",
    value: function (doChangePropagation, writeConcern) {
      doChangePropagation = (doChangePropagation == true);

      print("");
      print("Starting OpLog change listener, this function will NOT exit unless an error is encountered...");
      print("");

      var MyDb = db.getSiblingDB("<MY_DB_NAME>");
      var lastTimestampEnqueued = null;

      while (true) {
        lastTimestampEnqueued = fnDecia_Consistency_EnqueueChanges(lastTimestampEnqueued, writeConcern);

        if (doChangePropagation && fnDecia_Consistency_CanPropagateChanges()) {
          fnDecia_Consistency_PropagateChanges(writeConcern);
        }

        sleep(50);
      }
    }
  }
);