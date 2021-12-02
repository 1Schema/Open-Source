CREATE PROCEDURE [dbo].[spDecia_Cleanup_DeleteAllOutdatedResults]
AS
BEGIN
	SET NOCOUNT ON;

	IF (object_id('tempdb..#DeciaResults_DeleteLock') IS NULL)
	BEGIN
		CREATE TABLE #DeciaResults_DeleteLock (IsLocked BIT);
	END;

	EXEC spDecia_ResultsLock_UnlockDeletion;

	DECLARE @latestChangeCount BIGINT;
	DECLARE @latestChangeDate DATETIME;

	EXEC [dbo].[spDecia_ChangeState_GetLatest] @latestChangeCount OUTPUT, @latestChangeDate OUTPUT;

	DELETE FROM dr
		FROM  [dbo].[Decia_ResultSet] dr
		WHERE (dr.[Metadata_ChangeCount] < @latestChangeCount);
END;
GO