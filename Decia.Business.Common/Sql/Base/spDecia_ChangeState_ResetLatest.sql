CREATE PROCEDURE [dbo].[spDecia_ChangeState_ResetLatest] (@changeCount BIGINT OUTPUT, @changeDate DATETIME OUTPUT)
AS
BEGIN
	UPDATE [dbo].[Decia_Metadata]
		SET [Latest_ChangeCount] = 0, [Latest_ChangeDate] = GETDATE();

	EXEC [dbo].[spDecia_ChangeState_GetLatest] @changeCount OUTPUT, @changeDate OUTPUT;
END;
GO