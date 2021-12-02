MyDb.system.js.save(
  {
    _id: "fnDecia_Utility_CopyJsonObject",
    value: function (jsonObject, stopAtIds, stopAtPropertyNames) {
      stopAtIds = fnDecia_Utility_GetValueAsArray(stopAtIds);
      stopAtPropertyNames = fnDecia_Utility_GetValueAsArray(stopAtPropertyNames);

      var jsonAsString = JSON.stringify(jsonObject);
      var jsonObjectCopy = JSON.parse(jsonAsString);

      fnDecia_Utility_CopyJsonObject_Recursor({ Object: jsonObject }, { Object: jsonObjectCopy }, stopAtIds, stopAtPropertyNames, null);
      return jsonObjectCopy;
    }
  }
);

MyDb.system.js.save(
  {
    _id: "fnDecia_Utility_CopyJsonObject_Recursor",
    value: function (wrapperObject, wrapperObjectCopy, stopAtIds, stopAtPropertyNames, maxCachingDepth) {
      if (wrapperObject.Object === undefined)
      { return; }
      else if (wrapperObject.Object == null)
      { return; }

      var isObject = (wrapperObject.Object.constructor === Object);
      var isArray = fnDecia_Utility_IsArray(wrapperObject.Object);
      var isId = ((wrapperObject.Object.equals !== undefined) && (typeof wrapperObject.Object.equals === 'function'));
      var isDate = (Object.prototype.toString.call(wrapperObject.Object) === '[object Date]');

      if (isArray) {
        for (var i = 0; i < wrapperObject.Object.length; i++) {
          var nestedWrapperObject = { Object: wrapperObject.Object[i] };
          var nestedWrapperObjectCopy = { Object: wrapperObjectCopy.Object[i] };

          fnDecia_Utility_CopyJsonObject_Recursor(
            nestedWrapperObject,
            nestedWrapperObjectCopy,
            stopAtIds.slice(),
            stopAtPropertyNames,
            maxCachingDepth);

          wrapperObjectCopy.Object[i] = nestedWrapperObjectCopy.Object;
        }
      }
      else if (isObject) {
        var stopForId = false;

        if (wrapperObject.Object.hasOwnProperty("_id")) {
          var objectId = wrapperObject.Object._id;

          if (fnDecia_Utility_IsNotNull(objectId)) {
            stopForId = fnDecia_Utility_ContainsObjectId(stopAtIds, objectId);
            stopAtIds.push(objectId);
          }
        }

        if (fnDecia_Utility_IsNull(maxCachingDepth) && wrapperObject.Object.hasOwnProperty("OS_MAX_CACHING_DEPTH")) {
          maxCachingDepth = wrapperObject.Object.OS_MAX_CACHING_DEPTH;
        }

        for (var propName in wrapperObject.Object) {
          var propValue = wrapperObject.Object[propName];
          var propStopNames = stopAtPropertyNames.slice();
          var propValue = wrapperObject.Object[propName];
          var propIsDescendable = false;
          var stopForName = false;

          if (fnDecia_Utility_IsNotNull(propValue)) {
            propIsDescendable = (fnDecia_Utility_IsArray(propValue) || (propValue.constructor === Object));

            if (propIsDescendable && (propName != "OS_CACHED_DOC")) {
              stopForName = (propStopNames.indexOf(propName) >= 0);
              propStopNames.push(propName);
            }
            else if (fnDecia_Utility_IsNotNull(maxCachingDepth) && (propName == "OS_CACHED_DOC")) {
              stopForId = (stopForId || (maxCachingDepth < 1));
              maxCachingDepth--;
            }
          }

          var nestedWrapperObject = { Object: wrapperObject.Object[propName] };
          var nestedWrapperObjectCopy = { Object: wrapperObjectCopy.Object[propName] };

          if (stopForName && propIsDescendable) {
            delete wrapperObjectCopy.Object[propName];
          }
          else {
            if (stopForId && (propName == "OS_CACHED_DOC")) {
              nestedWrapperObjectCopy.Object = null;
            }
            else {
              fnDecia_Utility_CopyJsonObject_Recursor(
                nestedWrapperObject,
                nestedWrapperObjectCopy,
                stopAtIds.slice(),
                propStopNames,
                maxCachingDepth);
            }

            wrapperObjectCopy.Object[propName] = nestedWrapperObjectCopy.Object;
          }
        }
      }
      else if (isId) {
        wrapperObjectCopy.Object = eval(wrapperObject.Object.toString());
      }
      else if (isDate) {
        wrapperObjectCopy.Object = new Date(wrapperObject.Object.getTime());
      }
    }
  }
);