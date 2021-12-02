MyDb.system.js.save(
  {
    _id: "fnDecia_Utility_GetValueAsArray",
    value: function (value) {
      if (fnDecia_Utility_IsNull(value))
      { return new Array(); }

      if (fnDecia_Utility_IsArray(value))
      { return value; }

      var valueAsArray = new Array();
      valueAsArray.push(value);
      return valueAsArray;
    }
  }
);