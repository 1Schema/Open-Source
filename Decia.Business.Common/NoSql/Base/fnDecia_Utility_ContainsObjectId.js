MyDb.system.js.save(
  {
    _id: "fnDecia_Utility_ContainsObjectId",
    value: function (objectIdArray, desiredObjectId) {
      if (!fnDecia_Utility_IsArray(objectIdArray))
      { return false; }

      for (var z = 0; z < objectIdArray.length; z++) {
        var objectId = objectIdArray[z];

        if (fnDecia_Utility_AreJsonObjectsEqual(objectId, desiredObjectId)) {
          return true;
        }
      }
      return false;
    }
  }
);