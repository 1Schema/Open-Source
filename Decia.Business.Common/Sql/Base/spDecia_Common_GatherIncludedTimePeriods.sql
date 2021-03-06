CREATE PROCEDURE [dbo].[spDecia_Common_GatherIncludedTimePeriods] (@startDate datetime, @endDate datetime)
AS
BEGIN
	SET NOCOUNT ON;

	IF object_id('tempdb..#Included_TimePeriod') IS NULL
	BEGIN
		CREATE TABLE #Included_TimePeriod (TimePeriodId UNIQUEIDENTIFIER, TimePeriodTypeId INT, StartDate DATETIME, EndDate DATETIME, OrderIndex INT);
	END;
	
	INSERT INTO #Included_TimePeriod
		SELECT dtp.[Id] AS [TimePeriodId], dtp.[TimePeriodTypeId], dtp.[StartDate], dtp.[EndDate], 0 AS [OrderIndex]
		FROM  [dbo].[Decia_TimePeriod] dtp
		WHERE ((@startDate <= dtp.[StartDate]) AND (dtp.[EndDate] <= @endDate))
			OR ((@startDate <= dtp.[StartDate]) AND (dtp.[StartDate] < @endDate) AND (@endDate <= dtp.[EndDate]))
			OR ((dtp.[StartDate] <= @startDate) AND (@startDate < dtp.[EndDate]) AND (dtp.[EndDate] <= @endDate));

	UPDATE #Included_TimePeriod
		SET [OrderIndex] = tp2.[OrderIndex]
		FROM #Included_TimePeriod tp1
			JOIN
			(SELECT [TimePeriodId], (ROW_NUMBER() OVER (PARTITION BY [TimePeriodTypeId] ORDER BY [StartDate]) - 1) AS [OrderIndex]
			FROM #Included_TimePeriod) tp2 ON tp1.[TimePeriodId] = tp2.[TimePeriodId];

	SELECT itp.[TimePeriodId], itp.[TimePeriodTypeId], itp.[StartDate], itp.[OrderIndex] FROM #Included_TimePeriod itp ORDER BY itp.[TimePeriodTypeId], itp.[OrderIndex];
END;
GO