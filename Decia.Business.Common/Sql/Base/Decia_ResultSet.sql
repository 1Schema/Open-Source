CREATE TABLE [dbo].[Decia_ResultSet](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[Metadata_ChangeCount] [bigint] NOT NULL,
	[Metadata_ChangeDate] [datetime] NOT NULL,
	[Computation_Succeeded] [bit] NOT NULL CONSTRAINT [DF_Decia_ResultSet_Computation_Succeeded]  DEFAULT (0),
	[Computation_StartDate] [datetime] NOT NULL CONSTRAINT [DF_Decia_ResultSet_Computation_StartDate]  DEFAULT (getdate()),
	[Computation_EndDate] [datetime] NULL,
 CONSTRAINT [PK_ResultSet] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

ALTER TABLE [dbo].[Decia_ResultSet] ADD  CONSTRAINT [DF_Decia_ResultSet_Id]  DEFAULT (newid()) FOR [Id]
GO