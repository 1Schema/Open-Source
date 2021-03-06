CREATE PROCEDURE [dbo].[spDecia_Cleanup_DeleteAllResults]
AS
BEGIN
	SET NOCOUNT ON;

	IF (object_id('tempdb..#DeciaResults_DeleteLock') IS NULL)
	BEGIN
		CREATE TABLE #DeciaResults_DeleteLock (IsLocked BIT);
	END;

	EXEC spDecia_ResultsLock_UnlockDeletion;

	DELETE FROM dr
		FROM  [dbo].[Decia_ResultSet] dr;
END;
GO