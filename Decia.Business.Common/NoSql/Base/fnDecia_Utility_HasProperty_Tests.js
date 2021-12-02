MyDb.system.js.save(
  {
    _id: "fnDecia_Utility_HasProperty_Tests",
    value: function (myDb) {
      var obj = { B: null, C: 1, D: "d", E: [2] };

      if (fnDecia_Utility_HasProperty(obj.A))
      { throw "a) Not HasProperty expected."; }

      if (!fnDecia_Utility_HasProperty(obj.B))
      { throw "b) HasProperty expected."; }

      if (!fnDecia_Utility_HasProperty(obj.C))
      { throw "c) HasProperty expected."; }

      if (!fnDecia_Utility_HasProperty(obj.D))
      { throw "d) HasProperty expected."; }

      if (!fnDecia_Utility_HasProperty(obj.E))
      { throw "e) HasProperty expected."; }
    }
  }
);