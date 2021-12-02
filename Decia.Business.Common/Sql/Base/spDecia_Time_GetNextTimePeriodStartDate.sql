CREATE PROCEDURE [dbo].[spDecia_Time_GetNextTimePeriodStartDate] (@timePeriodTypeId INT, @currentDate DATETIME, @nextDate DATETIME OUTPUT)
AS
BEGIN
	DECLARE @timePeriodTypeName VARCHAR(24);
	DECLARE @timePeriodTypeDatePart VARCHAR(24);
	DECLARE @multiplier FLOAT = 1.0;

	SET @timePeriodTypeName = (SELECT [Name] FROM [dbo].[1Schema_TimePeriodType] WHERE [Id] = @timePeriodTypeId);
	SET @timePeriodTypeDatePart = (SELECT [DatePart_Value] FROM [dbo].[1Schema_TimePeriodType] WHERE [Id] = @timePeriodTypeId);

	DECLARE @thisMonthsStartDate DATETIME = DATEFROMPARTS(DATEPART(yy, @currentDate), DATEPART(mm, @currentDate), 1);
    DECLARE @nextMonthsStartDate DATETIME = DATEADD(mm, (@multiplier * 1), @thisMonthsStartDate);
	DECLARE @durationInSeconds INT = DATEDIFF(ss, @thisMonthsStartDate, @nextMonthsStartDate);

    IF (@timePeriodTypeName = 'Years')
	BEGIN
		SET @nextDate = DATEADD(yy, (@multiplier * 1), @currentDate);
	END;
	ELSE IF (@timePeriodTypeName = 'HalfYears')
	BEGIN
		SET @nextDate = DATEADD(mm, (@multiplier * 6), @currentDate);
	END;
	ELSE IF (@timePeriodTypeName = 'QuarterYears')
	BEGIN
		SET @nextDate = DATEADD(mm, (@multiplier * 3), @currentDate);
	END;
	ELSE IF (@timePeriodTypeName = 'Months')
	BEGIN
		SET @nextDate = DATEADD(mm, (@multiplier * 1), @currentDate);
	END;
	ELSE IF (@timePeriodTypeName = 'HalfMonths')
	BEGIN
		DECLARE @halfDurationInSeconds INT = (@durationInSeconds / 2.0);
		SET @nextDate = DATEADD(ss, @halfDurationInSeconds, @currentDate);
	END;
	ELSE IF (@timePeriodTypeName = 'QuarterMonths')
	BEGIN
		DECLARE @quarterDurationInSeconds INT = (@durationInSeconds / 4.0);
		SET @nextDate = DATEADD(ss, @quarterDurationInSeconds, @currentDate);
	END;
	ELSE IF (@timePeriodTypeName = 'Days')
	BEGIN
		SET @nextDate = DATEADD(dd, (@multiplier * 1), @currentDate);
	END;
	ELSE
	BEGIN
		RAISERROR('Unrecognized TimePeriodType encountered!', 16, 1);
	END;
END;
GO