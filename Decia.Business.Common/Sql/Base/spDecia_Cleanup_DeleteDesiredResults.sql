CREATE PROCEDURE [dbo].[spDecia_Cleanup_DeleteDesiredResults] (@resultSetIds NVARCHAR(MAX))
AS
BEGIN
	SET NOCOUNT ON;

	IF (object_id('tempdb..#DeciaResults_DeleteLock') IS NULL)
	BEGIN
		CREATE TABLE #DeciaResults_DeleteLock (IsLocked BIT);
	END;

	EXEC spDecia_ResultsLock_UnlockDeletion;

	DECLARE @resultSetIds_Trimmed NVARCHAR(MAX);
	SET @resultSetIds_Trimmed = REPLACE(@resultSetIds, ' ', '');

	DELETE FROM dr
		FROM  [dbo].[Decia_ResultSet] dr
		WHERE (',' + @resultSetIds_Trimmed + ',') LIKE ('%,' + CONVERT(varchar(50), dr.[Id]) + ',%');
END;
GO