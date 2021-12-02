MyDb.system.js.save(
  {
    _id: "fnDecia_Utility_IsArray",
    value: function (value) {
      if (fnDecia_Utility_IsNull(value))
      { return false; }

      var isArray = (Object.prototype.toString.call(value) === "[object Array]");
      return isArray;
    }
  }
);