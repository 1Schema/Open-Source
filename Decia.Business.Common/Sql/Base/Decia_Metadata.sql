CREATE TABLE [dbo].[Decia_Metadata](
	[ProjectId] [uniqueidentifier] NOT NULL,
	[RevisionNumber] [bigint] NOT NULL,
	[ModelTemplateId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[SqlName] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[ConciseRevisionNumber] [bigint] NOT NULL,
	[Latest_ChangeCount] [bigint] NOT NULL CONSTRAINT [DF_Model_LastChangeCount]  DEFAULT ((0)),
	[Latest_ChangeDate] [datetime] NOT NULL CONSTRAINT [DF_Decia_Metadata_LatestChange_Date]  DEFAULT (getdate())
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'DO NOT CHANGE! This value is the Id within the Decia System that identifies the "Project" that this DB was exported from.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Decia_Metadata', @level2type=N'COLUMN',@level2name=N'ProjectId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'DO NOT CHANGE! This value is the Revision of the Project that was exported.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Decia_Metadata', @level2type=N'COLUMN',@level2name=N'RevisionNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'DO NOT CHANGE! This value is the Id of the "Model Template" that was exported.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Decia_Metadata', @level2type=N'COLUMN',@level2name=N'ModelTemplateId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'DO NOT CHANGE! This value is the "RevisionNumber" shown to the user when hiding System-generated Revisions.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Decia_Metadata', @level2type=N'COLUMN',@level2name=N'ConciseRevisionNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'DO NOT CHANGE! This value should be auto-incremented when changes are made to data tables in this Database.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Decia_Metadata', @level2type=N'COLUMN',@level2name=N'Latest_ChangeCount'
GO