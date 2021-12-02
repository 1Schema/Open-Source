CREATE TRIGGER [dbo].[TRACKING___Decia_DataType___AllChanges] ON [dbo].[Decia_DataType]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	EXEC [dbo].[spDecia_ChangeState_IncrementLatest] NULL, NULL;
END;
GO