MyDb.system.js.save(
  {
    _id: "fnDecia_Consistency_CanEnqueueChanges",
    value: function (databaseName, collectionName, actionName) {
      var myDbName = "<MY_DB_NAME>";
      var relevantCollectionNames = ",,<REL_COL_NAMES>,,";
      var relevantActionNames = ",,<REL_ACT_NAMES>,,";

      if (databaseName != myDbName)
      { return false; }

      var collectionSplitter = "," + collectionName + ",";
      var collectionParts = relevantCollectionNames.split(collectionSplitter);

      if (collectionParts.length < 2)
      { return false; }
      if (collectionParts[0].length < 1)
      { return false; }
      if (collectionParts[1].length < 1)
      { return false; }

      var actionSplitter = "," + actionName + ",";
      var actionParts = relevantActionNames.split(actionSplitter);

      if (actionParts.length < 2)
      { return false; }
      if (actionParts[0].length < 1)
      { return false; }
      if (actionParts[1].length < 1)
      { return false; }

      return true;
    }
  }
);