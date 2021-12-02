CREATE PROCEDURE [dbo].[spDecia_ResultsLock_LockDeletion]
AS
BEGIN
	DELETE FROM #DeciaResults_DeleteLock;
	INSERT INTO #DeciaResults_DeleteLock VALUES (1);
END;
GO