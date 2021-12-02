MyDb.system.js.save(
  {
    _id: "fnDecia_Utility_CopyJsonObject_Tests",
    value: function (myDb) {
      var id1 = ObjectId();
      var id2 = ObjectId();
      var date1 = new Date("2016-05-18");
      var date2 = new Date("2017-11-22");

      var complexObject1 = { A: "a" };
      var complexObject2 = fnDecia_Utility_CopyJsonObject(complexObject1);
      if (!fnDecia_Utility_AreJsonObjectsEqual(complexObject1, complexObject2)) { throw "1) CopyJsonObject expected."; }

      complexObject1 = { A: "a", B: [] };
      if (fnDecia_Utility_AreJsonObjectsEqual(complexObject1, complexObject2)) { throw "2) Not CopyJsonObject expected."; }

      complexObject2 = fnDecia_Utility_CopyJsonObject(complexObject1);
      if (!fnDecia_Utility_AreJsonObjectsEqual(complexObject1, complexObject2)) { throw "3) CopyJsonObject expected."; }

      complexObject1 = { A: "a", B: [1] };
      if (fnDecia_Utility_AreJsonObjectsEqual(complexObject1, complexObject2)) { throw "4) Not CopyJsonObject expected."; }

      complexObject2 = fnDecia_Utility_CopyJsonObject(complexObject1);
      if (!fnDecia_Utility_AreJsonObjectsEqual(complexObject1, complexObject2)) { throw "5) CopyJsonObject expected."; }

      complexObject1 = { A: "a", B: [1, 2] };
      if (fnDecia_Utility_AreJsonObjectsEqual(complexObject1, complexObject2)) { throw "6) Not CopyJsonObject expected."; }

      complexObject2 = fnDecia_Utility_CopyJsonObject(complexObject1);
      if (!fnDecia_Utility_AreJsonObjectsEqual(complexObject1, complexObject2)) { throw "7) CopyJsonObject expected."; }

      complexObject1 = { A: "a", B: [1, 2, 3] };
      if (fnDecia_Utility_AreJsonObjectsEqual(complexObject1, complexObject2)) { throw "8) Not CopyJsonObject expected."; }

      complexObject2 = fnDecia_Utility_CopyJsonObject(complexObject1);
      if (!fnDecia_Utility_AreJsonObjectsEqual(complexObject1, complexObject2)) { throw "9) CopyJsonObject expected."; }

      complexObject1 = { A: "a", B: [1, 2, 3], C: { D: date1 } };
      if (fnDecia_Utility_AreJsonObjectsEqual(complexObject1, complexObject2)) { throw "10) Not CopyJsonObject expected."; }

      complexObject2 = fnDecia_Utility_CopyJsonObject(complexObject1);
      if (!fnDecia_Utility_AreJsonObjectsEqual(complexObject1, complexObject2)) { throw "11) CopyJsonObject expected."; }

      complexObject1 = { A: "a", B: [1, 2, 3], C: { D: date1, E: id1 } };
      if (fnDecia_Utility_AreJsonObjectsEqual(complexObject1, complexObject2)) { throw "12) Not CopyJsonObject expected."; }

      complexObject2 = fnDecia_Utility_CopyJsonObject(complexObject1);
      if (!fnDecia_Utility_AreJsonObjectsEqual(complexObject1, complexObject2)) { throw "13) CopyJsonObject expected."; }

      complexObject1 = { A: "a", B: [1, 2, 3], C: { D: [date1], E: [id1] } };
      if (fnDecia_Utility_AreJsonObjectsEqual(complexObject1, complexObject2)) { throw "14) Not CopyJsonObject expected."; }

      complexObject2 = fnDecia_Utility_CopyJsonObject(complexObject1);
      if (!fnDecia_Utility_AreJsonObjectsEqual(complexObject1, complexObject2)) { throw "15) CopyJsonObject expected."; }
    }
  }
);