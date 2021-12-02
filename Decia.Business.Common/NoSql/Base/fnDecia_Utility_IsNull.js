MyDb.system.js.save(
  {
    _id: "fnDecia_Utility_IsNull",
    value: function (value) {
      if (value == undefined) {
        return true;
      }
      if (value == null) {
        return true;
      }
      return false;
    }
  }
);