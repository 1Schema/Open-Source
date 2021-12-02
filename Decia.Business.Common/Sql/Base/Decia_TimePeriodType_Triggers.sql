CREATE TRIGGER [dbo].[TRACKING___Decia_TimePeriodType___AllChanges] ON [dbo].[Decia_TimePeriodType]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	EXEC [dbo].[spDecia_ChangeState_IncrementLatest] NULL, NULL;
END;
GO