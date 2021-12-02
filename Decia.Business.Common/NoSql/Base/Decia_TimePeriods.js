/*
DOCUMENT SCHEMA:
<?xml version="1.0" encoding="utf-16"?>
<xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema">
  <xs:complexType name="Decia_TimePeriod_Type">
    <xs:sequence>
      <xs:element minOccurs="1" maxOccurs="1" name="_id" type="Text" />
      <xs:element minOccurs="1" maxOccurs="1" name="TimePeriodTypeId" type="Integer" />
      <xs:element minOccurs="1" maxOccurs="1" name="StartDate" type="DateTime" />
      <xs:element minOccurs="1" maxOccurs="1" name="EndDate" type="DateTime" />
      <xs:element minOccurs="1" maxOccurs="1" name="IsForever" type="Boolean" />
    </xs:sequence>
  </xs:complexType>
</xs:schema>
*/
MyDb.createCollection("Decia_TimePeriods");
var OS_TimePeriods = MyDb.getCollection("Decia_TimePeriods");
OS_TimePeriods.createIndex({ "TimePeriodTypeId": 1 });
OS_TimePeriods.createIndex({ "StartDate": 1 });
OS_TimePeriods.createIndex({ "EndDate": 1 });