MyDb.system.js.save(
  {
    _id: "fnDecia_ChangeState_ResetLatest",
    value: function (writeConcern) {
      var MyDb = db.getSiblingDB("<MY_DB_NAME>");

      var metadataCollection = MyDb.getCollection("Decia_Metadatas");
      var metadataCursor = metadataCollection.find();

      var hasMetadata = metadataCursor.hasNext();
      var metadata = hasMetadata ? metadataCursor.next() : { _id: 1 };

      var newChangeCount = 0;
      var newChangeDate = Date.now();

      metadata.Latest_ChangeCount = newChangeCount;
      metadata.Latest_ChangeDate = newChangeDate;

      metadataCollection.save(metadata, writeConcern);
    }
  }
);