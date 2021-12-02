CREATE PROCEDURE [dbo].[spDecia_Common_GatherIncludedVariableTemplateGroups] (@variableTemplateIds NVARCHAR(MAX) = NULL)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @variableTemplateIds_Trimmed NVARCHAR(MAX);
	DECLARE @variableTemplate_MaxCount INT;
	DECLARE @variableTemplate_CurrentCount INT;
	DECLARE @variableTemplate_UpdatedCount INT;

	DECLARE @VariableTemplateList TABLE (Id UNIQUEIDENTIFIER);

	IF object_id('tempdb..#Included_VariableTemplate') IS NULL
	BEGIN
		CREATE TABLE #Included_VariableTemplate (VariableTemplateId UNIQUEIDENTIFIER, VariableTemplateGroupId UNIQUEIDENTIFIER, OrderIndex INT);
	END;
	IF object_id('tempdb..#Included_VariableTemplateGroup') IS NULL
	BEGIN
		CREATE TABLE #Included_VariableTemplateGroup (VariableTemplateGroupId UNIQUEIDENTIFIER, OrderIndex INT);
	END;


	IF (@variableTemplateIds IS NOT NULL)
	BEGIN
		SET @variableTemplateIds_Trimmed = REPLACE(@variableTemplateIds, ' ', '');
	
		INSERT INTO @VariableTemplateList
			SELECT DISTINCT dvt.[Id]
			FROM  [dbo].[Decia_VariableTemplate] dvt
			WHERE (',' + @variableTemplateIds_Trimmed + ',') LIKE ('%,' + CONVERT(varchar(50), dvt.[Id]) + ',%');

		SET @variableTemplate_MaxCount = (SELECT COUNT(*) FROM [dbo].[Decia_VariableTemplate]);
		SET @variableTemplate_CurrentCount = (SELECT COUNT(*) FROM @VariableTemplateList);
		SET @variableTemplate_UpdatedCount = (@variableTemplate_CurrentCount + 1);

		WHILE (@variableTemplate_CurrentCount < @variableTemplate_UpdatedCount)
		BEGIN
			INSERT INTO @VariableTemplateList
				SELECT dvtd.[Dependency_VariableTemplateId]
				FROM [dbo].[Decia_VariableTemplateDependency] dvtd INNER JOIN @VariableTemplateList vtl ON dvtd.[Result_VariableTemplateId] = vtl.[Id];

			SET @variableTemplate_CurrentCount = @variableTemplate_UpdatedCount;
			SET @variableTemplate_UpdatedCount = (SELECT COUNT(DISTINCT vtl.[Id]) FROM @VariableTemplateList vtl);
		END;
	END
	ELSE
	BEGIN
		INSERT INTO @VariableTemplateList
			SELECT DISTINCT dvt.[Id]
			FROM [dbo].[Decia_VariableTemplate] dvt;
	END;


	INSERT INTO #Included_VariableTemplate
		SELECT DISTINCT vtl.[Id], dvtg.[Id], dvtg.[ProcessingIndex]
		FROM @VariableTemplateList vtl
			INNER JOIN [dbo].[Decia_VariableTemplateGroupMember] dvtgm ON vtl.[Id] = dvtgm.[VariableTemplateId]
			INNER JOIN [dbo].[Decia_VariableTemplateGroup] dvtg ON dvtgm.[VariableTemplateGroupId] = dvtg.[Id];

	INSERT INTO #Included_VariableTemplateGroup
		SELECT DISTINCT ivt.[VariableTemplateGroupId], ivt.[OrderIndex]
		FROM #Included_VariableTemplate ivt;
END;
GO