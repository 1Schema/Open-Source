/*
 * INSTRUCTIONS:
 *
 * 1) For the change handling code to work correctly, you must launch mongod using replication
 *    EXAMPLE: mongod.exe--dbpath [PATH]--replSet [NAME] --oplogSize [SIZE_IN_MB]
 *
 * 2) To create your database, open a mongo shell and use the load command to load this file
 *    EXAMPLE: load([FILE_PATH]);
 *
 * 3) To automatically record changes that occur, make sure that "recordChangesAutomatically"
 *    is set to true (the default)
 *
 * 4) To automatically propagate changes that occur throughout your database, make sure that
 *    "propagateChangesAutomatically" is set to true (the default)
 */

var recordChangesAutomatically = true;
var propagateChangesAutomatically = true;