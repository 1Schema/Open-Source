/*
DOCUMENT SCHEMA:
<?xml version="1.0" encoding="utf-16"?>
<xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema">
  <xs:complexType name="Decia_DataType_Type">
    <xs:sequence>
      <xs:element minOccurs="1" maxOccurs="1" name="_id" type="Integer" />
      <xs:element minOccurs="1" maxOccurs="1" name="Name" type="Text" />
      <xs:element minOccurs="1" maxOccurs="1" name="Description" type="Text" />
    </xs:sequence>
  </xs:complexType>
</xs:schema>
*/
MyDb.createCollection("Decia_DataTypes");
var OS_DataTypes = MyDb.getCollection("Decia_DataTypes");