CREATE TABLE [dbo].[Decia_VariableTemplateGroupMember](
	[VariableTemplateGroupId] [uniqueidentifier] NOT NULL,
	[VariableTemplateId] [uniqueidentifier] NOT NULL,
	[Priority] [int] NOT NULL,
 CONSTRAINT [PK_VariableTemplateGroupMember] PRIMARY KEY CLUSTERED 
(
	[VariableTemplateGroupId] ASC,
	[VariableTemplateId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[Decia_VariableTemplateGroupMember]  WITH CHECK ADD  CONSTRAINT [FK_Decia_VariableTemplateGroupMember_Decia_VariableTemplate] FOREIGN KEY([VariableTemplateId])
REFERENCES [dbo].[Decia_VariableTemplate] ([Id])
GO

ALTER TABLE [dbo].[Decia_VariableTemplateGroupMember] CHECK CONSTRAINT [FK_Decia_VariableTemplateGroupMember_Decia_VariableTemplate]
GO

ALTER TABLE [dbo].[Decia_VariableTemplateGroupMember]  WITH CHECK ADD  CONSTRAINT [FK_Decia_VariableTemplateGroupMember_Decia_VariableTemplateGroup] FOREIGN KEY([VariableTemplateGroupId])
REFERENCES [dbo].[Decia_VariableTemplateGroup] ([Id])
GO

ALTER TABLE [dbo].[Decia_VariableTemplateGroupMember] CHECK CONSTRAINT [FK_Decia_VariableTemplateGroupMember_Decia_VariableTemplateGroup]
GO