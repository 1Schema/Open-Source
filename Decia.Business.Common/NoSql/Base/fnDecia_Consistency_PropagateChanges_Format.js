MyDb.system.js.save(
  {
    _id: "fnDecia_Consistency_PropagateChanges",
    value: function (writeConcern) {
      var MyDb = db.getSiblingDB("<MY_DB_NAME>");
      var maxErrorCount = (<MAX_HANDL_ERROR_COUNT> >= 3) ? <MAX_HANDL_ERROR_COUNT> : 3;

      var changePropsCollection = MyDb.getCollection("Decia_PropagableChanges");
      var changePropsCursor = changePropsCollection.find({ ErrorCount: { $lt: maxErrorCount } }).sort({ LatestTimestamp: 1 });
      
      var changeItem = changePropsCursor.hasNext() ? changePropsCursor.next() : null;
      var wasSuccessful = false;

      if (fnDecia_Utility_IsNull(changeItem))
      { return true; }

      try {
        var changeHandlerName = ("fnDecia_Consistency_HandleChangeTo__" + changeItem.CollectionName + "");
        print("STARTING " + changeHandlerName + " for ObjectId = " + changeItem.ObjectId + " ...");

        wasSuccessful = eval(changeHandlerName + "(changeItem, writeConcern);")
      }
      catch (error) {
        wasSuccessful = false;
      }

      if (!wasSuccessful) {
        var currentErrorCount = (changeItem.ErrorCount + 1);
        changeItem.ErrorCount = currentErrorCount;
        changePropsCollection.save(changeItem, writeConcern);

        if (currentErrorCount < maxErrorCount)
        { print("WAITING: Must process other changes first"); }
        else
        { print("FAILED"); }
      }
      else {
        changePropsCollection.deleteOne({ _id: changeItem._id }, writeConcern);

        print("SUCCEEDED");
      }
      print("");

      return true;
    }
  }
);