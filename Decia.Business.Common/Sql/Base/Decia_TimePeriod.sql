CREATE TABLE [dbo].[Decia_TimePeriod](
	[Id] [uniqueidentifier] NOT NULL,
	[TimePeriodTypeId] [int] NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
	[IsForever] [bit] NOT NULL,
 CONSTRAINT [PK_TimePeriodValue] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[Decia_TimePeriod] ADD  CONSTRAINT [DF_Decia_TimePeriod_IsForever]  DEFAULT ((0)) FOR [IsForever]
GO

ALTER TABLE [dbo].[Decia_TimePeriod]  WITH CHECK ADD  CONSTRAINT [FK_Decia_TimePeriod_Decia_TimePeriodType] FOREIGN KEY([TimePeriodTypeId])
REFERENCES [dbo].[Decia_TimePeriodType] ([Id])
GO

ALTER TABLE [dbo].[Decia_TimePeriod] CHECK CONSTRAINT [FK_Decia_TimePeriod_Decia_TimePeriodType]
GO