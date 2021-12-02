/*
DOCUMENT SCHEMA:
<?xml version="1.0" encoding="utf-16"?>
<xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema">
  <xs:complexType name="Decia_ResultSet_Type">
    <xs:sequence>
      <xs:element minOccurs="1" maxOccurs="1" name="_id" type="UniqueId" />
      <xs:element minOccurs="1" maxOccurs="1" name="Name" type="Text" />
      <xs:element minOccurs="1" maxOccurs="1" name="Description" type="Text" />
      <xs:element minOccurs="1" maxOccurs="1" name="Metadata_ChangeCount" type="Integer" />
      <xs:element minOccurs="1" maxOccurs="1" name="TimeDimensionSetting">
        <xs:complexType>
          <xs:sequence>
            <xs:element minOccurs="1" maxOccurs="1" name="StartDate" type="DateTime" />
            <xs:element minOccurs="1" maxOccurs="1" name="EndDate" type="DateTime" />
          </xs:sequence>
        </xs:complexType>
      </xs:element>
      <xs:element minOccurs="1" maxOccurs="1" name="ProcessedGroups">
        <xs:complexType>
          <xs:sequence>
            <xs:element minOccurs="0" maxOccurs="unbounded" name="ArrayItem">
              <xs:complexType>
                <xs:sequence>
                  <xs:element minOccurs="1" maxOccurs="1" name="VariableTemplateGroupId" type="UniqueID" />
                  <xs:element minOccurs="1" maxOccurs="1" name="OrderIndex" type="Integer" />
                  <xs:element minOccurs="1" maxOccurs="1" name="Computation_Succeeded" type="Boolean" />
                </xs:sequence>
              </xs:complexType>
            </xs:element>
          </xs:sequence>
        </xs:complexType>
      </xs:element>
    </xs:sequence>
  </xs:complexType>
</xs:schema>
*/
MyDb.createCollection("Decia_ResultSets");
var OS_ResultSets = MyDb.getCollection("Decia_ResultSets");
OS_ResultSets.createIndex({ "Metadata_ChangeCount": 1 });
OS_ResultSets.createIndex({ "TimeDimensionSetting.StartDate": 1 });
OS_ResultSets.createIndex({ "TimeDimensionSetting.EndDate": 1 });
OS_ResultSets.createIndex({ "ProcessedGroups.VariableTemplateGroupId": 1 });
OS_ResultSets.createIndex({ "ProcessedGroups.Computation_Succeeded": 1 });