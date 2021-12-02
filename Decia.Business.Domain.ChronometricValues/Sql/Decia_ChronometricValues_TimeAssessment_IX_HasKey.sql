CREATE NONCLUSTERED INDEX [IX_TimeAssessments_HasKey] ON [dbo].[TimeAssessments]
(
	[EF_ProjectGuid] ASC,
	[EF_RevisionNumber] ASC,
	[EF_ChronometricValueGuid] ASC,
	[EF_HasPrimaryTimeDimension] ASC,
	[EF_HasSecondaryTimeDimension] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]