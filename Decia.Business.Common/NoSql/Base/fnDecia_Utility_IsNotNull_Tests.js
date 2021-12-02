MyDb.system.js.save(
  {
    _id: "fnDecia_Utility_IsNotNull_Tests",
    value: function (myDb) {
      var a = undefined;
      if (fnDecia_Utility_IsNotNull(a))
      { throw "a) IsNull expected."; }

      var b = null;
      if (fnDecia_Utility_IsNotNull(b))
      { throw "b) IsNull expected."; }

      var c;
      if (fnDecia_Utility_IsNotNull(c))
      { throw "c) IsNull expected."; }

      var d = 12;
      if (!fnDecia_Utility_IsNotNull(d))
      { throw "d) Not IsNull expected."; }
    }
  }
);