CREATE TABLE [dbo].[Decia_StructuralType](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[SqlName] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[ObjectTypeId] [int] NOT NULL,
	[TreeLevel_Basic] [int] NOT NULL,
	[TreeLevel_Extended] [int] NOT NULL,
	[Parent_StructuralTypeId] [uniqueidentifier] NULL,
	[Parent_IsNullable] [bit] NOT NULL,
	[Parent_Default_InstanceId] [uniqueidentifier] NULL,
	[Instance_Table_Name] [nvarchar](max) NOT NULL,
	[Instance_Id_VariableTemplateId] [uniqueidentifier] NOT NULL,
	[Instance_Name_VariableTemplateId] [uniqueidentifier] NOT NULL,
	[Instance_Sorting_VariableTemplateId] [uniqueidentifier] NOT NULL,
	[Instance_ParentId_VariableTemplateId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_Decia_StructuralType_1] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

ALTER TABLE [dbo].[Decia_StructuralType]  WITH CHECK ADD  CONSTRAINT [FK_Decia_StructuralType_Decia_ObjectType] FOREIGN KEY([ObjectTypeId])
REFERENCES [dbo].[Decia_ObjectType] ([Id])
GO

ALTER TABLE [dbo].[Decia_StructuralType] CHECK CONSTRAINT [FK_Decia_StructuralType_Decia_ObjectType]
GO

ALTER TABLE [dbo].[Decia_StructuralType]  WITH CHECK ADD  CONSTRAINT [FK_Decia_StructuralType_Decia_StructuralType] FOREIGN KEY([Parent_StructuralTypeId])
REFERENCES [dbo].[Decia_StructuralType] ([Id])
GO

ALTER TABLE [dbo].[Decia_StructuralType] CHECK CONSTRAINT [FK_Decia_StructuralType_Decia_StructuralType]
GO