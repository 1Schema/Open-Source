CREATE TRIGGER [dbo].[TRACKING___Decia_VariableTemplate___AllChanges] ON [dbo].[Decia_VariableTemplate]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	EXEC [dbo].[spDecia_ChangeState_IncrementLatest] NULL, NULL;
END;
GO