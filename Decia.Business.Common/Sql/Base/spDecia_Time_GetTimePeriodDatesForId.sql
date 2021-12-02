CREATE PROCEDURE [dbo].[spDecia_Time_GetTimePeriodDatesForId] (@id UNIQUEIDENTIFIER, @startDate DATETIME OUTPUT, @endDate DATETIME OUTPUT)
AS
BEGIN
	DECLARE @earliestSqlDateTime DATETIME = '1900/01/01';
	DECLARE @dotNetTicksPrimer BIGINT = 599266080000000000;
	DECLARE @dayToMsConverter BIGINT = 86400000;
	DECLARE @msToTickConverter BIGINT = 10000;

	DECLARE @id_AsBytes AS BINARY(16) = CONVERT(BINARY(16), @id);

	DECLARE @startDate_AsBytes BINARY(8) = SUBSTRING(@id_AsBytes, 1, 8);
	DECLARE @endDate_AsBytes BINARY(8) = SUBSTRING(@id_AsBytes, 9, 8);

	DECLARE @startDate_AsLong BIGINT = CONVERT(BIGINT, CONVERT(VARBINARY(8), REVERSE(CONVERT(VARBINARY(8), @startDate_AsBytes))));
	DECLARE @endDate_AsLong BIGINT = CONVERT(BIGINT, CONVERT(VARBINARY(8), REVERSE(CONVERT(VARBINARY(8), @endDate_AsBytes))));

	DECLARE @startDate_AsMs BIGINT = (@startDate_AsLong - @dotNetTicksPrimer) / @msToTickConverter;
	DECLARE @endDate_AsMs BIGINT = (@endDate_AsLong - @dotNetTicksPrimer) / @msToTickConverter;

	SET @startDate = DATEADD(ms, (@startDate_AsMs % @dayToMsConverter), DATEADD(d, (@startDate_AsMs / @dayToMsConverter), @earliestSqlDateTime));
	SET @endDate = DATEADD(ms, (@endDate_AsMs % @dayToMsConverter), DATEADD(d, (@endDate_AsMs / @dayToMsConverter), @earliestSqlDateTime));

	PRINT CONVERT(VARCHAR, @startDate, 121);
	PRINT CONVERT(VARCHAR, @endDate, 121);
END;
GO