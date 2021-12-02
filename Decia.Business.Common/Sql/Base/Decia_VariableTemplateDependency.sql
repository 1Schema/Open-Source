CREATE TABLE [dbo].[Decia_VariableTemplateDependency](
	[Result_VariableTemplateId] [uniqueidentifier] NOT NULL,
	[Dependency_VariableTemplateId] [uniqueidentifier] NOT NULL,
	[Dependency_StructuralDimensionNumber] [int] NOT NULL,
	[IsStrict] [bit] NOT NULL,
 CONSTRAINT [PK_VariableTemplateDependency] PRIMARY KEY CLUSTERED 
(
	[Result_VariableTemplateId] ASC,
	[Dependency_VariableTemplateId] ASC,
	[Dependency_StructuralDimensionNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[Decia_VariableTemplateDependency] ADD  CONSTRAINT [DF_Decia_VariableTemplateDependency_DimensionNumber]  DEFAULT ((1)) FOR [Dependency_StructuralDimensionNumber]
GO

ALTER TABLE [dbo].[Decia_VariableTemplateDependency] ADD  CONSTRAINT [DF_Decia_VariableTemplateDependency_IsStrict]  DEFAULT ((1)) FOR [IsStrict]
GO

ALTER TABLE [dbo].[Decia_VariableTemplateDependency]  WITH CHECK ADD  CONSTRAINT [FK_Decia_VariableTemplateDependency_Decia_OutgoingVariableTemplate] FOREIGN KEY([Result_VariableTemplateId])
REFERENCES [dbo].[Decia_VariableTemplate] ([Id])
GO

ALTER TABLE [dbo].[Decia_VariableTemplateDependency] CHECK CONSTRAINT [FK_Decia_VariableTemplateDependency_Decia_OutgoingVariableTemplate]
GO

ALTER TABLE [dbo].[Decia_VariableTemplateDependency]  WITH CHECK ADD  CONSTRAINT [FK_Decia_VariableTemplateDependency_Decia_IncomingVariableTemplate] FOREIGN KEY([Dependency_VariableTemplateId])
REFERENCES [dbo].[Decia_VariableTemplate] ([Id])
GO

ALTER TABLE [dbo].[Decia_VariableTemplateDependency] CHECK CONSTRAINT [FK_Decia_VariableTemplateDependency_Decia_IncomingVariableTemplate]
GO