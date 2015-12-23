using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RT.CombExtensions {

	public static class Extensions {

		public static DateTime FromCombSql(this Guid value)
			=> RT.Comb.FromComb(value, CombVariant.SqlServer);

		public static Guid ToCombSql(this Guid value, DateTime timestamp)
			=> RT.Comb.ToComb(value, timestamp, CombVariant.SqlServer);

		public static DateTime FromCombByte(this Guid value)
			=> RT.Comb.FromComb(value, CombVariant.ByteOrder);

		public static Guid ToCombByte(this Guid value, DateTime timestamp)
			=> RT.Comb.ToComb(value, timestamp, CombVariant.ByteOrder);

	}

}
