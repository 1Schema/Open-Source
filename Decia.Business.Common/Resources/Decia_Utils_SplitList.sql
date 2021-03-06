ALTER PROCEDURE [dbo].[Decia_Utils_SplitList]
(
	@projectGuid uniqueidentifier,
	@modelTemplateNumber int, 
	@tableName nvarchar(MAX),
	@idColumnName nvarchar(MAX),
	@list nvarchar(MAX),
	@delimiter nvarchar(255)
)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @projectGuidAsText nvarchar(50);
	DECLARE @isNotRevisionSpecific bit;
	DECLARE @modelTemplateNumberAsText nvarchar(50);
	DECLARE @listAsTrimmed nvarchar(MAX);
	DECLARE @query nvarchar(MAX);
	
	SET @projectGuidAsText = @projectGuid;
	SET @isNotRevisionSpecific = 0;
	SET @modelTemplateNumberAsText = @modelTemplateNumber;
	SET @listAsTrimmed = REPLACE(@list, ' ', '');

	IF (OBJECT_ID('tempdb..#IncludedRevisionNumbers') IS NULL)
	BEGIN
		SET @isNotRevisionSpecific = 1;
	END;

	SET @query = 'SELECT DISTINCT ' + @idColumnName + ' ' +
					'FROM ' + @tableName + ' ' +
					'WHERE (EF_ProjectGuid = ''' + @projectGuidAsText + ''') ' +
							CASE @isNotRevisionSpecific WHEN 1 THEN '' ELSE 'AND (EF_RevisionNumber IN (SELECT * FROM #IncludedRevisionNumbers)) ' END +
							'AND (EF_ModelTemplateNumber = ' + @modelTemplateNumberAsText + ') ' +
							'AND (''' + ',' + @listAsTrimmed + ',' + ''' LIKE ' + '''%,'' + ' + 'CONVERT(varchar(50), ' + @idColumnName + ')' + ' + '',%''' + ')';

	EXEC(@query);
END;