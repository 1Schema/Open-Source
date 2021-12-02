ALTER TABLE [dbo].[TimeAssessments] ADD  CONSTRAINT [UK_TimeAssessments_MultiTimeKey] UNIQUE NONCLUSTERED 
(
	[EF_ProjectGuid] ASC,
	[EF_RevisionNumber] ASC,
	[EF_ChronometricValueGuid] ASC,
	[EF_PrimaryStartDate] ASC,
	[EF_PrimaryEndDate] ASC,
	[EF_SecondaryStartDate] ASC,
	[EF_SecondaryEndDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]