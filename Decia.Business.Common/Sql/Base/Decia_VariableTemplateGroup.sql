CREATE TABLE [dbo].[Decia_VariableTemplateGroup](
	[Id] [uniqueidentifier] NOT NULL,
	[ProcessingIndex] [int] NOT NULL,
	[HasCycles] [bit] NOT NULL,
	[HasUnresolvableCycles] [bit] NOT NULL,
 CONSTRAINT [PK_VariableTemplateGroup] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO