MyDb.system.js.save(
  {
    _id: "fnDecia_Utility_AreJsonObjectsEqual_Tests",
    value: function (myDb) {
      var id1 = ObjectId();
      var id2 = ObjectId();
      var date1 = new Date("2016-05-18");
      var date2 = new Date("2017-11-22");

      var object_Values = [undefined, null, 1, 2, "a", "b", id1, id2, date1, date2];

      for (var i = 0; i < object_Values.length; i++) {
        var object1 = object_Values[i];

        for (var j = 0; j < object_Values.length; j++) {
          var object2 = object_Values[j];

          var name = ("" + i + "_" + j);
          var shouldBeEqual = (i == j);
          var areEqual = fnDecia_Utility_AreJsonObjectsEqual(object1, object2);

          if (shouldBeEqual != areEqual) {
            if (shouldBeEqual)
            { throw name + ") AreJsonObjectsEqual expected."; }
            else
            { throw name + ") Not AreJsonObjectsEqual expected."; }
          }
        }
      }

      var complexObject1 = { A: "a" };
      var complexObject2 = { A: "a" };
      if (!fnDecia_Utility_AreJsonObjectsEqual(complexObject1, complexObject2)) { throw "1) AreJsonObjectsEqual expected."; }

      complexObject1 = { A: "a", B: [] };
      if (fnDecia_Utility_AreJsonObjectsEqual(complexObject1, complexObject2)) { throw "2) Not AreJsonObjectsEqual expected."; }

      complexObject2 = { A: "a", B: [] };
      if (!fnDecia_Utility_AreJsonObjectsEqual(complexObject1, complexObject2)) { throw "3) AreJsonObjectsEqual expected."; }

      complexObject1 = { A: "a", B: [1] };
      if (fnDecia_Utility_AreJsonObjectsEqual(complexObject1, complexObject2)) { throw "4) Not AreJsonObjectsEqual expected."; }

      complexObject2 = { A: "a", B: [1] };
      if (!fnDecia_Utility_AreJsonObjectsEqual(complexObject1, complexObject2)) { throw "5) AreJsonObjectsEqual expected."; }

      complexObject1 = { A: "a", B: [1, 2] };
      if (fnDecia_Utility_AreJsonObjectsEqual(complexObject1, complexObject2)) { throw "6) Not AreJsonObjectsEqual expected."; }

      complexObject2 = { A: "a", B: [1, 2] };
      if (!fnDecia_Utility_AreJsonObjectsEqual(complexObject1, complexObject2)) { throw "7) AreJsonObjectsEqual expected."; }

      complexObject1 = { A: "a", B: [1, 2, 3] };
      if (fnDecia_Utility_AreJsonObjectsEqual(complexObject1, complexObject2)) { throw "8) Not AreJsonObjectsEqual expected."; }

      complexObject2 = { A: "a", B: [1, 3, 2] };
      if (fnDecia_Utility_AreJsonObjectsEqual(complexObject1, complexObject2)) { throw "9) Not AreJsonObjectsEqual expected."; }
      complexObject2 = { A: "a", B: [1, 2, 3] };
      if (!fnDecia_Utility_AreJsonObjectsEqual(complexObject1, complexObject2)) { throw "10) AreJsonObjectsEqual expected."; }

      complexObject1 = { A: "a", B: [1, 2, 3], C: { D: date1 } };
      if (fnDecia_Utility_AreJsonObjectsEqual(complexObject1, complexObject2)) { throw "11) Not AreJsonObjectsEqual expected."; }

      complexObject2 = { A: "a", C: { D: date1 }, B: [1, 2, 3] };
      if (!fnDecia_Utility_AreJsonObjectsEqual(complexObject1, complexObject2)) { throw "12) AreJsonObjectsEqual expected."; }
      complexObject2 = { A: "a", B: [1, 2, 3], C: { D: date1 } };
      if (!fnDecia_Utility_AreJsonObjectsEqual(complexObject1, complexObject2)) { throw "13) AreJsonObjectsEqual expected."; }

      complexObject1 = { A: "a", B: [1, 2, 3], C: { D: date1, E: [id1] } };
      if (fnDecia_Utility_AreJsonObjectsEqual(complexObject1, complexObject2)) { throw "14) Not AreJsonObjectsEqual expected."; }

      complexObject2 = { A: "a", B: [1, 2, 3], C: { E: [id1], D: date1 } };
      if (!fnDecia_Utility_AreJsonObjectsEqual(complexObject1, complexObject2)) { throw "15) AreJsonObjectsEqual expected."; }
      complexObject2 = { A: "a", B: [1, 2, 3], C: { D: date1, E: [id1] } };
      if (!fnDecia_Utility_AreJsonObjectsEqual(complexObject1, complexObject2)) { throw "16) AreJsonObjectsEqual expected."; }

      complexObject1 = { A: "a", B: [1, 2, 3], C: { D: date1 } };
      if (fnDecia_Utility_AreJsonObjectsEqual(complexObject1, complexObject2)) { throw "17) Not AreJsonObjectsEqual expected."; }

      complexObject2 = { A: "a", B: [1, 2, 3], C: { D: date1 } };
      if (!fnDecia_Utility_AreJsonObjectsEqual(complexObject1, complexObject2)) { throw "18) AreJsonObjectsEqual expected."; }

      complexObject1 = { A: "a", C: { D: date1 } };
      if (fnDecia_Utility_AreJsonObjectsEqual(complexObject1, complexObject2)) { throw "19) Not AreJsonObjectsEqual expected."; }

      complexObject2 = { A: "a", C: { D: date1 } };
      if (!fnDecia_Utility_AreJsonObjectsEqual(complexObject1, complexObject2)) { throw "20) AreJsonObjectsEqual expected."; }
    }
  }
);