CREATE PROCEDURE [dbo].[spDecia_ChangeState_GetLatest] (@changeCount BIGINT OUTPUT, @changeDate DATETIME OUTPUT)
AS
BEGIN
	SET @changeCount = (SELECT [Latest_ChangeCount] FROM [dbo].[Decia_Metadata]);
	SET @changeDate = (SELECT [Latest_ChangeDate] FROM [dbo].[Decia_Metadata]);
END;
GO