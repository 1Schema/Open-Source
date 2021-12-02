ALTER PROCEDURE Decia_CompoundUnits_ReadCurrent  (@projectGuid uniqueidentifier, @revisionNumber bigint, @compoundUnitGuidsAsText nvarchar(MAX) = NULL)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @AssertLatestRevisionIsEditable bigint = 0;
	DECLARE @AssertExactRevisionExists bigint = 0;

	CREATE TABLE #IncludedRevisionNumbers
	( EF_RevisionNumber bigint );

	CREATE TABLE #IncludedCompoundUnitGuids
	( EF_CompoundUnitGuid uniqueidentifier );

	CREATE TABLE #IncludedIdRows
	( EF_ProjectGuid uniqueidentifier, EF_RevisionNumber bigint, EF_CompoundUnitGuid uniqueidentifier, EF_IsDeleted bit );

	CREATE TABLE #IncludedIdRows_Current
	( EF_ProjectGuid uniqueidentifier, EF_RevisionNumber bigint, EF_CompoundUnitGuid uniqueidentifier, EF_IsDeleted bit );


	EXEC Decia_Projects_Revision_ReadChain @projectGuid, @revisionNumber, @assertLatestRevisionIsEditable, @assertExactRevisionExists;
	
	DECLARE @latestRevisionNumber bigint = (SELECT MAX(EF_RevisionNumber) FROM #IncludedRevisionNumbers);
	
	IF (@latestRevisionNumber IS NULL)
	BEGIN
		RAISERROR('The requested Revision could not be loaded.', 10, 11);
		RETURN -1;
	END;


	DECLARE @limitsCompoundUnitGuids bit = 0;
	
	IF (@compoundUnitGuidsAsText IS NOT NULL)
	BEGIN
		SET @limitsCompoundUnitGuids = 1;

		DECLARE @compoundUnitGuidsAsText_Trimmed NVARCHAR(MAX) = (',' + REPLACE(@compoundUnitGuidsAsText, ' ', '') + ',');
	
		INSERT INTO #IncludedCompoundUnitGuids
			SELECT [EF_CompoundUnitGuid]
			FROM [dbo].[CompoundUnits]
			WHERE @compoundUnitGuidsAsText_Trimmed LIKE ('%,' + CONVERT(varchar(50), [EF_CompoundUnitGuid]) + ',%');
	END;


	INSERT INTO #IncludedIdRows
		SELECT IdRow_Existing.[EF_ProjectGuid]
			,  CASE WHEN (IdRow_Deleted.[EF_RevisionNumber] IS NULL) THEN IdRow_Existing.[EF_RevisionNumber] WHEN (IdRow_Existing.[EF_RevisionNumber] > IdRow_Deleted.[EF_RevisionNumber]) THEN IdRow_Existing.[EF_RevisionNumber] ELSE IdRow_Deleted.[EF_RevisionNumber] END AS [EF_RevisionNumber]
			,  IdRow_Existing.[EF_CompoundUnitGuid]
			,  CASE WHEN (IdRow_Deleted.[EF_RevisionNumber] IS NULL) THEN IdRow_Existing.[EF_IsDeleted] WHEN (IdRow_Existing.[EF_RevisionNumber] > IdRow_Deleted.[EF_RevisionNumber]) THEN IdRow_Existing.[EF_IsDeleted] ELSE IdRow_Deleted.[EF_IsDeleted] END AS [EF_IsDeleted]
		FROM
			(SELECT [EF_ProjectGuid], [EF_CompoundUnitGuid], [EF_IsDeleted], MAX([EF_RevisionNumber]) AS [EF_RevisionNumber]
				FROM [dbo].[CompoundUnits]
				WHERE ([EF_ProjectGuid] = @projectGuid)
					AND ([EF_RevisionNumber] <= @revisionNumber)
					AND ([EF_RevisionNumber] IN (SELECT [EF_RevisionNumber] FROM #IncludedRevisionNumbers))
					AND ((@limitsCompoundUnitGuids = 0)
						OR ((@limitsCompoundUnitGuids = 1) AND ([EF_CompoundUnitGuid] IN (SELECT [EF_CompoundUnitGuid] FROM #IncludedCompoundUnitGuids))))
					AND ([EF_IsDeleted] = 0)
				GROUP BY [EF_ProjectGuid], [EF_CompoundUnitGuid], [EF_IsDeleted]) AS IdRow_Existing
		LEFT OUTER JOIN
			(SELECT [EF_ProjectGuid], [EF_CompoundUnitGuid], [EF_IsDeleted], MAX([EF_RevisionNumber]) AS [EF_RevisionNumber]
				FROM [dbo].[CompoundUnits]
				WHERE ([EF_ProjectGuid] = @projectGuid)
					AND ([EF_RevisionNumber] <= @revisionNumber)
					AND ([EF_RevisionNumber] IN (SELECT [EF_RevisionNumber] FROM #IncludedRevisionNumbers))
					AND ((@limitsCompoundUnitGuids = 0)
						OR ((@limitsCompoundUnitGuids = 1) AND ([EF_CompoundUnitGuid] IN (SELECT [EF_CompoundUnitGuid] FROM #IncludedCompoundUnitGuids))))
					AND ([EF_IsDeleted] = 1)
				GROUP BY [EF_ProjectGuid], [EF_CompoundUnitGuid], [EF_IsDeleted]) AS IdRow_Deleted 
			ON (IdRow_Existing.[EF_ProjectGuid] = IdRow_Deleted.[EF_ProjectGuid]) AND (IdRow_Existing.[EF_CompoundUnitGuid] = IdRow_Deleted.[EF_CompoundUnitGuid]);

	INSERT INTO #IncludedIdRows_Current
		SELECT [EF_ProjectGuid], [EF_RevisionNumber], [EF_CompoundUnitGuid], [EF_IsDeleted]
		FROM #IncludedIdRows
		WHERE ([EF_IsDeleted] = 0);


	SELECT ObjRow.[EF_ProjectGuid]
		,  ObjRow.[EF_RevisionNumber]
		,  ObjRow.[EF_CompoundUnitGuid]
		,  ObjRow.[EF_ParentModelObjectType]
		,  ObjRow.[EF_ParentModelObjectRefs]
		,  ObjRow.[EF_CreatorGuid]
		,  ObjRow.[EF_CreationDate]
		,  ObjRow.[EF_OwnerType]
		,  ObjRow.[EF_OwnerGuid]
		,  ObjRow.[EF_IsDeleted]
		,  ObjRow.[EF_DeleterGuid]
		,  ObjRow.[EF_DeletionDate]
		FROM [dbo].[CompoundUnits] ObjRow
		INNER JOIN #IncludedIdRows_Current IdRow
			ON (ObjRow.[EF_ProjectGuid] = IdRow.[EF_ProjectGuid]) AND (ObjRow.[EF_RevisionNumber] = IdRow.[EF_RevisionNumber]) AND (ObjRow.[EF_CompoundUnitGuid] = IdRow.[EF_CompoundUnitGuid]);

	SELECT ObjRow.[EF_ProjectGuid]
		,  ObjRow.[EF_RevisionNumber]
		,  ObjRow.[EF_CompoundUnitGuid]
		,  ObjRow.[EF_BaseUnitTypeNumber]
		,  ObjRow.[EF_IsBaseUnitTypeScalar]
		,  ObjRow.[EF_NumeratorExponentiation]
		,  ObjRow.[EF_DenominatorExponention]
		FROM [dbo].[BaseUnitExponentiationValues] ObjRow
		INNER JOIN #IncludedIdRows_Current IdRow
			ON (ObjRow.[EF_ProjectGuid] = IdRow.[EF_ProjectGuid]) AND (ObjRow.[EF_RevisionNumber] = IdRow.[EF_RevisionNumber]) AND (ObjRow.[EF_CompoundUnitGuid] = IdRow.[EF_CompoundUnitGuid]);
END;