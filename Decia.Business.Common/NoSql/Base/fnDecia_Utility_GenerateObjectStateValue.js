MyDb.system.js.save(
  {
    _id: "fnDecia_Utility_GenerateObjectStateValue",
    value: function (currentObject, originalObject, pathsToNestedCollections, isForDeletion) {
      pathsToNestedCollections = fnDecia_Utility_IsNotNull(pathsToNestedCollections) ? pathsToNestedCollections : [];
      isForDeletion = fnDecia_Utility_IsNotNull(isForDeletion) ? isForDeletion : false;

      var basePath = "";
      var idPropName = "_id";
      var separator = "__";

      var objectStateValue = { IdsByPath: {}, StatesByPathAndId: {} };
      objectStateValue.IdsByPath[basePath] = new Array();

      if ((fnDecia_Utility_IsNull(currentObject)) || (!currentObject.hasOwnProperty(idPropName)))
      { return objectStateValue; }

      var rootObjectId = currentObject[idPropName];
      objectStateValue.IdsByPath[basePath] = [rootObjectId];
      objectStateValue.StatesByPathAndId[basePath + separator + rootObjectId] = { Object: currentObject, IsDeleted: isForDeletion };

      fnDecia_Utility_GenerateObjectStateValue_Recursor(currentObject, originalObject, basePath, objectStateValue, pathsToNestedCollections, isForDeletion);
      return objectStateValue;
    }
  }
);
MyDb.system.js.save(
  {
    _id: "fnDecia_Utility_GenerateObjectStateValue_Recursor",
    value: function (currentObject, originalObject, basePath, objectStateValue, pathsToNestedCollections, isForDeletion) {
      if (fnDecia_Utility_IsNull(currentObject))
      { return; }
      if (fnDecia_Utility_IsNull(objectStateValue))
      { throw "The object state value must not be null."; }
      if (fnDecia_Utility_IsNull(pathsToNestedCollections))
      { throw "The paths to nested collections must not be null."; }

      var idPropName = "_id";
      var separator = "__";

      var hasCurrentObject = fnDecia_Utility_IsNotNull(currentObject);
      var hasOriginalObject = fnDecia_Utility_IsNotNull(originalObject);

      var propertyNames = {};
      if (hasCurrentObject) {
        for (var propertyName in currentObject) {
          propertyNames[propertyName] = 1;
        }
      }
      if (hasOriginalObject) {
        for (var propertyName in originalObject) {
          propertyNames[propertyName] = 1;
        }
      }

      for (var propertyName in propertyNames) {
        var currentPropertyValue = hasCurrentObject ? currentObject[propertyName] : null;
        var originalPropertyValue = hasOriginalObject ? originalObject[propertyName] : null;

        var hasCurrentPropertyValue = fnDecia_Utility_IsNotNull(currentPropertyValue);
        var hasOriginalPropertyValue = fnDecia_Utility_IsNotNull(originalPropertyValue);

        var listPath = ((basePath != null) && (basePath != "")) ? (basePath + "." + propertyName) : propertyName;
        if (pathsToNestedCollections.indexOf(listPath) < 0)
        { continue; }


        if (fnDecia_Utility_IsNull(objectStateValue.IdsByPath[listPath])) {
          objectStateValue.IdsByPath[listPath] = new Array();
        }


        if (hasCurrentPropertyValue && !fnDecia_Utility_IsArray(currentPropertyValue)) {
          if (currentPropertyValue.hasOwnProperty("_id")) {
            var currentItem = currentPropertyValue;
            currentPropertyValue = new Array();
            currentPropertyValue.push(currentItem);
          }
          else {
            hasCurrentPropertyValue = false;
          }
        }
        if (hasOriginalPropertyValue && !fnDecia_Utility_IsArray(originalPropertyValue)) {
          if (originalPropertyValue.hasOwnProperty("_id")) {
            var originalItem = originalPropertyValue;
            originalPropertyValue = new Array();
            originalPropertyValue.push(originalItem);
          }
          else {
            hasOriginalPropertyValue = false;
          }
        }


        for (var i = 0; (hasCurrentPropertyValue) && (i < currentPropertyValue.length) ; i++) {
          var currentNestedObject = currentPropertyValue[i];
          var originalNestedObject = null;
          var nestedIsDeleted = isForDeletion;

          if ((fnDecia_Utility_IsNull(currentNestedObject)) || (!currentNestedObject.hasOwnProperty(idPropName)))
          { continue; }

          for (var j = 0; (hasOriginalPropertyValue) && (j < originalPropertyValue.length) ; j++) {
            var prospectiveOriginalObject = originalPropertyValue[j];

            if ((fnDecia_Utility_IsNull(prospectiveOriginalObject)) || (!prospectiveOriginalObject.hasOwnProperty(idPropName)))
            { continue; }

            if (currentNestedObject[idPropName] == prospectiveOriginalObject[idPropName]) {
              originalNestedObject = prospectiveOriginalObject;
              break;
            }
          }

          var nestedObjectId = currentNestedObject[idPropName];
          objectStateValue.IdsByPath[listPath].push(nestedObjectId);
          objectStateValue.StatesByPathAndId[listPath + separator + nestedObjectId] = { Object: currentNestedObject, IsDeleted: nestedIsDeleted };

          fnDecia_Utility_GenerateObjectStateValue_Recursor(currentNestedObject, originalNestedObject, listPath, objectStateValue, pathsToNestedCollections, isForDeletion);
        }

        for (var j = 0; (hasOriginalPropertyValue) && (j < originalPropertyValue.length) ; j++) {
          var originalNestedObject = originalPropertyValue[j];

          if ((fnDecia_Utility_IsNull(originalNestedObject)) || (!originalNestedObject.hasOwnProperty(idPropName)))
          { continue; }

          var nestedObjectId = originalNestedObject[idPropName];

          if (objectStateValue.IdsByPath[listPath].indexOf(nestedObjectId) >= 0)
          { continue; }

          objectStateValue.IdsByPath[listPath].push(nestedObjectId);
          objectStateValue.StatesByPathAndId[listPath + separator + nestedObjectId] = { Object: originalNestedObject, IsDeleted: true };
        }
      }
    }
  }
);