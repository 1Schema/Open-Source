/*
DOCUMENT SCHEMA:
<?xml version="1.0" encoding="utf-16"?>
<xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema">
  <xs:complexType name="Decia_StructuralType_Type">
    <xs:sequence>
      <xs:element minOccurs="1" maxOccurs="1" name="_id" type="UniqueId" />
      <xs:element minOccurs="1" maxOccurs="1" name="Name" type="Text" />
      <xs:element minOccurs="1" maxOccurs="1" name="MongoName" type="Text" />
      <xs:element minOccurs="1" maxOccurs="1" name="Description" type="Text" />
      <xs:element minOccurs="1" maxOccurs="1" name="ObjectTypeId" type="Integer" />
      <xs:element minOccurs="1" maxOccurs="1" name="TreeLevel_Basic" type="Integer" />
      <xs:element minOccurs="1" maxOccurs="1" name="TreeLevel_Extended" type="Integer" />
      <xs:element minOccurs="1" maxOccurs="1" name="Parent_StructuralTypeId" nillable="true" type="Integer" />
      <xs:element minOccurs="1" maxOccurs="1" name="Parent_IsNullable" type="Boolean" />
      <xs:element minOccurs="1" maxOccurs="1" name="Parent_Default_InstanceId" nillable="true" type="UniqueId" />
      <xs:element minOccurs="1" maxOccurs="1" name="Instance_Collection_Name" type="Text" />
      <xs:element minOccurs="1" maxOccurs="1" name="Instance_Id_VariableTemplateId" type="UniqueId" />
      <xs:element minOccurs="1" maxOccurs="1" name="Instance_Name_VariableTemplateId" type="UniqueId" />
      <xs:element minOccurs="1" maxOccurs="1" name="Instance_Sorting_VariableTemplateId" type="UniqueId" />
      <xs:element minOccurs="1" maxOccurs="1" name="Instance_ParentId_VariableTemplateId" nillable="true" type="UniqueId" />
    </xs:sequence>
  </xs:complexType>
</xs:schema>
*/
MyDb.createCollection("Decia_StructuralTypes");
var OS_StructuralTypes = MyDb.getCollection("Decia_StructuralTypes");
OS_StructuralTypes.createIndex({ "TreeLevel_Basic": 1 });
OS_StructuralTypes.createIndex({ "TreeLevel_Extended": 1 });
OS_StructuralTypes.createIndex({ "Parent_StructuralTypeId": 1 });
OS_StructuralTypes.createIndex({ "Instance_Collection_Name": 1 });
OS_StructuralTypes.createIndex({ "Instance_Id_VariableTemplateId": 1 });
OS_StructuralTypes.createIndex({ "Instance_Name_VariableTemplateId": 1 });
OS_StructuralTypes.createIndex({ "Instance_Sorting_VariableTemplateId": 1 });
OS_StructuralTypes.createIndex({ "Instance_ParentId_VariableTemplateId": 1 });