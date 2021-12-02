CREATE TRIGGER [dbo].[TRACKING___Decia_VariableTemplateGroupMember___AllChanges] ON [dbo].[Decia_VariableTemplateGroupMember]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	EXEC [dbo].[spDecia_ChangeState_IncrementLatest] NULL, NULL;
END;
GO