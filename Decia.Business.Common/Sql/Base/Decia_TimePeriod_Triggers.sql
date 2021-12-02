CREATE TRIGGER [dbo].[CHANGE_TRACKING___Decia_TimePeriod___AllChanges_Pre] ON [dbo].[Decia_TimePeriod]
INSTEAD OF DELETE
AS
BEGIN
    IF (object_id('tempdb..#DeciaResults_DeleteLock') IS NULL)
    BEGIN
        CREATE TABLE #DeciaResults_DeleteLock (IsLocked BIT);
    END;

    EXEC [dbo].[spDecia_ResultsLock_UnlockDeletion];

    DELETE t FROM [dbo].[Decia_TimePeriod] t WHERE t.[Id] IN (SELECT d.[Id] FROM deleted d);
END;
GO

CREATE TRIGGER [dbo].[CHANGE_TRACKING___Decia_TimePeriod___AllChanges_Post] ON [dbo].[Decia_TimePeriod]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	EXEC [dbo].[spDecia_ChangeState_IncrementLatest] NULL, NULL;
END;
GO