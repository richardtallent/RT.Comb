SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION dbo.NewCombLegacy() RETURNS UNIQUEIDENTIFIER AS BEGIN
    -- Generate a legacy (original) COMB as per https://github.com/richardtallent/RT.Comb documentation.
	-- Due to how `DATETIME` stores date and time, the resolution of this version is far lower than
	-- the default Unix style, and is only recommended for compatibility.
    RETURN	CAST(
        		(SELECT CAST(id AS BINARY(10)) FROM vwNewGuid)
					+ CAST(GETUTCDATE() AS BINARY(6))
        		AS UNIQUEIDENTIFIER
    		);
END