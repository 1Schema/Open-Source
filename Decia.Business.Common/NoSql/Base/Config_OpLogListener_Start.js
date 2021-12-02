if (recordChangesAutomatically) {
  MyDb.loadServerScripts();
  fnDecia_Consistency_OplogChangeListener(propagateChangesAutomatically);
}