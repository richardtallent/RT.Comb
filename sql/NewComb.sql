SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION dbo.NewComb() RETURNS UNIQUEIDENTIFIER AS BEGIN

    -- Generates a UNIX-style COMB as per https://github.com/richardtallent/RT.Comb documentation.
	-- Requires MSSQL 2016 or above. For older versions of MSSQL, replace `SYSUTCDATETIME()` with
	-- `GETUTCDATE()`, but note that time resolution will be limited to ~1/300s.

    RETURN	CAST(
				(SELECT CAST(id AS BINARY(10)) FROM vwNewGuid)
				+ CAST(
					DATEDIFF_BIG(MILLISECOND, '1970-1-1', SYSUTCDATETIME())
					AS BINARY(6)
				)
				AS UNIQUEIDENTIFIER
			);
END
GO
