using System;
using RT.CombUtils;

namespace RT.CombExtensions {

	/// <summary>
	/// This class includes COMB extension methods for System.Guid. This is in a separate
	/// namespace so you can decide whether or not to use them.
	/// </summary>
	public static class Extensions {

		/// <summary>
		/// Extracts the DateTime portion of an SQL Server variant COMB Guid.
		/// </summary>
		/// <param name="value">COMB GUID</param>
		/// <returns>DateTime contained within the input value</returns>
		public static DateTime FromCombSql(this Guid value)
			=> Comb.FromComb(value, CombVariant.SqlServer);

		/// <summary>
		/// Generates an SQL Server variant COMB GUID.
		/// </summary>
		/// <param name="value">GUID to use as the basis for the COMB</param>
		/// <param name="timestamp">DateTime to embed in the COMB</param>
		/// <returns>COMB value that combines the GUID and DateTime arguments</returns>
		public static Guid ToCombSql(this Guid value, DateTime timestamp)
			=> Comb.ToComb(value, timestamp, CombVariant.SqlServer);
		
		/// <summary>
		/// Extracts the DateTime portion of a byte-order variant COMB Guid.
		/// </summary>
		/// <param name="value">COMB GUID</param>
		/// <returns>DateTime contained within the input value</returns>
		public static DateTime FromCombByte(this Guid value)
			=> Comb.FromComb(value, CombVariant.ByteOrder);

		/// <summary>
		/// Generates an byte-order variant COMB GUID.
		/// </summary>
		/// <param name="value">GUID to use as the basis for the COMB</param>
		/// <param name="timestamp">DateTime to embed in the COMB</param>
		/// <returns>COMB value that combines the GUID and DateTime arguments</returns>
		public static Guid ToCombByte(this Guid value, DateTime timestamp)
			=> Comb.ToComb(value, timestamp, CombVariant.ByteOrder);

	}

}
