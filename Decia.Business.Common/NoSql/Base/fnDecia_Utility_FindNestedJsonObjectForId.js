MyDb.system.js.save(
  {
    _id: "fnDecia_Utility_FindNestedJsonObjectForId",
    value: function (rootObject, desiredId, pathToNestedObject, stopAtIds) {
      stopAtIds = fnDecia_Utility_GetValueAsArray(stopAtIds);

      var hasMorePath = false;
      var nextPathPart = null;

      if (fnDecia_Utility_IsNotNull(pathToNestedObject)) {
        if (pathToNestedObject.constructor === String)
        { pathToNestedObject = pathToNestedObject.split("."); }

        if (pathToNestedObject.length > 0) {
          nextPathPart = pathToNestedObject[0];
          pathToNestedObject.shift();

          if (nextPathPart != "")
          { hasMorePath = true; }
        }
      }

      if (!hasMorePath) {
        var idsMatch = fnDecia_Utility_AreJsonObjectsEqual(desiredId, rootObject._id);

        if (!idsMatch)
        { return null; }
        else
        { return fnDecia_Utility_CopyJsonObject(rootObject, stopAtIds); }
      }

      var nextPath = "rootObject." + nextPathPart;
      var nextObject = eval(nextPath);

      var isObject = (nextObject.constructor === Object);
      var isArray = fnDecia_Utility_IsArray(nextObject);

      if (isObject) {
        if (rootObject.hasOwnProperty("_id")) {
          var objectId = rootObject._id;

          if (fnDecia_Utility_IsNotNull(objectId))
          { stopAtIds.push(objectId); }
        }

        return fnDecia_Utility_FindNestedJsonObjectForId(nextObject, desiredId, pathToNestedObject, stopAtIds.slice());
      }
      else if (isArray) {
        for (var i = 0; i < nextObject.length; i++) {
          var arrayItem = nextObject[i];

          var result = fnDecia_Utility_FindNestedJsonObjectForId(arrayItem, desiredId, pathToNestedObject, stopAtIds.slice());

          if (result != null)
          { return result; }
        }
        return null;
      }
      else {
        throw "Unexpected type encountered.";
      }
    }
  }
);