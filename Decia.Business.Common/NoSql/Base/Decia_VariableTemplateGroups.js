/*
DOCUMENT SCHEMA:
<?xml version="1.0" encoding="utf-16"?>
<xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema">
  <xs:complexType name="Decia_VariableTemplateGroup_Type">
    <xs:sequence>
      <xs:element minOccurs="1" maxOccurs="1" name="_id" type="UniqueId" />
      <xs:element minOccurs="1" maxOccurs="1" name="ProcessingIndex" type="Integer" />
      <xs:element minOccurs="1" maxOccurs="1" name="HasCycles" type="Boolean" />
      <xs:element minOccurs="1" maxOccurs="1" name="HasUnresolvableCycles" type="Boolean" />
      <xs:element minOccurs="1" maxOccurs="1" name="Members">
        <xs:complexType>
          <xs:sequence>
            <xs:element minOccurs="0" maxOccurs="unbounded" name="ArrayItem">
              <xs:complexType>
                <xs:sequence>
                  <xs:element minOccurs="1" maxOccurs="1" name="VariableTemplateId" type="UniqueID" />
                  <xs:element minOccurs="1" maxOccurs="1" name="Priority" type="Integer" />
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
MyDb.createCollection("Decia_VariableTemplateGroups");
var OS_VariableTemplateGroups = MyDb.getCollection("Decia_VariableTemplateGroups");
OS_VariableTemplateGroups.createIndex({ "ProcessingIndex": 1 });
OS_VariableTemplateGroups.createIndex({ "HasCycles": 1 });
OS_VariableTemplateGroups.createIndex({ "HasUnresolvableCycles": 1 });
OS_VariableTemplateGroups.createIndex({ "Members.VariableTemplateId": 1 });
OS_VariableTemplateGroups.createIndex({ "Members.Priority": 1 });