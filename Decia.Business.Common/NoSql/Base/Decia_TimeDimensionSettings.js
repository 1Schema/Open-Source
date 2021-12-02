/*
DOCUMENT SCHEMA:
<?xml version="1.0" encoding="utf-16"?>
<xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema">
  <xs:complexType name="Decia_TimeDimensionSetting_Type">
    <xs:sequence>
      <xs:element minOccurs="1" maxOccurs="1" name="_id" type="Integer" />
      <xs:element minOccurs="1" maxOccurs="1" name="StartDate" type="DateTime" />
      <xs:element minOccurs="1" maxOccurs="1" name="EndDate" type="DateTime" />
    </xs:sequence>
  </xs:complexType>
</xs:schema>
*/
MyDb.createCollection("Decia_TimeDimensionSettings", { capped: true, size: 4000, max: 1 });
var OS_TimeDimensionSettings = MyDb.getCollection("Decia_TimeDimensionSettings");