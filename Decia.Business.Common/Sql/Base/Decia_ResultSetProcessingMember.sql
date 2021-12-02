CREATE TABLE [dbo].[Decia_ResultSetProcessingMember](
	[ResultSetId] [uniqueidentifier] NOT NULL,
	[VariableTemplateGroupId] [uniqueidentifier] NOT NULL,
	[OrderIndex] [bigint] NOT NULL,
	[Computation_Succeeded] [bit] NOT NULL,
	[Computation_StartDate] [datetime] NULL,
	[Computation_EndDate] [datetime] NULL,
 CONSTRAINT [PK_Decia_ResultSetProcessingMember] PRIMARY KEY CLUSTERED 
(
	[ResultSetId] ASC,
	[VariableTemplateGroupId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[Decia_ResultSetProcessingMember] ADD  CONSTRAINT [DF_Decia_ResultSetProcessingMember_Succeeded]  DEFAULT ((0)) FOR [Computation_Succeeded]
GO

ALTER TABLE [dbo].[Decia_ResultSetProcessingMember]  WITH CHECK ADD  CONSTRAINT [FK_Decia_ResultSetProcessingMember_Decia_ResultSet] FOREIGN KEY([ResultSetId])
REFERENCES [dbo].[Decia_ResultSet] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Decia_ResultSetProcessingMember] CHECK CONSTRAINT [FK_Decia_ResultSetProcessingMember_Decia_ResultSet]
GO

ALTER TABLE [dbo].[Decia_ResultSetProcessingMember]  WITH CHECK ADD  CONSTRAINT [FK_Decia_ResultSetProcessingMember_Decia_VariableTemplateGroup] FOREIGN KEY([VariableTemplateGroupId])
REFERENCES [dbo].[Decia_VariableTemplateGroup] ([Id])
GO

ALTER TABLE [dbo].[Decia_ResultSetProcessingMember] CHECK CONSTRAINT [FK_Decia_ResultSetProcessingMember_Decia_VariableTemplateGroup]
GO