MyDb.system.js.save(
  {
    _id: "fnDecia_Utility_GenerateObjectStateValue_Tests",
    value: function (myDb) {
      var id1 = ObjectId();
      var id2 = ObjectId();
      var id3 = ObjectId();
      var id4 = ObjectId();

      var id1_Val = id1.valueOf();
      var id2_Val = id2.valueOf();
      var id3_Val = id3.valueOf();
      var id4_Val = id4.valueOf();


      var rootDocument = { _id: id1 };
      var objectStateValue = fnDecia_Utility_GenerateObjectStateValue(rootDocument, null, [], false);

      if (objectStateValue.IdsByPath[""].indexOf(id1) < 0) { throw "1) GenerateObjectStateValue IdsByPath expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["__" + id1_Val].Object, rootDocument)) { throw "2) GenerateObjectStateValue StatesByPathAndId Object expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["__" + id1_Val].IsDeleted, false)) { throw "3) GenerateObjectStateValue StatesByPathAndId IsDeleted expected."; }
      if (objectStateValue.IdsByPath[""].indexOf(id2) >= 0) { throw "4) Not GenerateObjectStateValue IdsByPath expected."; }

      var objectStateValue = fnDecia_Utility_GenerateObjectStateValue(rootDocument, null, [""], false);

      if (objectStateValue.IdsByPath[""].indexOf(id1) < 0) { throw "5) GenerateObjectStateValue IdsByPath expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["__" + id1_Val].Object, rootDocument)) { throw "6) GenerateObjectStateValue StatesByPathAndId Object expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["__" + id1_Val].IsDeleted, false)) { throw "7) GenerateObjectStateValue StatesByPathAndId IsDeleted expected."; }
      if (objectStateValue.IdsByPath[""].indexOf(id2) >= 0) { throw "8) Not GenerateObjectStateValue IdsByPath expected."; }


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
      var objectStateValue = fnDecia_Utility_GenerateObjectStateValue(rootDocument, null, ["A", "A.C", "B", "B.D"], false);

      if (objectStateValue.IdsByPath[""].indexOf(id1) < 0) { throw "9) GenerateObjectStateValue IdsByPath expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["__" + id1_Val].Object, rootDocument)) { throw "10) GenerateObjectStateValue StatesByPathAndId Object expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["__" + id1_Val].IsDeleted, false)) { throw "11) GenerateObjectStateValue StatesByPathAndId IsDeleted expected."; }
      if (objectStateValue.IdsByPath[""].indexOf(id2) >= 0) { throw "12) Not GenerateObjectStateValue IdsByPath expected."; }

      if (!objectStateValue.IdsByPath.hasOwnProperty("A") || (objectStateValue.IdsByPath["A"].length > 1)) { throw "13) Single GenerateObjectStateValue IdsByPath expected."; }
      if (!objectStateValue.IdsByPath.hasOwnProperty("A.C") || (objectStateValue.IdsByPath["A.C"].length > 1)) { throw "14) Single GenerateObjectStateValue IdsByPath expected."; }
      if (!objectStateValue.IdsByPath.hasOwnProperty("B") || (objectStateValue.IdsByPath["B"].length > 1)) { throw "15) Single GenerateObjectStateValue IdsByPath expected."; }
      if (!objectStateValue.IdsByPath.hasOwnProperty("B.D") || (objectStateValue.IdsByPath["B.D"].length > 1)) { throw "16) Single GenerateObjectStateValue IdsByPath expected."; }


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
      var objectStateValue = fnDecia_Utility_GenerateObjectStateValue(rootDocument, null, ["A", "A.C", "B", "B.D"], false);

      if (objectStateValue.IdsByPath[""].indexOf(id1) < 0) { throw "17) GenerateObjectStateValue IdsByPath expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["__" + id1_Val].Object, rootDocument)) { throw "18) GenerateObjectStateValue StatesByPathAndId Object expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["__" + id1_Val].IsDeleted, false)) { throw "19) GenerateObjectStateValue StatesByPathAndId IsDeleted expected."; }
      if (objectStateValue.IdsByPath[""].indexOf(id2) >= 0) { throw "20) Not GenerateObjectStateValue IdsByPath expected."; }

      if (!objectStateValue.IdsByPath.hasOwnProperty("A")) { throw "21) GenerateObjectStateValue IdsByPath expected."; }
      if (objectStateValue.IdsByPath["A"].indexOf(id1) < 0) { throw "22) GenerateObjectStateValue IdsByPath expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["A__" + id1_Val].Object, rootDocument.A[0])) { throw "23) GenerateObjectStateValue StatesByPathAndId Object expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["A__" + id1_Val].IsDeleted, false)) { throw "24) GenerateObjectStateValue StatesByPathAndId IsDeleted expected."; }
      if (objectStateValue.IdsByPath["A"].indexOf(id2) < 0) { throw "25) GenerateObjectStateValue IdsByPath expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["A__" + id2_Val].Object, rootDocument.A[1])) { throw "26) GenerateObjectStateValue StatesByPathAndId Object expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["A__" + id2_Val].IsDeleted, false)) { throw "27) GenerateObjectStateValue StatesByPathAndId IsDeleted expected."; }
      if (objectStateValue.IdsByPath["A"].indexOf(id3) < 0) { throw "28) GenerateObjectStateValue IdsByPath expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["A__" + id3_Val].Object, rootDocument.A[2])) { throw "29) GenerateObjectStateValue StatesByPathAndId Object expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["A__" + id3_Val].IsDeleted, false)) { throw "30) GenerateObjectStateValue StatesByPathAndId IsDeleted expected."; }
      if (objectStateValue.IdsByPath["A"].indexOf(id4) >= 0) { throw "31) Not GenerateObjectStateValue IdsByPath expected."; }

      if (!objectStateValue.IdsByPath.hasOwnProperty("A.C")) { throw "32) GenerateObjectStateValue IdsByPath expected."; }
      if (objectStateValue.IdsByPath["A.C"].indexOf(id1) < 0) { throw "33) GenerateObjectStateValue IdsByPath expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["A.C__" + id1_Val].Object, rootDocument.A[0].C[0])) { throw "34) GenerateObjectStateValue StatesByPathAndId Object expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["A.C__" + id1_Val].IsDeleted, false)) { throw "35) GenerateObjectStateValue StatesByPathAndId IsDeleted expected."; }
      if (objectStateValue.IdsByPath["A.C"].indexOf(id2) < 0) { throw "36) GenerateObjectStateValue IdsByPath expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["A.C__" + id2_Val].Object, rootDocument.A[0].C[1])) { throw "37) GenerateObjectStateValue StatesByPathAndId Object expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["A.C__" + id2_Val].IsDeleted, false)) { throw "38) GenerateObjectStateValue StatesByPathAndId IsDeleted expected."; }
      if (objectStateValue.IdsByPath["A.C"].indexOf(id3) < 0) { throw "39) GenerateObjectStateValue IdsByPath expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["A.C__" + id3_Val].Object, rootDocument.A[0].C[2])) { throw "40) GenerateObjectStateValue StatesByPathAndId Object expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["A.C__" + id3_Val].IsDeleted, false)) { throw "41) GenerateObjectStateValue StatesByPathAndId IsDeleted expected."; }
      if (objectStateValue.IdsByPath["A.C"].indexOf(id4) >= 0) { throw "42) Not GenerateObjectStateValue IdsByPath expected."; }

      if (!objectStateValue.IdsByPath.hasOwnProperty("B")) { throw "43) GenerateObjectStateValue IdsByPath expected."; }
      if (objectStateValue.IdsByPath["B"].indexOf(id1) < 0) { throw "44) GenerateObjectStateValue IdsByPath expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["B__" + id1_Val].Object, rootDocument.B[0])) { throw "45) GenerateObjectStateValue StatesByPathAndId Object expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["B__" + id1_Val].IsDeleted, false)) { throw "46) GenerateObjectStateValue StatesByPathAndId IsDeleted expected."; }
      if (objectStateValue.IdsByPath["B"].indexOf(id2) < 0) { throw "47) GenerateObjectStateValue IdsByPath expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["B__" + id2_Val].Object, rootDocument.B[1])) { throw "48) GenerateObjectStateValue StatesByPathAndId Object expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["B__" + id2_Val].IsDeleted, false)) { throw "49) GenerateObjectStateValue StatesByPathAndId IsDeleted expected."; }
      if (objectStateValue.IdsByPath["B"].indexOf(id3) < 0) { throw "50) GenerateObjectStateValue IdsByPath expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["B__" + id3_Val].Object, rootDocument.B[2])) { throw "51) GenerateObjectStateValue StatesByPathAndId Object expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["B__" + id3_Val].IsDeleted, false)) { throw "52) GenerateObjectStateValue StatesByPathAndId IsDeleted expected."; }
      if (objectStateValue.IdsByPath["B"].indexOf(id4) >= 0) { throw "53) Not GenerateObjectStateValue IdsByPath expected."; }

      if (!objectStateValue.IdsByPath.hasOwnProperty("B.D")) { throw "54) GenerateObjectStateValue IdsByPath expected."; }
      if (objectStateValue.IdsByPath["B.D"].indexOf(id1) < 0) { throw "55) GenerateObjectStateValue IdsByPath expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["B.D__" + id1_Val].Object, rootDocument.B[0].D[0])) { throw "56) GenerateObjectStateValue StatesByPathAndId Object expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["B.D__" + id1_Val].IsDeleted, false)) { throw "57) GenerateObjectStateValue StatesByPathAndId IsDeleted expected."; }
      if (objectStateValue.IdsByPath["B.D"].indexOf(id2) < 0) { throw "58) GenerateObjectStateValue IdsByPath expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["B.D__" + id2_Val].Object, rootDocument.B[0].D[1])) { throw "59) GenerateObjectStateValue StatesByPathAndId Object expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["B.D__" + id2_Val].IsDeleted, false)) { throw "60) GenerateObjectStateValue StatesByPathAndId IsDeleted expected."; }
      if (objectStateValue.IdsByPath["B.D"].indexOf(id3) < 0) { throw "61) GenerateObjectStateValue IdsByPath expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["B.D__" + id3_Val].Object, rootDocument.B[0].D[2])) { throw "62) GenerateObjectStateValue StatesByPathAndId Object expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["B.D__" + id3_Val].IsDeleted, false)) { throw "63) GenerateObjectStateValue StatesByPathAndId IsDeleted expected."; }
      if (objectStateValue.IdsByPath["B.D"].indexOf(id4) >= 0) { throw "64) Not GenerateObjectStateValue IdsByPath expected."; }


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
      var objectStateValue = fnDecia_Utility_GenerateObjectStateValue(rootDocument, null, ["A", "A.C", "B", "B.D"], true);

      if (objectStateValue.IdsByPath[""].indexOf(id1) < 0) { throw "65) GenerateObjectStateValue IdsByPath expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["__" + id1_Val].Object, rootDocument)) { throw "66) GenerateObjectStateValue StatesByPathAndId Object expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["__" + id1_Val].IsDeleted, true)) { throw "67) GenerateObjectStateValue StatesByPathAndId IsDeleted expected."; }
      if (objectStateValue.IdsByPath[""].indexOf(id2) >= 0) { throw "68) Not GenerateObjectStateValue IdsByPath expected."; }

      if (!objectStateValue.IdsByPath.hasOwnProperty("A")) { throw "69) GenerateObjectStateValue IdsByPath expected."; }
      if (objectStateValue.IdsByPath["A"].indexOf(id1) < 0) { throw "70) GenerateObjectStateValue IdsByPath expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["A__" + id1_Val].Object, rootDocument.A[0])) { throw "71) GenerateObjectStateValue StatesByPathAndId Object expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["A__" + id1_Val].IsDeleted, true)) { throw "72) GenerateObjectStateValue StatesByPathAndId IsDeleted expected."; }
      if (objectStateValue.IdsByPath["A"].indexOf(id2) < 0) { throw "73) GenerateObjectStateValue IdsByPath expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["A__" + id2_Val].Object, rootDocument.A[1])) { throw "74) GenerateObjectStateValue StatesByPathAndId Object expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["A__" + id2_Val].IsDeleted, true)) { throw "75) GenerateObjectStateValue StatesByPathAndId IsDeleted expected."; }
      if (objectStateValue.IdsByPath["A"].indexOf(id3) < 0) { throw "76) GenerateObjectStateValue IdsByPath expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["A__" + id3_Val].Object, rootDocument.A[2])) { throw "77) GenerateObjectStateValue StatesByPathAndId Object expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["A__" + id3_Val].IsDeleted, true)) { throw "78) GenerateObjectStateValue StatesByPathAndId IsDeleted expected."; }
      if (objectStateValue.IdsByPath["A"].indexOf(id4) >= 0) { throw "79) Not GenerateObjectStateValue IdsByPath expected."; }

      if (!objectStateValue.IdsByPath.hasOwnProperty("A.C")) { throw "80) GenerateObjectStateValue IdsByPath expected."; }
      if (objectStateValue.IdsByPath["A.C"].indexOf(id1) < 0) { throw "81) GenerateObjectStateValue IdsByPath expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["A.C__" + id1_Val].Object, rootDocument.A[0].C[0])) { throw "82) GenerateObjectStateValue StatesByPathAndId Object expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["A.C__" + id1_Val].IsDeleted, true)) { throw "83) GenerateObjectStateValue StatesByPathAndId IsDeleted expected."; }
      if (objectStateValue.IdsByPath["A.C"].indexOf(id2) < 0) { throw "84) GenerateObjectStateValue IdsByPath expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["A.C__" + id2_Val].Object, rootDocument.A[0].C[1])) { throw "85) GenerateObjectStateValue StatesByPathAndId Object expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["A.C__" + id2_Val].IsDeleted, true)) { throw "86) GenerateObjectStateValue StatesByPathAndId IsDeleted expected."; }
      if (objectStateValue.IdsByPath["A.C"].indexOf(id3) < 0) { throw "87) GenerateObjectStateValue IdsByPath expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["A.C__" + id3_Val].Object, rootDocument.A[0].C[2])) { throw "88) GenerateObjectStateValue StatesByPathAndId Object expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["A.C__" + id3_Val].IsDeleted, true)) { throw "89) GenerateObjectStateValue StatesByPathAndId IsDeleted expected."; }
      if (objectStateValue.IdsByPath["A.C"].indexOf(id4) >= 0) { throw "90) Not GenerateObjectStateValue IdsByPath expected."; }

      if (!objectStateValue.IdsByPath.hasOwnProperty("B")) { throw "91) GenerateObjectStateValue IdsByPath expected."; }
      if (objectStateValue.IdsByPath["B"].indexOf(id1) < 0) { throw "92) GenerateObjectStateValue IdsByPath expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["B__" + id1_Val].Object, rootDocument.B[0])) { throw "93) GenerateObjectStateValue StatesByPathAndId Object expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["B__" + id1_Val].IsDeleted, true)) { throw "94) GenerateObjectStateValue StatesByPathAndId IsDeleted expected."; }
      if (objectStateValue.IdsByPath["B"].indexOf(id2) < 0) { throw "95) GenerateObjectStateValue IdsByPath expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["B__" + id2_Val].Object, rootDocument.B[1])) { throw "96) GenerateObjectStateValue StatesByPathAndId Object expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["B__" + id2_Val].IsDeleted, true)) { throw "97) GenerateObjectStateValue StatesByPathAndId IsDeleted expected."; }
      if (objectStateValue.IdsByPath["B"].indexOf(id3) < 0) { throw "98) GenerateObjectStateValue IdsByPath expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["B__" + id3_Val].Object, rootDocument.B[2])) { throw "99) GenerateObjectStateValue StatesByPathAndId Object expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["B__" + id3_Val].IsDeleted, true)) { throw "100) GenerateObjectStateValue StatesByPathAndId IsDeleted expected."; }
      if (objectStateValue.IdsByPath["B"].indexOf(id4) >= 0) { throw "101) Not GenerateObjectStateValue IdsByPath expected."; }

      if (!objectStateValue.IdsByPath.hasOwnProperty("B.D")) { throw "102) GenerateObjectStateValue IdsByPath expected."; }
      if (objectStateValue.IdsByPath["B.D"].indexOf(id1) < 0) { throw "103) GenerateObjectStateValue IdsByPath expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["B.D__" + id1_Val].Object, rootDocument.B[0].D[0])) { throw "104) GenerateObjectStateValue StatesByPathAndId Object expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["B.D__" + id1_Val].IsDeleted, true)) { throw "105) GenerateObjectStateValue StatesByPathAndId IsDeleted expected."; }
      if (objectStateValue.IdsByPath["B.D"].indexOf(id2) < 0) { throw "106) GenerateObjectStateValue IdsByPath expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["B.D__" + id2_Val].Object, rootDocument.B[0].D[1])) { throw "107) GenerateObjectStateValue StatesByPathAndId Object expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["B.D__" + id2_Val].IsDeleted, true)) { throw "108) GenerateObjectStateValue StatesByPathAndId IsDeleted expected."; }
      if (objectStateValue.IdsByPath["B.D"].indexOf(id3) < 0) { throw "109) GenerateObjectStateValue IdsByPath expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["B.D__" + id3_Val].Object, rootDocument.B[0].D[2])) { throw "110) GenerateObjectStateValue StatesByPathAndId Object expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["B.D__" + id3_Val].IsDeleted, true)) { throw "111) GenerateObjectStateValue StatesByPathAndId IsDeleted expected."; }
      if (objectStateValue.IdsByPath["B.D"].indexOf(id4) >= 0) { throw "112) Not GenerateObjectStateValue IdsByPath expected."; }


      var originalDocument = {
        _id: id1,
        A: [
          { _id: id2 }
        ],
        C: [
          { _id: id1 },
          { _id: id2 }
        ]
      };
      var currentDocument = {
        _id: id1,
        B: [
          { _id: id3 }
        ],
        C: [
          { _id: id2 },
          { _id: id3 }
        ]
      };
      var objectStateValue = fnDecia_Utility_GenerateObjectStateValue(currentDocument, originalDocument, ["A", "B", "C"], false);

      if (objectStateValue.IdsByPath[""].indexOf(id1) < 0) { throw "113) GenerateObjectStateValue IdsByPath expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["__" + id1_Val].Object, currentDocument)) { throw "114) GenerateObjectStateValue StatesByPathAndId Object expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["__" + id1_Val].IsDeleted, false)) { throw "115) GenerateObjectStateValue StatesByPathAndId IsDeleted expected."; }
      if (objectStateValue.IdsByPath[""].indexOf(id2) >= 0) { throw "116) Not GenerateObjectStateValue IdsByPath expected."; }

      if (!objectStateValue.IdsByPath.hasOwnProperty("A")) { throw "117) GenerateObjectStateValue IdsByPath expected."; }
      if (objectStateValue.IdsByPath["A"].indexOf(id1) >= 0) { throw "118) GenerateObjectStateValue IdsByPath expected."; }
      if (objectStateValue.IdsByPath["A"].indexOf(id2) < 0) { throw "119) GenerateObjectStateValue IdsByPath expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["A__" + id2_Val].Object, originalDocument.A[0])) { throw "120) GenerateObjectStateValue StatesByPathAndId Object expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["A__" + id2_Val].IsDeleted, true)) { throw "121) GenerateObjectStateValue StatesByPathAndId IsDeleted expected."; }
      if (objectStateValue.IdsByPath["A"].indexOf(id3) >= 0) { throw "122) GenerateObjectStateValue IdsByPath expected."; }
      if (objectStateValue.IdsByPath["A"].indexOf(id4) >= 0) { throw "123) Not GenerateObjectStateValue IdsByPath expected."; }

      if (!objectStateValue.IdsByPath.hasOwnProperty("B")) { throw "124) GenerateObjectStateValue IdsByPath expected."; }
      if (objectStateValue.IdsByPath["B"].indexOf(id1) >= 0) { throw "125) GenerateObjectStateValue IdsByPath expected."; }
      if (objectStateValue.IdsByPath["B"].indexOf(id2) >= 0) { throw "126) GenerateObjectStateValue IdsByPath expected."; }
      if (objectStateValue.IdsByPath["B"].indexOf(id3) < 0) { throw "127) GenerateObjectStateValue IdsByPath expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["B__" + id3_Val].Object, currentDocument.B[0])) { throw "128) GenerateObjectStateValue StatesByPathAndId Object expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["B__" + id3_Val].IsDeleted, false)) { throw "129) GenerateObjectStateValue StatesByPathAndId IsDeleted expected."; }
      if (objectStateValue.IdsByPath["B"].indexOf(id4) >= 0) { throw "130) Not GenerateObjectStateValue IdsByPath expected."; }

      if (!objectStateValue.IdsByPath.hasOwnProperty("C")) { throw "131) GenerateObjectStateValue IdsByPath expected."; }
      if (objectStateValue.IdsByPath["C"].indexOf(id1) < 0) { throw "132) GenerateObjectStateValue IdsByPath expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["C__" + id1_Val].Object, originalDocument.C[0])) { throw "133) GenerateObjectStateValue StatesByPathAndId Object expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["C__" + id1_Val].IsDeleted, true)) { throw "134) GenerateObjectStateValue StatesByPathAndId IsDeleted expected."; }
      if (objectStateValue.IdsByPath["C"].indexOf(id2) < 0) { throw "135) GenerateObjectStateValue IdsByPath expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["C__" + id2_Val].Object, originalDocument.C[1])) { throw "136) GenerateObjectStateValue StatesByPathAndId Object expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["C__" + id2_Val].Object, currentDocument.C[0])) { throw "137) GenerateObjectStateValue StatesByPathAndId Object expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["C__" + id2_Val].IsDeleted, false)) { throw "138) GenerateObjectStateValue StatesByPathAndId IsDeleted expected."; }
      if (objectStateValue.IdsByPath["C"].indexOf(id3) < 0) { throw "139) GenerateObjectStateValue IdsByPath expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["C__" + id3_Val].Object, currentDocument.C[1])) { throw "140) GenerateObjectStateValue StatesByPathAndId Object expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(objectStateValue.StatesByPathAndId["C__" + id3_Val].IsDeleted, false)) { throw "141) GenerateObjectStateValue StatesByPathAndId IsDeleted expected."; }
      if (objectStateValue.IdsByPath["C"].indexOf(id4) >= 0) { throw "142) Not GenerateObjectStateValue IdsByPath expected."; }
    }
  }
);