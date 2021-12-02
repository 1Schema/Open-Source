MyDb.system.js.save(
  {
    _id: "fnDecia_Utility_AreJsonObjectsEqual",
    value: function (jsonObject1, jsonObject2) {
      if (!fnDecia_Utility_HasProperty(jsonObject1) || !fnDecia_Utility_HasProperty(jsonObject2)) {
        return (!fnDecia_Utility_HasProperty(jsonObject1) && !fnDecia_Utility_HasProperty(jsonObject2));
      }
      if (fnDecia_Utility_IsNull(jsonObject1) || fnDecia_Utility_IsNull(jsonObject2)) {
        return (fnDecia_Utility_IsNull(jsonObject1) && fnDecia_Utility_IsNull(jsonObject2));
      }

      var isObject = (jsonObject1.constructor === Object);
      var isArray = fnDecia_Utility_IsArray(jsonObject1);
      var isId = ((jsonObject1.equals !== undefined) && (typeof jsonObject1.equals === 'function'));
      var isDate = (Object.prototype.toString.call(jsonObject1) === '[object Date]');

      if (isObject) {
        if (!(jsonObject2.constructor === Object))
        { return false; }

        for (var propName in jsonObject1) {
          if (!jsonObject2.hasOwnProperty(propName))
          { return false; }
        }
        for (var propName in jsonObject2) {
          if (!jsonObject1.hasOwnProperty(propName))
          { return false; }
        }

        for (var propName in jsonObject1) {
          var nestedJsonObject1 = jsonObject1[propName];
          var nestedJsonObject2 = jsonObject2[propName];

          if (propName == "OS_LAST_UPDATE_DATE")
          { continue; }

          var areEqual = fnDecia_Utility_AreJsonObjectsEqual(
              nestedJsonObject1,
              nestedJsonObject2);

          if (!areEqual)
          { return false; }
        }
        return true;
      }
      else if (isArray) {
        if (!fnDecia_Utility_IsArray(jsonObject2))
        { return false; }
        if (jsonObject1.length != jsonObject2.length)
        { return false; }

        for (var i = 0; i < jsonObject1.length; i++) {
          var nestedJsonObject1 = jsonObject1[i];
          var nestedJsonObject2 = jsonObject2[i];

          var areEqual = fnDecia_Utility_AreJsonObjectsEqual(
              nestedJsonObject1,
              nestedJsonObject2);

          if (!areEqual)
          { return false; }
        }
        return true;
      }
      else if (isId) {
        if (!((jsonObject2.equals !== undefined) && (typeof jsonObject2.equals === 'function')))
        { return false; }
        return jsonObject1.equals(jsonObject2);
      }
      else if (isDate) {
        if (!(Object.prototype.toString.call(jsonObject2) === '[object Date]'))
        { return false; }
        return (String(jsonObject1.getTime()) == String(jsonObject2.getTime()));
      }
      else {
        if ((jsonObject2.equals !== undefined) && (typeof jsonObject2.equals === 'function'))
        { return (jsonObject1 == jsonObject2.valueOf()); }

        if (Object.prototype.toString.call(jsonObject2) === '[object Date]')
        { return (jsonObject1 == String(jsonObject2.getTime())); }

        return (jsonObject1 == jsonObject2);
      }
    }
  }
);