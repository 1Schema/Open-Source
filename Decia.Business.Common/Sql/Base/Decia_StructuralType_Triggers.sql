CREATE TRIGGER [dbo].[TRACKING___Decia_StructuralType___AllChanges] ON [dbo].[Decia_StructuralType]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	EXEC [dbo].[spDecia_ChangeState_IncrementLatest] NULL, NULL;
END;
GO