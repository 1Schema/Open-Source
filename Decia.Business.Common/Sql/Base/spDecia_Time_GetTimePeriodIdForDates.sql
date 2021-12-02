CREATE PROCEDURE [dbo].[spDecia_Time_GetTimePeriodIdForDates] (@startDate DATETIME, @endDate DATETIME, @id UNIQUEIDENTIFIER OUTPUT)
AS
BEGIN
	DECLARE @earliestSqlDateTime DATETIME = '1900/01/01';
	DECLARE @dotNetTicksPrimer BIGINT = 599266080000000000;
	DECLARE @dayToMsConverter BIGINT = 86400000;
	DECLARE @msToTickConverter BIGINT = 10000;

	DECLARE @startDate_DaysPart BIGINT = DATEDIFF(d, @earliestSqlDateTime, @startDate);
	DECLARE @endDate_DaysPart BIGINT = DATEDIFF(d, @earliestSqlDateTime, @endDate);

	DECLARE @startDate_DatePart DATETIME = DATEADD(d, @startDate_DaysPart, @earliestSqlDateTime);
	DECLARE @endDate_DatePart DATETIME = DATEADD(d, @endDate_DaysPart, @earliestSqlDateTime);

	DECLARE @startDate_MsPart BIGINT = DATEDIFF(ms, @startDate_DatePart, @startDate);
	DECLARE @endDate_MsPart BIGINT = DATEDIFF(ms, @endDate_DatePart, @endDate);

	DECLARE @startDate_Mod BIGINT = (DATEPART(mcs, @startDate) / 1000) - (@startDate_MsPart % 1000);
	DECLARE @endDate_Mod BIGINT = (DATEPART(mcs, @endDate) / 1000) - (@endDate_MsPart % 1000);

	DECLARE @startDate_AsLong BIGINT = (@dotNetTicksPrimer + (@msToTickConverter * ((@startDate_DaysPart * @dayToMsConverter) + @startDate_MsPart + @startDate_Mod)));
	DECLARE @endDate_AsLong BIGINT = (@dotNetTicksPrimer + (@msToTickConverter * ((@endDate_DaysPart * @dayToMsConverter) + @endDate_MsPart + @endDate_Mod)));

	DECLARE @startDate_AsBytes BINARY(8) = CONVERT(BINARY(8), REVERSE(CONVERT(VARBINARY(8), @startDate_AsLong)));
	DECLARE @endDate_AsBytes BINARY(8) = CONVERT(BINARY(8), REVERSE(CONVERT(VARBINARY(8), @endDate_AsLong)));

	SET @id = CONVERT(UNIQUEIDENTIFIER, @startDate_AsBytes + @endDate_AsBytes);

	PRINT @id;
END;
GO