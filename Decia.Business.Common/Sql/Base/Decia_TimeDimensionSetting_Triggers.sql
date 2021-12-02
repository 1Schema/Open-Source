CREATE TRIGGER [dbo].[TRACKING___Decia_TimeDimensionSetting___AllChanges] ON [dbo].[Decia_TimeDimensionSetting]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	DECLARE @startDate DATETIME;
	DECLARE @endDate DATETIME;

	IF ((SELECT COUNT(*) FROM INSERTED) > 0)
	BEGIN
		SET @startDate = (SELECT [StartDate] FROM inserted);
		SET @endDate = (SELECT [EndDate] FROM inserted);
	END;
	ELSE IF ((SELECT COUNT(*) FROM DELETED) > 0)
	BEGIN
		SET @startDate = (SELECT [StartDate] FROM deleted);
		SET @endDate = (SELECT [EndDate] FROM deleted);
	END;
	ELSE
	BEGIN
		SET @startDate = (SELECT [StartDate] FROM [dbo].[Decia_TimeDimensionSetting]);
		SET @endDate = (SELECT [EndDate] FROM [dbo].[Decia_TimeDimensionSetting]);
	END;

	DECLARE @zeroIndexNonNullDate DATETIME = @startDate;
	DECLARE @minDateIsFirstStart BIT = 1;

	DECLARE @TimePeriodTypesUsed TABLE
	(
		[Id] [int] NOT NULL,
		[Name] [nvarchar](max) NOT NULL,
		[IsForever] [bit] NOT NULL,
		[EstimateInDays] [float] NOT NULL,
		[MinValidDays] [float] NOT NULL,
		[MaxValidDays] [float] NOT NULL,
		[DatePart_Value] [nvarchar](20) NOT NULL,
		[DatePart_Multiplier] [float] NOT NULL
	);

	DECLARE @TimePeriodGenerated TABLE
	(
		[Id] [uniqueidentifier] NOT NULL,
		[TimePeriodTypeId] [int] NOT NULL,
		[StartDate] [datetime] NOT NULL,
		[EndDate] [datetime] NOT NULL,
		[IsForever] [bit] NOT NULL
	);

	INSERT INTO @TimePeriodTypesUsed
		SELECT tpt.[Id], tpt.[Name], tpt.[IsForever], tpt.[EstimateInDays], tpt.[MinValidDays], tpt.[MaxValidDays], tpt.[DatePart_Value], tpt.[DatePart_Multiplier] 
		FROM [dbo].[Decia_TimePeriodType] tpt
			INNER JOIN 
				((SELECT [PrimaryTimePeriodTypeId] AS [TimePeriodTypeId]
				  FROM [dbo].[Decia_VariableTemplate]
				  WHERE [PrimaryTimePeriodTypeId] IS NOT NULL)
				  UNION
				(SELECT [SecondaryTimePeriodTypeId] AS [TimePeriodTypeId]
				  FROM [dbo].[Decia_VariableTemplate]
				  WHERE [SecondaryTimePeriodTypeId] IS NOT NULL)) tptu
			ON tpt.[Id] = tptu.[TimePeriodTypeId];

	DECLARE @timePeriodTypeCount BIT;
	SET @timePeriodTypeCount = (SELECT COUNT([Id]) FROM @TimePeriodTypesUsed);

	IF(@timePeriodTypeCount < 1)
		RETURN;

	DECLARE @timePeriodTypeId INT = 0;
	DECLARE @endDateDiffInMs INT = -3;

	DECLARE TimePeriodType_Cursor CURSOR FOR SELECT [Id] FROM @TimePeriodTypesUsed;
	OPEN TimePeriodType_Cursor;  
	FETCH NEXT FROM TimePeriodType_Cursor INTO @timePeriodTypeId;
	WHILE @@FETCH_STATUS = 0  
	BEGIN
		DECLARE @indexOfFirstStartDate INT = 0;
		DECLARE @indexLoopStartDate DATETIME = @startDate;
		DECLARE @indexLoopTerminationDate DATETIME = @zeroIndexNonNullDate;

		WHILE(@indexLoopStartDate < @indexLoopTerminationDate)
		BEGIN
			DECLARE @indexLoopEndDate DATETIME;

			EXEC spDecia_Time_GetNextTimePeriodStartDate @timePeriodTypeId, @indexLoopStartDate, @indexLoopEndDate OUTPUT;
			SET @indexLoopEndDate = DATEADD(ms, @endDateDiffInMs, @indexLoopEndDate);

			IF (@indexLoopTerminationDate <= @indexLoopEndDate)
				BREAK;
        
			SET @indexOfFirstStartDate = (@indexOfFirstStartDate + 1);

			EXEC spDecia_Time_GetNextTimePeriodStartDate @timePeriodTypeId, @indexLoopStartDate, @indexLoopStartDate OUTPUT;
		END; 

		DECLARE @periodStartDate DATETIME = @startDate;
		DECLARE @currentIndex INT = @indexOfFirstStartDate;

		WHILE(@periodStartDate < @endDate)
		BEGIN
			DECLARE @periodEndDate DATETIME;
			DECLARE @periodId UNIQUEIDENTIFIER = NULL;

			EXEC spDecia_Time_GetNextTimePeriodStartDate @timePeriodTypeId, @periodStartDate, @periodEndDate OUTPUT;
			SET @periodEndDate = DATEADD(ms, @endDateDiffInMs, @periodEndDate);
		
			EXEC spDecia_Time_GetTimePeriodIdForDates @periodStartDate, @periodEndDate, @periodId OUTPUT;
        
			INSERT INTO @TimePeriodGenerated
				VALUES (@periodId, @timePeriodTypeId, @periodStartDate, @periodEndDate, 0);

			EXEC spDecia_Time_GetNextTimePeriodStartDate @timePeriodTypeId, @periodStartDate, @periodStartDate OUTPUT;

			SET @currentIndex = (@currentIndex + 1);
		END;

		FETCH NEXT FROM TimePeriodType_Cursor INTO @timePeriodTypeId;
	END;  
	CLOSE TimePeriodType_Cursor;  
	DEALLOCATE TimePeriodType_Cursor;

	DELETE FROM [dbo].[Decia_TimePeriod]
		FROM [dbo].[Decia_TimePeriod]
		WHERE [Id] NOT IN (SELECT [Id] FROM @TimePeriodGenerated);

	INSERT INTO [dbo].[Decia_TimePeriod]
		SELECT [Id], [TimePeriodTypeId], [StartDate], [EndDate], [IsForever]
		FROM @TimePeriodGenerated
		WHERE [Id] NOT IN (SELECT [Id] FROM [dbo].[Decia_TimePeriod]);
	EXEC [dbo].[spDecia_ChangeState_IncrementLatest] NULL, NULL;
END;
GO

CREATE TRIGGER [dbo].[SINGLETON___Decia_TimeDimensionSetting___InsertOrDelete] ON [dbo].[Decia_TimeDimensionSetting]
AFTER INSERT, DELETE
AS
BEGIN
    DECLARE @actualCount INT;
    SET @actualCount = (SELECT COUNT(*) FROM [dbo].[Decia_TimeDimensionSetting]);

    IF (@actualCount <> 1)
    BEGIN
        ROLLBACK TRANSACTION;
        RAISERROR ('System must have exactly one Decia_TimeDimensionSetting row!', 16, 1);
    END;
END;
GO