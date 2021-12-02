CREATE TABLE [dbo].[Decia_TimePeriodType](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[IsForever] [bit] NOT NULL,
	[EstimateInDays] [float] NOT NULL,
	[MinValidDays] [float] NOT NULL,
	[MaxValidDays] [float] NOT NULL,
	[DatePart_Value] [nvarchar](20) NOT NULL,
	[DatePart_Multiplier] [float] NOT NULL,
 CONSTRAINT [PK_Decia_TimePeriodType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

ALTER TABLE [dbo].[Decia_TimePeriodType] ADD  CONSTRAINT [DF_Decia_TimePeriodType_IsForever]  DEFAULT ((0)) FOR [IsForever]
GO