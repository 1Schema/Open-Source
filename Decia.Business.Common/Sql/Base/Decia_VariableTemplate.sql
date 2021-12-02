CREATE TABLE [dbo].[Decia_VariableTemplate](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[SqlName] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[StructuralTypeId] [uniqueidentifier] NOT NULL,
	[Related_StructuralTypeId] [uniqueidentifier] NULL,
	[Related_StructuralDimensionNumber] [int] NULL,
	[IsComputed] [bit] NOT NULL,
	[TimeDimensionCount] [int] NOT NULL CONSTRAINT [DF_Decia_VariableTemplate_TimeDimensionCount]  DEFAULT ((0)),
	[PrimaryTimePeriodTypeId] [int] NULL,
	[SecondaryTimePeriodTypeId] [int] NULL,
	[DataTypeId] [int] NOT NULL,
	[Instance_Column_Name] [nvarchar](max) NOT NULL,
	[Instance_Column_DefaultValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_VariableTemplate] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

ALTER TABLE [dbo].[Decia_VariableTemplate]  WITH CHECK ADD  CONSTRAINT [FK_Decia_VariableTemplate_Decia_DataType] FOREIGN KEY([DataTypeId])
REFERENCES [dbo].[Decia_DataType] ([Id])
GO

ALTER TABLE [dbo].[Decia_VariableTemplate] CHECK CONSTRAINT [FK_Decia_VariableTemplate_Decia_DataType]
GO

ALTER TABLE [dbo].[Decia_VariableTemplate]  WITH CHECK ADD  CONSTRAINT [FK_Decia_VariableTemplate_Decia_StructuralType] FOREIGN KEY([StructuralTypeId])
REFERENCES [dbo].[Decia_StructuralType] ([Id])
GO

ALTER TABLE [dbo].[Decia_VariableTemplate] CHECK CONSTRAINT [FK_Decia_VariableTemplate_Decia_StructuralType]
GO

ALTER TABLE [dbo].[Decia_VariableTemplate]  WITH CHECK ADD  CONSTRAINT [FK_Decia_VariableTemplate_Decia_PrimaryTimePeriodType] FOREIGN KEY([PrimaryTimePeriodTypeId])
REFERENCES [dbo].[Decia_TimePeriodType] ([Id])
GO

ALTER TABLE [dbo].[Decia_VariableTemplate] CHECK CONSTRAINT [FK_Decia_VariableTemplate_Decia_PrimaryTimePeriodType]
GO

ALTER TABLE [dbo].[Decia_VariableTemplate]  WITH CHECK ADD  CONSTRAINT [FK_Decia_VariableTemplate_Decia_SecondaryTimePeriodType] FOREIGN KEY([SecondaryTimePeriodTypeId])
REFERENCES [dbo].[Decia_TimePeriodType] ([Id])
GO

ALTER TABLE [dbo].[Decia_VariableTemplate] CHECK CONSTRAINT [FK_Decia_VariableTemplate_Decia_SecondaryTimePeriodType]
GO