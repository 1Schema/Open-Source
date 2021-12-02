CREATE PROCEDURE [dbo].[spDecia_ResultsLock_UnlockDeletion]
AS
BEGIN
	DELETE FROM #DeciaResults_DeleteLock;
	INSERT INTO #DeciaResults_DeleteLock VALUES (0);
END;
GO