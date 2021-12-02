ALTER PROCEDURE Decia_ChronometricValues_ReadCurrent  (@projectGuid uniqueidentifier, @revisionNumber bigint, @chronometricValueGuidsAsText nvarchar(MAX) = NULL)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @AssertLatestRevisionIsEditable bigint = 0;
	DECLARE @AssertExactRevisionExists bigint = 0;

	CREATE TABLE #IncludedRevisionNumbers
	( EF_RevisionNumber bigint );

	CREATE TABLE #IncludedChronometricValueGuids
	( EF_ChronometricValueGuid uniqueidentifier );

	CREATE TABLE #IncludedIdRows
	( EF_ProjectGuid uniqueidentifier, EF_RevisionNumber bigint, EF_ChronometricValueGuid uniqueidentifier, EF_IsDeleted bit );

	CREATE TABLE #IncludedIdRows_Current
	( EF_ProjectGuid uniqueidentifier, EF_RevisionNumber bigint, EF_ChronometricValueGuid uniqueidentifier, EF_IsDeleted bit );


	EXEC Decia_Projects_Revision_ReadChain @projectGuid, @revisionNumber, @assertLatestRevisionIsEditable, @assertExactRevisionExists;
	
	DECLARE @latestRevisionNumber bigint = (SELECT MAX(EF_RevisionNumber) FROM #IncludedRevisionNumbers);
	
	IF (@latestRevisionNumber IS NULL)
	BEGIN
		RAISERROR('The requested Revision could not be loaded.', 10, 11);
		RETURN -1;
	END;


	DECLARE @limitsChronometricValueGuids bit = 0;
	
	IF (@chronometricValueGuidsAsText IS NOT NULL)
	BEGIN
		SET @limitsChronometricValueGuids = 1;

		DECLARE @chronometricValueGuidsAsText_Trimmed NVARCHAR(MAX) = (',' + REPLACE(@chronometricValueGuidsAsText, ' ', '') + ',');
	
		INSERT INTO #IncludedChronometricValueGuids
			SELECT [EF_ChronometricValueGuid]
			FROM [dbo].[ChronometricValues]
			WHERE @chronometricValueGuidsAsText_Trimmed LIKE ('%,' + CONVERT(varchar(50), [EF_ChronometricValueGuid]) + ',%');
	END;


	INSERT INTO #IncludedIdRows
		SELECT IdRow_Existing.[EF_ProjectGuid]
			,  CASE WHEN (IdRow_Deleted.[EF_RevisionNumber] IS NULL) THEN IdRow_Existing.[EF_RevisionNumber] WHEN (IdRow_Existing.[EF_RevisionNumber] > IdRow_Deleted.[EF_RevisionNumber]) THEN IdRow_Existing.[EF_RevisionNumber] ELSE IdRow_Deleted.[EF_RevisionNumber] END AS [EF_RevisionNumber]
			,  IdRow_Existing.[EF_ChronometricValueGuid]
			,  CASE WHEN (IdRow_Deleted.[EF_RevisionNumber] IS NULL) THEN IdRow_Existing.[EF_IsDeleted] WHEN (IdRow_Existing.[EF_RevisionNumber] > IdRow_Deleted.[EF_RevisionNumber]) THEN IdRow_Existing.[EF_IsDeleted] ELSE IdRow_Deleted.[EF_IsDeleted] END AS [EF_IsDeleted]
		FROM
			(SELECT [EF_ProjectGuid], [EF_ChronometricValueGuid], [EF_IsDeleted], MAX([EF_RevisionNumber]) AS [EF_RevisionNumber]
				FROM [dbo].[ChronometricValues]
				WHERE ([EF_ProjectGuid] = @projectGuid)
					AND ([EF_RevisionNumber] <= @revisionNumber)
					AND ([EF_RevisionNumber] IN (SELECT [EF_RevisionNumber] FROM #IncludedRevisionNumbers))
					AND ((@limitsChronometricValueGuids = 0)
						OR ((@limitsChronometricValueGuids = 1) AND ([EF_ChronometricValueGuid] IN (SELECT [EF_ChronometricValueGuid] FROM #IncludedChronometricValueGuids))))
					AND ([EF_IsDeleted] = 0)
				GROUP BY [EF_ProjectGuid], [EF_ChronometricValueGuid], [EF_IsDeleted]) AS IdRow_Existing
		LEFT OUTER JOIN
			(SELECT [EF_ProjectGuid], [EF_ChronometricValueGuid], [EF_IsDeleted], MAX([EF_RevisionNumber]) AS [EF_RevisionNumber]
				FROM [dbo].[ChronometricValues]
				WHERE ([EF_ProjectGuid] = @projectGuid)
					AND ([EF_RevisionNumber] <= @revisionNumber)
					AND ([EF_RevisionNumber] IN (SELECT [EF_RevisionNumber] FROM #IncludedRevisionNumbers))
					AND ((@limitsChronometricValueGuids = 0)
						OR ((@limitsChronometricValueGuids = 1) AND ([EF_ChronometricValueGuid] IN (SELECT [EF_ChronometricValueGuid] FROM #IncludedChronometricValueGuids))))
					AND ([EF_IsDeleted] = 1)
				GROUP BY [EF_ProjectGuid], [EF_ChronometricValueGuid], [EF_IsDeleted]) AS IdRow_Deleted 
			ON (IdRow_Existing.[EF_ProjectGuid] = IdRow_Deleted.[EF_ProjectGuid]) AND (IdRow_Existing.[EF_ChronometricValueGuid] = IdRow_Deleted.[EF_ChronometricValueGuid]);

	INSERT INTO #IncludedIdRows_Current
		SELECT [EF_ProjectGuid], [EF_RevisionNumber], [EF_ChronometricValueGuid], [EF_IsDeleted]
		FROM #IncludedIdRows
		WHERE ([EF_IsDeleted] = 0);


	SELECT ObjRow.[EF_ProjectGuid]
		,  ObjRow.[EF_RevisionNumber]
		,  ObjRow.[EF_ChronometricValueGuid]
		,  ObjRow.[EF_DataType]
		,  ObjRow.[EF_DefaultNumberValue]
		,  ObjRow.[EF_DefaultTextValue]
		,  ObjRow.[EF_ParentModelObjectType]
		,  ObjRow.[EF_ParentModelObjectRefs]
		,  ObjRow.[EF_CreatorGuid]
		,  ObjRow.[EF_CreationDate]
		,  ObjRow.[EF_OwnerType]
		,  ObjRow.[EF_OwnerGuid]
		,  ObjRow.[EF_IsDeleted]
		,  ObjRow.[EF_DeleterGuid]
		,  ObjRow.[EF_DeletionDate]
		FROM [dbo].[ChronometricValues] ObjRow
		INNER JOIN #IncludedIdRows_Current IdRow
			ON (ObjRow.[EF_ProjectGuid] = IdRow.[EF_ProjectGuid]) AND (ObjRow.[EF_RevisionNumber] = IdRow.[EF_RevisionNumber]) AND (ObjRow.[EF_ChronometricValueGuid] = IdRow.[EF_ChronometricValueGuid]);

	SELECT ObjRow.[EF_ProjectGuid]
		,  ObjRow.[EF_RevisionNumber]
		,  ObjRow.[EF_ChronometricValueGuid]
		,  ObjRow.[EF_TimeDimensionType]
		,  ObjRow.[EF_TimeValueType]
		,  ObjRow.[EF_TimePeriodType]
		,  ObjRow.[EF_FirstPeriodStartDate]
		,  ObjRow.[EF_LastPeriodEndDate]
		FROM [dbo].[SaveableTimeDimensions] ObjRow
		INNER JOIN #IncludedIdRows_Current IdRow
			ON (ObjRow.[EF_ProjectGuid] = IdRow.[EF_ProjectGuid]) AND (ObjRow.[EF_RevisionNumber] = IdRow.[EF_RevisionNumber]) AND (ObjRow.[EF_ChronometricValueGuid] = IdRow.[EF_ChronometricValueGuid]);

	SELECT ObjRow.[EF_ProjectGuid]
		,  ObjRow.[EF_RevisionNumber]
		,  ObjRow.[EF_ChronometricValueGuid]
		,  ObjRow.[EF_TimeAssessmentGuid]
		,  ObjRow.[EF_HasPrimaryTimeDimension]
		,  ObjRow.[EF_PrimaryStartDate]
		,  ObjRow.[EF_PrimaryEndDate]
		,  ObjRow.[EF_HasSecondaryTimeDimension]
		,  ObjRow.[EF_SecondaryStartDate]
		,  ObjRow.[EF_SecondaryEndDate]
		,  ObjRow.[EF_DataType]
		,  ObjRow.[EF_NumberValue]
		,  ObjRow.[EF_TextValue]
		FROM [dbo].[TimeAssessments] ObjRow
		INNER JOIN #IncludedIdRows_Current IdRow
			ON (ObjRow.[EF_ProjectGuid] = IdRow.[EF_ProjectGuid]) AND (ObjRow.[EF_RevisionNumber] = IdRow.[EF_RevisionNumber]) AND (ObjRow.[EF_ChronometricValueGuid] = IdRow.[EF_ChronometricValueGuid]);
END;