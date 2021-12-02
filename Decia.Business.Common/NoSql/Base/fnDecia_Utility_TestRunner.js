MyDb.system.js.save(
  {
    _id: "fnDecia_Utility_TestRunner",
    value: function (myDb, sysDb) {
      print();
      print("JavaScript tests started! \n");

      print("Started: fnDecia_System_OpLog_Tests");
      fnDecia_System_OpLog_Tests(myDb, sysDb);
      print("Finished: fnDecia_System_OpLog_Tests \n");

      print("Started: fnDecia_Utility_IsNull_Tests");
      fnDecia_Utility_IsNull_Tests(myDb);
      print("Finished: fnDecia_Utility_IsNull_Tests \n");

      print("Started: fnDecia_Utility_IsNotNull_Tests");
      fnDecia_Utility_IsNotNull_Tests(myDb);
      print("Finished: fnDecia_Utility_IsNotNull_Tests \n");

      print("Started: fnDecia_Utility_HasProperty_Tests");
      fnDecia_Utility_HasProperty_Tests(myDb);
      print("Finished: fnDecia_Utility_HasProperty_Tests \n");

      print("Started: fnDecia_Utility_IsArray_Tests");
      fnDecia_Utility_IsArray_Tests(myDb);
      print("Finished: fnDecia_Utility_IsArray_Tests \n");

      print("Started: fnDecia_Utility_AreJsonObjectsEqual_Tests");
      fnDecia_Utility_AreJsonObjectsEqual_Tests(myDb);
      print("Finished: fnDecia_Utility_AreJsonObjectsEqual_Tests \n");

      print("Started: fnDecia_Utility_CopyJsonObject_Tests");
      fnDecia_Utility_CopyJsonObject_Tests(myDb);
      print("Finished: fnDecia_Utility_CopyJsonObject_Tests \n");

      print("Started: fnDecia_Utility_FindNestedJsonObjectForId_Tests");
      fnDecia_Utility_FindNestedJsonObjectForId_Tests(myDb);
      print("Finished: fnDecia_Utility_FindNestedJsonObjectForId_Tests \n");

      print("Started: fnDecia_Utility_GenerateObjectStateValue_Tests");
      fnDecia_Utility_GenerateObjectStateValue_Tests(myDb);
      print("Finished: fnDecia_Utility_GenerateObjectStateValue_Tests \n");

      print("Started: fnDecia_Utility_UpdateCachedReferences_Tests");
      fnDecia_Utility_UpdateCachedReferences_Tests(myDb);
      print("Finished: fnDecia_Utility_UpdateCachedReferences_Tests \n");

      print("All tests complete successfully! \n");
    }
  }
);