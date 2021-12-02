CREATE PROCEDURE [dbo].[spDecia_ResultsLock_AssertIsNotReadOnly]
AS
BEGIN
	DECLARE @isLocked BIT;
	SET @isLocked = 1;

	IF object_id('tempdb..#DeciaResults_DeleteLock') IS NOT NULL
	BEGIN
		SET @isLocked = (SELECT [IsLocked] FROM #DeciaResults_DeleteLock);
	END;

	IF (@isLocked <> 0)
	BEGIN
		ROLLBACK TRANSACTION;
		RAISERROR ('Cannot change Decia Results since they are currently read-only!', 16, 1);
	END;
END;
GO