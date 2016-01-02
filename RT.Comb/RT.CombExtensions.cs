using System;
using RT.CombUtils;

namespace RT.CombExtensions {

	public static class Extensions {

		public static DateTime FromCombSql(this Guid value)
			=> Comb.FromComb(value, CombVariant.SqlServer);

		public static Guid ToCombSql(this Guid value, DateTime timestamp)
			=> Comb.ToComb(value, timestamp, CombVariant.SqlServer);

		public static DateTime FromCombByte(this Guid value)
			=> Comb.FromComb(value, CombVariant.ByteOrder);

		public static Guid ToCombByte(this Guid value, DateTime timestamp)
			=> Comb.ToComb(value, timestamp, CombVariant.ByteOrder);

	}

}
