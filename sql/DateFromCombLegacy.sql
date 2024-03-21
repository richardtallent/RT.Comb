SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION dbo.DateFromCombLegacy(@comb UNIQUEIDENTIFIER) RETURNS DATETIME AS BEGIN

	-- Returns the timestamp for a *legacy* (original-style) COMB value. This style is
	-- not recommended for new development and is only included for compatibility.

	-- Example:
	-- SELECT dbo.DateFromCombLegacy('E25AFE33-DB2D-4502-9BF0-919001862CC4')
	-- -- Returns `2002-01-10 23:40:35.000`

	RETURN	CAST(
				CAST(0 AS BINARY(2))
				+ SUBSTRING(
					CAST(@comb AS BINARY(16)),
					11,
					6
				)
    			AS DATETIME
			)

END