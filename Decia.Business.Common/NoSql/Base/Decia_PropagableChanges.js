/*
DOCUMENT SCHEMA:
<?xml version="1.0" encoding="utf-16"?>
<xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema">
  <xs:complexType name="Decia_PropagableChange_Type">
    <xs:sequence>
      <xs:element minOccurs="1" maxOccurs="1" name="_id" type="Text" />
      <xs:element minOccurs="1" maxOccurs="1" name="CollectionName" type="Text" />
      <xs:element minOccurs="1" maxOccurs="1" name="ObjectId" type="UniqueId" />
      <xs:element minOccurs="1" maxOccurs="1" name="LatestTimestamp" type="Text" />
      <xs:element minOccurs="1" maxOccurs="1" name="ErrorCount" type="Integer" />
      <xs:element minOccurs="1" maxOccurs="1" name="IsCreated" type="Boolean" />
      <xs:element minOccurs="1" maxOccurs="1" name="CreatedLogItem" />
      <xs:element minOccurs="1" maxOccurs="1" name="IsUpdated" type="Boolean" />
      <xs:element minOccurs="1" maxOccurs="1" name="UpdatedLogItem" />
      <xs:element minOccurs="1" maxOccurs="1" name="IsDeleted" type="Boolean" />
      <xs:element minOccurs="1" maxOccurs="1" name="DeletedLogItem" />
    </xs:sequence>
  </xs:complexType>
</xs:schema>
*/
MyDb.createCollection("Decia_PropagableChanges");
var OS_PropagableChanges = MyDb.getCollection("Decia_PropagableChanges");
OS_PropagableChanges.createIndex({ "CollectionName": 1 });
OS_PropagableChanges.createIndex({ "ObjectId": 1 });
OS_PropagableChanges.createIndex({ "LatestTimestamp": 1 });
OS_PropagableChanges.createIndex({ "ErrorCount": 1 });
OS_PropagableChanges.createIndex({ "IsCreated": 1 });
OS_PropagableChanges.createIndex({ "CreatedLogItem.ts": 1 });
OS_PropagableChanges.createIndex({ "IsUpdated": 1 });
OS_PropagableChanges.createIndex({ "UpdatedLogItem.ts": 1 });
OS_PropagableChanges.createIndex({ "IsDeleted": 1 });
OS_PropagableChanges.createIndex({ "DeletedLogItem.ts": 1 });