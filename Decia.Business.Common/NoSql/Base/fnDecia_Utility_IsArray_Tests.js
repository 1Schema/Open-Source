MyDb.system.js.save(
  {
    _id: "fnDecia_Utility_IsArray_Tests",
    value: function (myDb) {
      var a = undefined;
      if (fnDecia_Utility_IsArray(a))
      { throw "a) Not IsArray expected."; }

      var b = null;
      if (fnDecia_Utility_IsArray(b))
      { throw "b) Not IsArray expected."; }

      var c;
      if (fnDecia_Utility_IsArray(c))
      { throw "c) Not IsArray expected."; }

      var d = 12;
      if (fnDecia_Utility_IsArray(d))
      { throw "d) Not IsArray expected."; }

      var e = "asdds";
      if (fnDecia_Utility_IsArray(e))
      { throw "e) Not IsArray expected."; }

      var f = { Text: "hello" };
      if (fnDecia_Utility_IsArray(f))
      { throw "f) Not IsArray expected."; }

      var g = ["hello"];
      if (!fnDecia_Utility_IsArray(g))
      { throw "g) IsArray expected."; }

      var h = [{ Text: "hello" }];
      if (!fnDecia_Utility_IsArray(h))
      { throw "h) IsArray expected."; }

      var i = { A1: [{ Text: "hello" }] };
      if (fnDecia_Utility_IsArray(i))
      { throw "f) Not IsArray expected."; }
      if (!fnDecia_Utility_IsArray(i.A1))
      { throw "i) IsArray expected."; }
      if (fnDecia_Utility_IsArray(i.A2))
      { throw "f) Not IsArray expected."; }
    }
  }
);