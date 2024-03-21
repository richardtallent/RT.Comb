SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION dbo.DateFromComb(@comb UNIQUEIDENTIFIER) RETURNS DATETIME2 AS BEGIN

	-- Returns the timestamp for a *Unix-style* (default/recommended) COMB value.

	-- Example:
	-- SELECT dbo.DateFromComb('d01c25cc-63a7-47cc-b5ba-018e5f3bff48');
	-- -- Returns `2024-03-21 04:19:11.3040000`

	DECLARE @msSinceEpoch BIGINT = CAST(
			CAST(0 AS BINARY(2))
			+ SUBSTRING(CAST(@comb AS BINARY(16)), 11, 6)
		AS BIGINT);

	DECLARE @msPerDay		BIGINT = 24 * 60 * 60 * 1000;
	DECLARE @daysSinceEpoch	BIGINT = @msSinceEpoch / @msPerDay;
	DECLARE @leftoverMs		INT = @msSinceEpoch - @daysSinceEpoch * @msPerDay;

	RETURN	DATEADD(
				MILLISECOND,
				@leftoverMs,
				DATEADD(
					DAY,
					@daysSinceEpoch,
					CAST('1970-01-01' AS DATETIME2)
				)
			);

END