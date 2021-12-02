MyDb.system.js.save(
  {
    _id: "fnDecia_Utility_UpdateCachedReferences_Tests",
    value: function (myDb) {
      var id1 = ObjectId();
      var id2 = ObjectId();
      var id3 = ObjectId();
      var id4 = ObjectId();

      var id1_Val = id1.valueOf();
      var id2_Val = id2.valueOf();
      var id3_Val = id3.valueOf();
      var id4_Val = id4.valueOf();


      var localDocument = {
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
      var objectStateValue = fnDecia_Utility_GenerateObjectStateValue(localDocument, null, ["A", "A.C", "B", "B.D"], false);

      var externalDocument = {
        _id: id4,
        Related_Roots: { _id: id1 },
        Related_As: [{ _id: id1 }, { _id: id2 }, { _id: id3 }],
        Related_Ds: [{ _id: id1 }, { _id: id2 }, { _id: id3 }]
      }

      fnDecia_Utility_UpdateCachedReferences(externalDocument, "", "Related_Roots", objectStateValue, "");
      fnDecia_Utility_UpdateCachedReferences(externalDocument, "", "Related_As", objectStateValue, "A");
      fnDecia_Utility_UpdateCachedReferences(externalDocument, "", "Related_Ds", objectStateValue, "B.D");

      if (!fnDecia_Utility_AreJsonObjectsEqual(localDocument, externalDocument.Related_Roots.OS_CACHED_DOC)) { throw "1a) UpdateCachedReferences OS_CACHED_DOC expected."; }
      if (externalDocument.Related_As.length != 3) { throw "2) UpdateCachedReferences expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(localDocument.A[0], externalDocument.Related_As[0].OS_CACHED_DOC)) { throw "2a) UpdateCachedReferences OS_CACHED_DOC expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(localDocument.A[1], externalDocument.Related_As[1].OS_CACHED_DOC)) { throw "2b) UpdateCachedReferences OS_CACHED_DOC expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(localDocument.A[2], externalDocument.Related_As[2].OS_CACHED_DOC)) { throw "2c) UpdateCachedReferences OS_CACHED_DOC expected."; }
      if (externalDocument.Related_Ds.length != 3) { throw "3) UpdateCachedReferences expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(localDocument.B[0].D[0], externalDocument.Related_Ds[0].OS_CACHED_DOC)) { throw "3a) UpdateCachedReferences OS_CACHED_DOC expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(localDocument.B[0].D[1], externalDocument.Related_Ds[1].OS_CACHED_DOC)) { throw "3b) UpdateCachedReferences OS_CACHED_DOC expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(localDocument.B[0].D[2], externalDocument.Related_Ds[2].OS_CACHED_DOC)) { throw "3c) UpdateCachedReferences OS_CACHED_DOC expected."; }


      var localDocument = {
        _id: id1,
        Related_Local: { _id: id1 },
        Related_Locals: [
          { _id: id1 },
          { _id: id2 },
          { _id: id3 }
        ],
      };
      var objectStateValue = fnDecia_Utility_GenerateObjectStateValue(localDocument, null, ["Related_Local", "Related_Locals"], false);

      fnDecia_Utility_UpdateCachedReferences(localDocument, "", "Related_Local", objectStateValue, "");
      fnDecia_Utility_UpdateCachedReferences(localDocument, "", "Related_Locals", objectStateValue, "");

      if (localDocument.Related_Locals.length != 3) { throw "4) UpdateCachedReferences expected."; }

      if (!fnDecia_Utility_AreJsonObjectsEqual(localDocument._id, localDocument.Related_Local._id)) { throw "4a) UpdateCachedReferences OS_CACHED_DOC expected."; }
      if (!fnDecia_Utility_IsNull(localDocument.Related_Local.OS_CACHED_DOC)) { throw "4b) UpdateCachedReferences OS_CACHED_DOC expected."; }

      if (!fnDecia_Utility_AreJsonObjectsEqual(localDocument._id, localDocument.Related_Locals[0]._id)) { throw "4c) UpdateCachedReferences OS_CACHED_DOC expected."; }
      if (!fnDecia_Utility_IsNull(localDocument.Related_Locals[0].OS_CACHED_DOC)) { throw "4d) UpdateCachedReferences OS_CACHED_DOC expected."; }


      var localDocument_Original = {
        _id: id1,
        A: [
          {
            _id: id1,
            C: [{ _id: id1 }, { _id: id2 }]
          },
          { _id: id2 }
        ],
        B: [
          {
            _id: id1,
            D: [{ _id: id2 }, { _id: id3 }]
          },
          { _id: id3 }
        ]
      };
      var localDocument_Current = {
        _id: id1,
        A: [
          {
            _id: id1,
            C: [{ _id: id2 }, { _id: id3 }]
          },
          { _id: id3 }
        ],
        B: [
          {
            _id: id1,
            D: [{ _id: id1 }, { _id: id2 }]
          },
          { _id: id2 }
        ]
      };
      var objectStateValue = fnDecia_Utility_GenerateObjectStateValue(localDocument_Current, localDocument_Original, ["A", "A.C", "B", "B.D"], false);

      var externalDocument = {
        _id: id4,
        Related_Roots: { _id: id1 },
        Related_As: [{ _id: id1 }, { _id: id2 }],
        Related_Ds: [{ _id: id1 }, { _id: id3 }]
      }

      fnDecia_Utility_UpdateCachedReferences(externalDocument, "", "Related_Roots", objectStateValue, "");
      fnDecia_Utility_UpdateCachedReferences(externalDocument, "", "Related_As", objectStateValue, "A");
      fnDecia_Utility_UpdateCachedReferences(externalDocument, "", "Related_Ds", objectStateValue, "B.D");

      if (!fnDecia_Utility_AreJsonObjectsEqual(localDocument_Current, externalDocument.Related_Roots.OS_CACHED_DOC)) { throw "1a) UpdateCachedReferences OS_CACHED_DOC expected."; }
      if (externalDocument.Related_As.length != 2) { throw "5) UpdateCachedReferences expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(localDocument_Current.A[0]._id, id1)) { throw "5a) UpdateCachedReferences OS_CACHED_DOC expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(localDocument_Current.A[0], externalDocument.Related_As[0].OS_CACHED_DOC)) { throw "5c) UpdateCachedReferences OS_CACHED_DOC expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(localDocument_Current.A[1]._id, id3)) { throw "5d) UpdateCachedReferences OS_CACHED_DOC expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(localDocument_Current.A[1], externalDocument.Related_As[1].OS_CACHED_DOC)) { throw "5e) UpdateCachedReferences OS_CACHED_DOC expected."; }
      if (externalDocument.Related_Ds.length != 2) { throw "6) UpdateCachedReferences expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(localDocument_Current.B[0].D[0]._id, id1)) { throw "6a) UpdateCachedReferences OS_CACHED_DOC expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(localDocument_Current.B[0].D[0], externalDocument.Related_Ds[0].OS_CACHED_DOC)) { throw "6c) UpdateCachedReferences OS_CACHED_DOC expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(localDocument_Current.B[0].D[1]._id, id2)) { throw "6d) UpdateCachedReferences OS_CACHED_DOC expected."; }
      if (!fnDecia_Utility_AreJsonObjectsEqual(localDocument_Current.B[0].D[1], externalDocument.Related_Ds[1].OS_CACHED_DOC)) { throw "6e) UpdateCachedReferences OS_CACHED_DOC expected."; }


      var localDocument_Current = {
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
      var objectStateValue = fnDecia_Utility_GenerateObjectStateValue(localDocument_Current, null, ["A", "A.C", "B", "B.D"], true);

      var externalDocument = {
        _id: id4,
        Related_Roots: { _id: id1 },
        Related_As: [{ _id: id1 }, { _id: id2 }],
        Related_Ds: [{ _id: id1 }, { _id: id3 }]
      }

      fnDecia_Utility_UpdateCachedReferences(externalDocument, "", "Related_Roots", objectStateValue, "");
      fnDecia_Utility_UpdateCachedReferences(externalDocument, "", "Related_As", objectStateValue, "A");
      fnDecia_Utility_UpdateCachedReferences(externalDocument, "", "Related_Ds", objectStateValue, "B.D");

      if (fnDecia_Utility_IsNotNull(externalDocument.Related_Roots.OS_CACHED_DOC)) { throw "7a) UpdateCachedReferences OS_CACHED_DOC expected."; }
      if (externalDocument.Related_As.length != 0) { throw "7b) UpdateCachedReferences expected."; }
      if (externalDocument.Related_Ds.length != 0) { throw "7c) UpdateCachedReferences expected."; }
    }
  }
);