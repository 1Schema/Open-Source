CREATE TRIGGER [dbo].[TRACKING___Decia_VariableTemplateDependency___AllChanges] ON [dbo].[Decia_VariableTemplateDependency]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	EXEC [dbo].[spDecia_ChangeState_IncrementLatest] NULL, NULL;
END;
GO