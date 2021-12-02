MyDb.system.js.save(
  {
    _id: "fnDecia_ChangeState_GetLatest",
    value: function () {
      var MyDb = db.getSiblingDB("<MY_DB_NAME>");

      var metadataCollection = MyDb.getCollection("Decia_Metadatas");
      var metadataCursor = metadataCollection.find();

      var hasMetadata = metadataCursor.hasNext();
      var metadata = hasMetadata ? metadataCursor.next() : null;

      var changeCount = hasMetadata ? metadata.Latest_ChangeCount : 0;
      var changeDate = hasMetadata ? metadata.Latest_ChangeDate : Date.now();

      var changeState = { Latest_ChangeCount: changeCount, Latest_ChangeDate: changeDate };
      return changeState;
    }
  }
);