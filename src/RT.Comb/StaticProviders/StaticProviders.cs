
namespace RT.Comb {

	public static class Provider {
		public static ICombProvider Legacy = new SqlCombProvider(new SqlDateTimeStrategy());
		public static ICombProvider Sql = new SqlCombProvider(new UnixDateTimeStrategy());
		public static ICombProvider PostgreSql = new PostgreSqlCombProvider(new UnixDateTimeStrategy());
	}

}