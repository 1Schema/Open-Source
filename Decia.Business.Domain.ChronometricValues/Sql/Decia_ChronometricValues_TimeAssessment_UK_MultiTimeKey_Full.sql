CREATE UNIQUE NONCLUSTERED INDEX [UK_TimeAssessments_MultiTimeKey_Full] ON [dbo].[TimeAssessments]
(
	[EF_ProjectGuid] ASC,
	[EF_RevisionNumber] ASC,
	[EF_ChronometricValueGuid] ASC,
	[EF_HasPrimaryTimeDimension] ASC,
	[EF_PrimaryStartDate] ASC,
	[EF_PrimaryEndDate] ASC,
	[EF_HasSecondaryTimeDimension] ASC,
	[EF_SecondaryStartDate] ASC,
	[EF_SecondaryEndDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]