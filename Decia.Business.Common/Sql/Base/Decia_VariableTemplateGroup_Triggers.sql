CREATE TRIGGER [dbo].[TRACKING___Decia_VariableTemplateGroup___AllChanges] ON [dbo].[Decia_VariableTemplateGroup]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	EXEC [dbo].[spDecia_ChangeState_IncrementLatest] NULL, NULL;
END;
GO