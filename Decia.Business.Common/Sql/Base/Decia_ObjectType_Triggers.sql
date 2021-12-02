CREATE TRIGGER [dbo].[TRACKING___Decia_ObjectType___AllChanges] ON [dbo].[Decia_ObjectType]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	EXEC [dbo].[spDecia_ChangeState_IncrementLatest] NULL, NULL;
END;
GO