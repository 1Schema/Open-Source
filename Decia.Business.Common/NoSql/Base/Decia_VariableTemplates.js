/*
DOCUMENT SCHEMA:
<?xml version="1.0" encoding="utf-16"?>
<xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema">
  <xs:complexType name="Decia_VariableTemplate_Type">
    <xs:sequence>
      <xs:element minOccurs="1" maxOccurs="1" name="_id" type="UniqueId" />
      <xs:element minOccurs="1" maxOccurs="1" name="Name" type="Text" />
      <xs:element minOccurs="1" maxOccurs="1" name="MongoName" type="Text" />
      <xs:element minOccurs="1" maxOccurs="1" name="Description" type="Text" />
      <xs:element minOccurs="1" maxOccurs="1" name="StructuralTypeId" type="UniqueId" />
      <xs:element minOccurs="1" maxOccurs="1" name="Related_StructuralTypeId" nillable="true" type="UniqueId" />
      <xs:element minOccurs="1" maxOccurs="1" name="Related_StructuralDimensionNumber" nillable="true" type="Integer" />
      <xs:element minOccurs="1" maxOccurs="1" name="IsComputed" type="Boolean" />
      <xs:element minOccurs="1" maxOccurs="1" name="TimeDimensionCount" type="Integer" />
      <xs:element minOccurs="1" maxOccurs="1" name="PrimaryTimePeriodTypeId" nillable="true" type="Integer" />
      <xs:element minOccurs="1" maxOccurs="1" name="SecondaryTimePeriodTypeId" nillable="true" type="Integer" />
      <xs:element minOccurs="1" maxOccurs="1" name="DataTypeId" type="Integer" />
      <xs:element minOccurs="1" maxOccurs="1" name="Instance_Field_Name" type="Text" />
      <xs:element minOccurs="1" maxOccurs="1" name="Instance_Field_DefaultValue" nillable="true" type="Text" />
      <xs:element minOccurs="1" maxOccurs="1" name="Dependencies">
        <xs:complexType>
          <xs:sequence>
            <xs:element minOccurs="0" maxOccurs="unbounded" name="ArrayItem">
              <xs:complexType>
                <xs:sequence>
                  <xs:element minOccurs="1" maxOccurs="1" name="VariableTemplateId" type="UniqueID" />
                  <xs:element minOccurs="1" maxOccurs="1" name="StructuralDimensionNumber" type="Integer" />
                  <xs:element minOccurs="1" maxOccurs="1" name="IsStrict" type="Boolean" />
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
MyDb.createCollection("Decia_VariableTemplates");
var OS_VariableTemplates = MyDb.getCollection("Decia_VariableTemplates");
OS_VariableTemplates.createIndex({ "StructuralTypeId": 1 });
OS_VariableTemplates.createIndex({ "Related_StructuralTypeId": 1 });
OS_VariableTemplates.createIndex({ "Related_StructuralDimensionNumber": 1 });
OS_VariableTemplates.createIndex({ "IsComputed": 1 });
OS_VariableTemplates.createIndex({ "TimeDimensionCount": 1 });
OS_VariableTemplates.createIndex({ "PrimaryTimePeriodTypeId": 1 });
OS_VariableTemplates.createIndex({ "SecondaryTimePeriodTypeId": 1 });
OS_VariableTemplates.createIndex({ "DataTypeId": 1 });
OS_VariableTemplates.createIndex({ "Dependencies.VariableTemplateId": 1 });
OS_VariableTemplates.createIndex({ "Dependencies.StructuralDimensionNumber": 1 });
OS_VariableTemplates.createIndex({ "Dependencies.IsStrict": 1 });