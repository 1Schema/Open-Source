CREATE TRIGGER [dbo].[SINGLETON___Decia_Metadata___InsertOrDelete] ON [dbo].[Decia_Metadata]
AFTER INSERT, DELETE
AS
BEGIN
    DECLARE @actualCount INT;
    SET @actualCount = (SELECT COUNT(*) FROM [dbo].[Decia_Metadata]);

    IF (@actualCount <> 1)
    BEGIN
        ROLLBACK TRANSACTION;
        RAISERROR ('System must have exactly one Decia_Metadata row!', 16, 1);
    END;
END;
GO