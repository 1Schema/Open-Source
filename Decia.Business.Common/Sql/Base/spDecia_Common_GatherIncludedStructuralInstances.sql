CREATE PROCEDURE [dbo].[spDecia_Common_GatherIncludedStructuralInstances] (@structuralInstance_TableName NVARCHAR(MAX), @structuralInstanceIds NVARCHAR(MAX) = NULL)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @structuralInstanceIds_Trimmed NVARCHAR(MAX);
	DECLARE @structuralTypeId UNIQUEIDENTIFIER;
	DECLARE @structuralInstance_IdColumnName NVARCHAR(MAX);
	DECLARE @structuralInstance_NameColumnName NVARCHAR(MAX);
	DECLARE @structuralInstance_SortingColumnName NVARCHAR(MAX);
	DECLARE @query NVARCHAR(MAX);

	IF object_id('tempdb..#Included_StructuralInstance') IS NULL
	BEGIN
		CREATE TABLE #Included_StructuralInstance (Id UNIQUEIDENTIFIER, Name NVARCHAR(MAX), SortingIndex INT, StructuralTypeId UNIQUEIDENTIFIER);
	END;

	SET @structuralTypeId = (SELECT dst.[Id] FROM [dbo].[Decia_StructuralType] dst WHERE dst.[Instance_Table_Name] = @structuralInstance_TableName);
	SET @structuralInstance_IdColumnName = (SELECT dvt.[Instance_Column_Name] FROM [dbo].[Decia_StructuralType] dst INNER JOIN [dbo].[Decia_VariableTemplate] dvt ON dst.[Instance_Id_VariableTemplateId] = dvt.[Id] WHERE dst.[Id] = @structuralTypeId);
	SET @structuralInstance_NameColumnName = (SELECT dvt.[Instance_Column_Name] FROM [dbo].[Decia_StructuralType] dst INNER JOIN [dbo].[Decia_VariableTemplate] dvt ON dst.[Instance_Name_VariableTemplateId] = dvt.[Id] WHERE dst.[Id] = @structuralTypeId);
	SET @structuralInstance_SortingColumnName = (SELECT dvt.[Instance_Column_Name] FROM [dbo].[Decia_StructuralType] dst INNER JOIN [dbo].[Decia_VariableTemplate] dvt ON dst.[Instance_Sorting_VariableTemplateId] = dvt.[Id] WHERE dst.[Id] = @structuralTypeId);
	
	
	IF (@structuralInstanceIds IS NOT NULL)
	BEGIN
		SET @structuralInstanceIds_Trimmed = REPLACE(@structuralInstanceIds, ' ', '');
	
		SET @query = 'INSERT INTO #Included_StructuralInstance ' +
					'SELECT DISTINCT sit.[' + @structuralInstance_IdColumnName + '], sit.[' + @structuralInstance_NameColumnName + '], sit.[' + @structuralInstance_SortingColumnName + '], ''' + CONVERT(NVARCHAR(50), @structuralTypeId) + ''' ' +
					'FROM [dbo].[' + @structuralInstance_TableName + '] sit ' +
					'WHERE ('',' + @structuralInstanceIds_Trimmed + ','') LIKE (''%,'' + CONVERT(varchar(50), sit.[' + @structuralInstance_IdColumnName + ']) + '',%'');';
		EXEC(@query);
	END
	ELSE
	BEGIN
		SET @query = 'INSERT INTO #Included_StructuralInstance ' +
					'SELECT DISTINCT sit.[' + @structuralInstance_IdColumnName + '], sit.[' + @structuralInstance_NameColumnName + '], sit.[' + @structuralInstance_SortingColumnName + '], ''' + CONVERT(NVARCHAR(50), @structuralTypeId) + ''' ' +
					'FROM [dbo].[' + @structuralInstance_TableName + '] sit;';
		EXEC(@query);
	END;

	SELECT sit.[Id], sit.[Name], sit.[SortingIndex], sit.[StructuralTypeId] FROM #Included_StructuralInstance sit;
END;
GO