MyDb.system.js.save(
  {
    _id: "fnDecia_Utility_UpdateCachedReferences",
    value: function (rootObject, pathToNestedObject, refPropertyName, objectStateValue, pathKeyForState, stopAtIds) {
      stopAtIds = fnDecia_Utility_GetValueAsArray(stopAtIds);

      fnDecia_Utility_UpdateCachedReferences_Recursor(rootObject, rootObject, pathToNestedObject, refPropertyName, objectStateValue, pathKeyForState, stopAtIds)
    }
  }
);
MyDb.system.js.save(
  {
    _id: "fnDecia_Utility_UpdateCachedReferences_Recursor",
    value: function (currentObject, rootObject, pathToNestedObject, refPropertyName, objectStateValue, pathKeyForState, stopAtIds) {
      var hasMorePath = false;
      var nextPathPart = null;
      var separator = "__";

      if (fnDecia_Utility_IsNull(currentObject))
      { return; }

      if (currentObject.hasOwnProperty("_id")) {
        var objectId = currentObject._id;

        if (fnDecia_Utility_IsNotNull(objectId))
        { stopAtIds.push(objectId); }
      }

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
        var propertyPath = "currentObject." + refPropertyName;
        var propertyObject = eval(propertyPath);
        var validIds = objectStateValue.IdsByPath[pathKeyForState];

        if (fnDecia_Utility_IsArray(propertyObject)) {
          var idsHandled = new Array();
          var indexesToRemove = new Array();

          for (var i = 0; i < propertyObject.length; i++) {
            var arrayItem = propertyObject[i];
            var refId = arrayItem._id;

            idsHandled.push(refId);
            var isRelevant = fnDecia_Utility_ContainsObjectId(validIds, refId);

            if (isRelevant) {
              var stateForId = objectStateValue.StatesByPathAndId[pathKeyForState + separator + refId];

              if (stateForId.IsDeleted) {
                indexesToRemove.push(i);
              }
              else {
                propertyObject.OS_CACHED_DOC = null;
                propertyObject.OS_LAST_UPDATE_DATE = ISODate();

                if (!fnDecia_Utility_AreJsonObjectsEqual(refId, rootObject._id)) {
                  stopAtIds.push(refId);
                  arrayItem.OS_CACHED_DOC = fnDecia_Utility_CopyJsonObject(stateForId.Object, stopAtIds);
                }
              }
            }
          }

          for (var j = (indexesToRemove.length - 1) ; j >= 0; j--) {
            var index = indexesToRemove[j];
            propertyObject.splice(index, 1);
          }

          for (var i = 0; i < validIds.length; i++) {
            var refId = validIds[i];

            if (fnDecia_Utility_ContainsObjectId(idsHandled, refId))
            { continue; }

            var stateForId = objectStateValue.StatesByPathAndId[pathKeyForState + separator + refId];

            if (stateForId.IsDeleted)
            { continue; }

            var newDate = ISODate();
            var newObject = { _id: refId, OS_CACHED_DOC: null, OS_LAST_UPDATE_DATE: newDate };
            propertyObject.push(newObject);

            if (!fnDecia_Utility_AreJsonObjectsEqual(refId, rootObject._id)) {
              stopAtIds.push(refId);
              newObject.OS_CACHED_DOC = fnDecia_Utility_CopyJsonObject(stateForId.Object, stopAtIds);
            }
          }
        }
        else {
          var refId = propertyObject._id;
          var isRelevant = fnDecia_Utility_ContainsObjectId(validIds, refId);

          if (isRelevant) {
            var stateForId = objectStateValue.StatesByPathAndId[pathKeyForState + separator + refId];

            if (stateForId.IsDeleted) {
              propertyObject._id = null;
              propertyObject.OS_CACHED_DOC = null;
              propertyObject.OS_LAST_UPDATE_DATE = ISODate();
            }
            else {
              propertyObject.OS_CACHED_DOC = null;
              propertyObject.OS_LAST_UPDATE_DATE = ISODate();

              if (!fnDecia_Utility_AreJsonObjectsEqual(refId, rootObject._id)) {
                stopAtIds.push(refId);
                propertyObject.OS_CACHED_DOC = fnDecia_Utility_CopyJsonObject(stateForId.Object, stopAtIds);
              }
            }
          }
        }
      }
      else {
        var nextPath = "currentObject." + nextPathPart;
        var nextObject = eval(nextPath);

        if (fnDecia_Utility_IsArray(nextObject)) {
          for (var i = 0; i < nextObject.length; i++) {
            var arrayItem = nextObject[i];
            fnDecia_Utility_UpdateCachedReferences_Recursor(arrayItem, rootObject, pathToNestedObject, refPropertyName, objectStateValue, pathKeyForState, stopAtIds.slice());
          }
        }
        else {
          fnDecia_Utility_UpdateCachedReferences_Recursor(nextObject, rootObject, pathToNestedObject, refPropertyName, objectStateValue, pathKeyForState, stopAtIds.slice());
        }
      }
    }
  }
);