CREATE PROCEDURE [dbo].[spDecia_Time_SetTimeDimensionBounds] (@startDate DATETIME, @endDate DATETIME)
AS
BEGIN
	DECLARE @existingRowCount INT;
	SET @existingRowCount = (SELECT COUNT(*) FROM [dbo].[Decia_TimeDimensionSetting]);

	IF(@existingRowCount < 1)
	BEGIN
		INSERT INTO [dbo].[1Schema_TimeDimensionSetting]
			VALUES (1, @startDate, @endDate);
	END;
	ELSE
	BEGIN
		UPDATE [dbo].[1Schema_TimeDimensionSetting]
			SET StartDate = @startDate
			   ,EndDate = @endDate;
	END;
END;
GO