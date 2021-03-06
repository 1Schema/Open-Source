CREATE PROCEDURE [dbo].[spDecia_Common_GetTempTableCreationText](@tableCreationText NVARCHAR(MAX) OUT)
AS
BEGIN 
    SET @tableCreationText = N'
	IF object_id(''tempdb..#DeciaResults_DeleteLock'') IS NULL
	BEGIN
		CREATE TABLE #DeciaResults_DeleteLock (IsLocked BIT);
	END;
	IF object_id(''tempdb..#Included_StructuralInstance'') IS NULL
	BEGIN
		CREATE TABLE #Included_StructuralInstance (Id UNIQUEIDENTIFIER, Name NVARCHAR(MAX), SortingIndex INT, StructuralTypeId UNIQUEIDENTIFIER);
	END;
	';
END;
GO