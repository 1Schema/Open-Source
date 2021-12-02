CREATE TABLE [dbo].[Decia_ResultSetTimeDimensionSetting](
	[ResultSetId] [uniqueidentifier] NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
 CONSTRAINT [PK_Decia_ResultSetTimeDimensionSetting] PRIMARY KEY CLUSTERED 
(
	[ResultSetId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[Decia_ResultSetTimeDimensionSetting]  WITH CHECK ADD  CONSTRAINT [FK_Decia_ResultSetTimeDimensionSetting_Decia_ResultSet] FOREIGN KEY([ResultSetId])
REFERENCES [dbo].[Decia_ResultSet] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Decia_ResultSetTimeDimensionSetting] CHECK CONSTRAINT [FK_Decia_ResultSetTimeDimensionSetting_Decia_ResultSet]
GO