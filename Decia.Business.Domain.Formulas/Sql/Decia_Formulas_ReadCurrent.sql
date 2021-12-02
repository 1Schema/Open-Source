ALTER PROCEDURE Decia_Formulas_ReadCurrent  (@projectGuid uniqueidentifier, @revisionNumber bigint, @formulaGuidsAsText nvarchar(MAX) = NULL)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @AssertLatestRevisionIsEditable bigint = 0;
	DECLARE @AssertExactRevisionExists bigint = 0;

	CREATE TABLE #IncludedRevisionNumbers
	( EF_RevisionNumber bigint );

	CREATE TABLE #IncludedFormulaGuids
	( EF_FormulaGuid uniqueidentifier );

	CREATE TABLE #IncludedIdRows
	( EF_ProjectGuid uniqueidentifier, EF_RevisionNumber bigint, EF_FormulaGuid uniqueidentifier, EF_IsDeleted bit );

	CREATE TABLE #IncludedIdRows_Current
	( EF_ProjectGuid uniqueidentifier, EF_RevisionNumber bigint, EF_FormulaGuid uniqueidentifier, EF_IsDeleted bit );


	EXEC Decia_Projects_Revision_ReadChain @projectGuid, @revisionNumber, @assertLatestRevisionIsEditable, @assertExactRevisionExists;
	
	DECLARE @latestRevisionNumber bigint = (SELECT MAX(EF_RevisionNumber) FROM #IncludedRevisionNumbers);
	
	IF (@latestRevisionNumber IS NULL)
	BEGIN
		RAISERROR('The requested Revision could not be loaded.', 10, 11);
		RETURN -1;
	END;


	DECLARE @limitsFormulaGuids bit = 0;
	
	IF (@formulaGuidsAsText IS NOT NULL)
	BEGIN
		SET @limitsFormulaGuids = 1;

		DECLARE @formulaGuidsAsText_Trimmed NVARCHAR(MAX) = (',' + REPLACE(@formulaGuidsAsText, ' ', '') + ',');
	
		INSERT INTO #IncludedFormulaGuids
			SELECT [EF_FormulaGuid]
			FROM [dbo].[Formulae]
			WHERE @formulaGuidsAsText_Trimmed LIKE ('%,' + CONVERT(varchar(50), [EF_FormulaGuid]) + ',%');
	END;


	INSERT INTO #IncludedIdRows
		SELECT IdRow_Existing.[EF_ProjectGuid]
			,  CASE WHEN (IdRow_Deleted.[EF_RevisionNumber] IS NULL) THEN IdRow_Existing.[EF_RevisionNumber] WHEN (IdRow_Existing.[EF_RevisionNumber] > IdRow_Deleted.[EF_RevisionNumber]) THEN IdRow_Existing.[EF_RevisionNumber] ELSE IdRow_Deleted.[EF_RevisionNumber] END AS [EF_RevisionNumber]
			,  IdRow_Existing.[EF_FormulaGuid]
			,  CASE WHEN (IdRow_Deleted.[EF_RevisionNumber] IS NULL) THEN IdRow_Existing.[EF_IsDeleted] WHEN (IdRow_Existing.[EF_RevisionNumber] > IdRow_Deleted.[EF_RevisionNumber]) THEN IdRow_Existing.[EF_IsDeleted] ELSE IdRow_Deleted.[EF_IsDeleted] END AS [EF_IsDeleted]
		FROM
			(SELECT [EF_ProjectGuid], [EF_FormulaGuid], [EF_IsDeleted], MAX([EF_RevisionNumber]) AS [EF_RevisionNumber]
				FROM [dbo].[Formulae]
				WHERE ([EF_ProjectGuid] = @projectGuid)
					AND ([EF_RevisionNumber] <= @revisionNumber)
					AND ([EF_RevisionNumber] IN (SELECT [EF_RevisionNumber] FROM #IncludedRevisionNumbers))
					AND ((@limitsFormulaGuids = 0)
						OR ((@limitsFormulaGuids = 1) AND ([EF_FormulaGuid] IN (SELECT [EF_FormulaGuid] FROM #IncludedFormulaGuids))))
					AND ([EF_IsDeleted] = 0)
				GROUP BY [EF_ProjectGuid], [EF_FormulaGuid], [EF_IsDeleted]) AS IdRow_Existing
		LEFT OUTER JOIN
			(SELECT [EF_ProjectGuid], [EF_FormulaGuid], [EF_IsDeleted], MAX([EF_RevisionNumber]) AS [EF_RevisionNumber]
				FROM [dbo].[Formulae]
				WHERE ([EF_ProjectGuid] = @projectGuid)
					AND ([EF_RevisionNumber] <= @revisionNumber)
					AND ([EF_RevisionNumber] IN (SELECT [EF_RevisionNumber] FROM #IncludedRevisionNumbers))
					AND ((@limitsFormulaGuids = 0)
						OR ((@limitsFormulaGuids = 1) AND ([EF_FormulaGuid] IN (SELECT [EF_FormulaGuid] FROM #IncludedFormulaGuids))))
					AND ([EF_IsDeleted] = 1)
				GROUP BY [EF_ProjectGuid], [EF_FormulaGuid], [EF_IsDeleted]) AS IdRow_Deleted 
			ON (IdRow_Existing.[EF_ProjectGuid] = IdRow_Deleted.[EF_ProjectGuid]) AND (IdRow_Existing.[EF_FormulaGuid] = IdRow_Deleted.[EF_FormulaGuid]);

	INSERT INTO #IncludedIdRows_Current
		SELECT [EF_ProjectGuid], [EF_RevisionNumber], [EF_FormulaGuid], [EF_IsDeleted]
		FROM #IncludedIdRows
		WHERE ([EF_IsDeleted] = 0);


	SELECT ObjRow.[EF_ProjectGuid]
		,  ObjRow.[EF_RevisionNumber]
		,  ObjRow.[EF_FormulaGuid]
		,  ObjRow.[EF_ModelObjectType]
		,  ObjRow.[EF_ModelObjectIdAsInt]
		,  ObjRow.[EF_ModelObjectIdAsGuid]
		,  ObjRow.[EF_IsNavigationVariable]
		,  ObjRow.[EF_IsStructuralAggregation]
		,  ObjRow.[EF_IsStructuralFilter]
		,  ObjRow.[EF_IsTimeAggregation]
		,  ObjRow.[EF_IsTimeFilter]
		,  ObjRow.[EF_IsTimeShift]
		,  ObjRow.[EF_IsTimeIntrospection]
		,  ObjRow.[EF_RootExpressionGuid]
		,  ObjRow.[EF_ParentModelObjectType]
		,  ObjRow.[EF_ParentModelObjectRefs]
		,  ObjRow.[EF_CreatorGuid]
		,  ObjRow.[EF_CreationDate]
		,  ObjRow.[EF_OwnerType]
		,  ObjRow.[EF_OwnerGuid]
		,  ObjRow.[EF_IsDeleted]
		,  ObjRow.[EF_DeleterGuid]
		,  ObjRow.[EF_DeletionDate]
		FROM [dbo].[Formulae] ObjRow
		INNER JOIN #IncludedIdRows_Current IdRow
			ON (ObjRow.[EF_ProjectGuid] = IdRow.[EF_ProjectGuid]) AND (ObjRow.[EF_RevisionNumber] = IdRow.[EF_RevisionNumber]) AND (ObjRow.[EF_FormulaGuid] = IdRow.[EF_FormulaGuid]);

	SELECT ObjRow.[EF_ProjectGuid]
		,  ObjRow.[EF_RevisionNumber]
		,  ObjRow.[EF_FormulaGuid]
		,  ObjRow.[EF_ExpressionGuid]
		,  ObjRow.[EF_OperationGuid]
		,  ObjRow.[EF_ShowAsOperator]
		,  ObjRow.[EF_OuterParenthesesCount]
		FROM [dbo].[Expressions] ObjRow
		INNER JOIN #IncludedIdRows_Current IdRow
			ON (ObjRow.[EF_ProjectGuid] = IdRow.[EF_ProjectGuid]) AND (ObjRow.[EF_RevisionNumber] = IdRow.[EF_RevisionNumber]) AND (ObjRow.[EF_FormulaGuid] = IdRow.[EF_FormulaGuid]);

	SELECT ObjRow.[EF_ProjectGuid]
		,  ObjRow.[EF_RevisionNumber]
		,  ObjRow.[EF_FormulaGuid]
		,  ObjRow.[EF_ExpressionGuid]
		,  ObjRow.[EF_ArgumentIndex]
		,  ObjRow.[EF_ArgumentType]
		,  ObjRow.[EF_ParentOperationGuid]
		,  ObjRow.[EF_AutoJoinOrder]
		,  ObjRow.[EF_NestedExpressionGuid]
		,  ObjRow.[EF_ReferencedType]
		,  ObjRow.[EF_ReferencedId]
		,  ObjRow.[EF_ReferencedAlternateDimensionNumber]
		,  ObjRow.[EF_DirectDataType]
		,  ObjRow.[EF_DirectNumberValue]
		,  ObjRow.[EF_DirectTextValue]
		,  ObjRow.[EF_IsRefDeleted]
		,  ObjRow.[EF_DeletedRef_StructuralTypeText]
		,  ObjRow.[EF_DeletedRef_VariableTemplateText]
		FROM [dbo].[Arguments] ObjRow
		INNER JOIN #IncludedIdRows_Current IdRow
			ON (ObjRow.[EF_ProjectGuid] = IdRow.[EF_ProjectGuid]) AND (ObjRow.[EF_RevisionNumber] = IdRow.[EF_RevisionNumber]) AND (ObjRow.[EF_FormulaGuid] = IdRow.[EF_FormulaGuid]);
END;