ALTER FUNCTION dbo.Decia_Utils_SplitStrings
(
   @List NVARCHAR(MAX),
   @Delimiter NVARCHAR(255)
)
RETURNS TABLE
WITH SCHEMABINDING AS
RETURN
  -- Use a string splitting procedure such as those listed here http://sqlperformance.com/2012/07/t-sql-queries/split-strings