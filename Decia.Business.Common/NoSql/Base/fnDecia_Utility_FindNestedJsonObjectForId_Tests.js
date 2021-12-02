MyDb.system.js.save(
  {
    _id: "fnDecia_Utility_FindNestedJsonObjectForId_Tests",
    value: function (myDb) {
      var id1 = ObjectId();
      var id2 = ObjectId();
      var id3 = ObjectId();
      var id4 = ObjectId();


      var rootDocument = { _id: id1 };
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id1, "");
      if (!fnDecia_Utility_AreJsonObjectsEqual(rootDocument, nestedDocument)) { throw "1) FindNestedJsonObjectForId expected."; }
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id2, "");
      if (!fnDecia_Utility_AreJsonObjectsEqual(null, nestedDocument)) { throw "2) Not FindNestedJsonObjectForId expected."; }


      var rootDocument = {
        _id: id1,
        A: {
          _id: id1,
          C: { _id: id1 }
        },
        B: {
          _id: id1,
          D: { _id: id1 }
        }
      };
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id1, "");
      if (!fnDecia_Utility_AreJsonObjectsEqual(rootDocument, nestedDocument)) { throw "3) FindNestedJsonObjectForId expected."; }
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id1, "A");
      if (!fnDecia_Utility_AreJsonObjectsEqual(rootDocument.A, nestedDocument)) { throw "4) FindNestedJsonObjectForId expected."; }
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id1, "B");
      if (!fnDecia_Utility_AreJsonObjectsEqual(rootDocument.B, nestedDocument)) { throw "5) FindNestedJsonObjectForId expected."; }
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id1, "A.C");
      if (!fnDecia_Utility_AreJsonObjectsEqual(rootDocument.A.C, nestedDocument)) { throw "6) FindNestedJsonObjectForId expected."; }
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id1, "B.D");
      if (!fnDecia_Utility_AreJsonObjectsEqual(rootDocument.B.D, nestedDocument)) { throw "7) FindNestedJsonObjectForId expected."; }
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id2, "");
      if (!fnDecia_Utility_AreJsonObjectsEqual(null, nestedDocument)) { throw "8) Not FindNestedJsonObjectForId expected."; }
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id2, "A");
      if (!fnDecia_Utility_AreJsonObjectsEqual(null, nestedDocument)) { throw "9) Not FindNestedJsonObjectForId expected."; }
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id2, "B");
      if (!fnDecia_Utility_AreJsonObjectsEqual(null, nestedDocument)) { throw "10) Not FindNestedJsonObjectForId expected."; }
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id2, "A.C");
      if (!fnDecia_Utility_AreJsonObjectsEqual(null, nestedDocument)) { throw "11) Not FindNestedJsonObjectForId expected."; }
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id2, "B.D");
      if (!fnDecia_Utility_AreJsonObjectsEqual(null, nestedDocument)) { throw "12) Not FindNestedJsonObjectForId expected."; }

      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id1, ".");
      if (!fnDecia_Utility_AreJsonObjectsEqual(rootDocument, nestedDocument)) { throw "13) FindNestedJsonObjectForId expected."; }
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id1, "A.");
      if (!fnDecia_Utility_AreJsonObjectsEqual(rootDocument.A, nestedDocument)) { throw "14) FindNestedJsonObjectForId expected."; }
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id1, "B.");
      if (!fnDecia_Utility_AreJsonObjectsEqual(rootDocument.B, nestedDocument)) { throw "15) FindNestedJsonObjectForId expected."; }
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id1, "A.C.");
      if (!fnDecia_Utility_AreJsonObjectsEqual(rootDocument.A.C, nestedDocument)) { throw "16) FindNestedJsonObjectForId expected."; }
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id1, "B.D.");
      if (!fnDecia_Utility_AreJsonObjectsEqual(rootDocument.B.D, nestedDocument)) { throw "17) FindNestedJsonObjectForId expected."; }
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id2, ".");
      if (!fnDecia_Utility_AreJsonObjectsEqual(null, nestedDocument)) { throw "18) Not FindNestedJsonObjectForId expected."; }
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id2, "A.");
      if (!fnDecia_Utility_AreJsonObjectsEqual(null, nestedDocument)) { throw "19) Not FindNestedJsonObjectForId expected."; }
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id2, "B.");
      if (!fnDecia_Utility_AreJsonObjectsEqual(null, nestedDocument)) { throw "20) Not FindNestedJsonObjectForId expected."; }
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id2, "A.C.");
      if (!fnDecia_Utility_AreJsonObjectsEqual(null, nestedDocument)) { throw "21) Not FindNestedJsonObjectForId expected."; }
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id2, "B.D.");
      if (!fnDecia_Utility_AreJsonObjectsEqual(null, nestedDocument)) { throw "22) Not FindNestedJsonObjectForId expected."; }


      var rootDocument = {
        _id: id1,
        A: [
          {
            _id: id1,
            C: [{ _id: id1 }, { _id: id2 }, { _id: id3 }]
          },
          { _id: id2 },
          { _id: id3 }
        ],
        B: [
          {
            _id: id1,
            D: [{ _id: id1 }, { _id: id2 }, { _id: id3 }]
          },
          { _id: id2 },
          { _id: id3 }
        ]
      };
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id1, "");
      if (!fnDecia_Utility_AreJsonObjectsEqual(rootDocument, nestedDocument)) { throw "23) FindNestedJsonObjectForId expected."; }
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id2, "");
      if (!fnDecia_Utility_AreJsonObjectsEqual(null, nestedDocument)) { throw "24) Not FindNestedJsonObjectForId expected."; }

      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id2, "A");
      if (!fnDecia_Utility_AreJsonObjectsEqual(rootDocument.A[1], nestedDocument)) { throw "25) FindNestedJsonObjectForId expected."; }
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id2, "B");
      if (!fnDecia_Utility_AreJsonObjectsEqual(rootDocument.B[1], nestedDocument)) { throw "26) FindNestedJsonObjectForId expected."; }
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id2, "A.C");
      if (!fnDecia_Utility_AreJsonObjectsEqual(rootDocument.A[0].C[1], nestedDocument)) { throw "27) FindNestedJsonObjectForId expected."; }
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id2, "B.D");
      if (!fnDecia_Utility_AreJsonObjectsEqual(rootDocument.B[0].D[1], nestedDocument)) { throw "28) FindNestedJsonObjectForId expected."; }
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id4, "");
      if (!fnDecia_Utility_AreJsonObjectsEqual(null, nestedDocument)) { throw "29) Not FindNestedJsonObjectForId expected."; }
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id4, "A");
      if (!fnDecia_Utility_AreJsonObjectsEqual(null, nestedDocument)) { throw "30) Not FindNestedJsonObjectForId expected."; }
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id4, "B");
      if (!fnDecia_Utility_AreJsonObjectsEqual(null, nestedDocument)) { throw "31) Not FindNestedJsonObjectForId expected."; }
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id4, "A.C");
      if (!fnDecia_Utility_AreJsonObjectsEqual(null, nestedDocument)) { throw "32) Not FindNestedJsonObjectForId expected."; }
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id4, "B.D");
      if (!fnDecia_Utility_AreJsonObjectsEqual(null, nestedDocument)) { throw "33) Not FindNestedJsonObjectForId expected."; }

      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id1, ".");
      if (!fnDecia_Utility_AreJsonObjectsEqual(rootDocument, nestedDocument)) { throw "34) FindNestedJsonObjectForId expected."; }
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id2, ".");
      if (!fnDecia_Utility_AreJsonObjectsEqual(null, nestedDocument)) { throw "35) Not FindNestedJsonObjectForId expected."; }

      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id2, "A.");
      if (!fnDecia_Utility_AreJsonObjectsEqual(rootDocument.A[1], nestedDocument)) { throw "36) FindNestedJsonObjectForId expected."; }
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id2, "B.");
      if (!fnDecia_Utility_AreJsonObjectsEqual(rootDocument.B[1], nestedDocument)) { throw "37) FindNestedJsonObjectForId expected."; }
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id2, "A.C.");
      if (!fnDecia_Utility_AreJsonObjectsEqual(rootDocument.A[0].C[1], nestedDocument)) { throw "38) FindNestedJsonObjectForId expected."; }
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id2, "B.D.");
      if (!fnDecia_Utility_AreJsonObjectsEqual(rootDocument.B[0].D[1], nestedDocument)) { throw "39) FindNestedJsonObjectForId expected."; }
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id4, ".");
      if (!fnDecia_Utility_AreJsonObjectsEqual(null, nestedDocument)) { throw "40) Not FindNestedJsonObjectForId expected."; }
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id4, "A.");
      if (!fnDecia_Utility_AreJsonObjectsEqual(null, nestedDocument)) { throw "41) Not FindNestedJsonObjectForId expected."; }
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id4, "B.");
      if (!fnDecia_Utility_AreJsonObjectsEqual(null, nestedDocument)) { throw "42) Not FindNestedJsonObjectForId expected."; }
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id4, "A.C.");
      if (!fnDecia_Utility_AreJsonObjectsEqual(null, nestedDocument)) { throw "43) Not FindNestedJsonObjectForId expected."; }
      var nestedDocument = fnDecia_Utility_FindNestedJsonObjectForId(rootDocument, id4, "B.D.");
      if (!fnDecia_Utility_AreJsonObjectsEqual(null, nestedDocument)) { throw "44) Not FindNestedJsonObjectForId expected."; }
    }
  }
);