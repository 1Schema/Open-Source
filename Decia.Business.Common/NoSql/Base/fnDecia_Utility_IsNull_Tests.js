MyDb.system.js.save(
  {
    _id: "fnDecia_Utility_IsNull_Tests",
    value: function (myDb) {
      var a = undefined;
      if (!fnDecia_Utility_IsNull(a))
      { throw "a) IsNull expected."; }

      var b = null;
      if (!fnDecia_Utility_IsNull(b))
      { throw "b) IsNull expected."; }

      var c;
      if (!fnDecia_Utility_IsNull(c))
      { throw "c) IsNull expected."; }

      var d = 12;
      if (fnDecia_Utility_IsNull(d))
      { throw "d) Not IsNull expected."; }
    }
  }
);