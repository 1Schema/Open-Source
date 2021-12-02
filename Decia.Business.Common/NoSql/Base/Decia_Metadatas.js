/*
DOCUMENT SCHEMA:
<?xml version="1.0" encoding="utf-16"?>
<xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema">
  <xs:complexType name="Decia_Metadata_Type">
    <xs:sequence>
      <xs:element minOccurs="1" maxOccurs="1" name="_id" type="Integer" />
      <xs:element minOccurs="1" maxOccurs="1" name="ProjectId" type="UniqueID" />
      <xs:element minOccurs="1" maxOccurs="1" name="RevisionNumber" type="Integer" />
      <xs:element minOccurs="1" maxOccurs="1" name="ModelTemplateId" type="UniqueID" />
      <xs:element minOccurs="1" maxOccurs="1" name="Name" type="Text" />
      <xs:element minOccurs="1" maxOccurs="1" name="MongoName" type="Text" />
      <xs:element minOccurs="1" maxOccurs="1" name="Description" type="Text" />
      <xs:element minOccurs="1" maxOccurs="1" name="ConciseRevisionNumber" type="Integer" />
      <xs:element minOccurs="1" maxOccurs="1" name="Latest_ChangeCount" type="Integer" />
      <xs:element minOccurs="1" maxOccurs="1" name="Latest_ChangeDate" type="DateTime" />
    </xs:sequence>
  </xs:complexType>
</xs:schema>
*/
MyDb.createCollection("Decia_Metadatas", { capped: true, size: 16000000, max: 1 });
var OS_Metadatas = MyDb.getCollection("Decia_Metadatas");