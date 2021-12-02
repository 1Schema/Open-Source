/*
DOCUMENT SCHEMA:
<?xml version="1.0" encoding="utf-16"?>
<xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema">
  <xs:complexType name="Decia_TimePeriodType_Type">
    <xs:sequence>
      <xs:element minOccurs="1" maxOccurs="1" name="_id" type="Integer" />
      <xs:element minOccurs="1" maxOccurs="1" name="Name" type="Text" />
      <xs:element minOccurs="1" maxOccurs="1" name="Description" type="Text" />
      <xs:element minOccurs="1" maxOccurs="1" name="IsForever" type="Boolean" />
      <xs:element minOccurs="1" maxOccurs="1" name="EstimateInDays" type="Decimal" />
      <xs:element minOccurs="1" maxOccurs="1" name="MinValidDays" type="Decimal" />
      <xs:element minOccurs="1" maxOccurs="1" name="MaxValidDays" type="Decimal" />
      <xs:element minOccurs="1" maxOccurs="1" name="DatePart_Value" type="Text" />
      <xs:element minOccurs="1" maxOccurs="1" name="DatePart_Multiplier" type="Decimal" />
    </xs:sequence>
  </xs:complexType>
</xs:schema>
*/
MyDb.createCollection("Decia_TimePeriodTypes");
var OS_TimePeriodTypes = MyDb.getCollection("Decia_TimePeriodTypes");