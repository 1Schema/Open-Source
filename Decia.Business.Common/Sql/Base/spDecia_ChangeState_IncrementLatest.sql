CREATE PROCEDURE [dbo].[spDecia_ChangeState_IncrementLatest] (@changeCount BIGINT OUTPUT, @changeDate DATETIME OUTPUT)
AS
BEGIN
	UPDATE [dbo].[Decia_Metadata]
		SET [Latest_ChangeCount] = (dm.Latest_ChangeCount + 1), [Latest_ChangeDate] = GETDATE()
		FROM [dbo].[Decia_Metadata] dm;

	EXEC [dbo].[spDecia_ChangeState_GetLatest] @changeCount OUTPUT, @changeDate OUTPUT;
END;
GO