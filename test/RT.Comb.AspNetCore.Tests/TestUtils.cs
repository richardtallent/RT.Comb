using System.Reflection;

namespace RT.Comb.AspNetCore.Tests {

	public static class TestUtils {

		public static ICombDateTimeStrategy GetCurrentDateTimeStrategy(ICombProvider combProvider) =>
		 	combProvider
				.GetType()
				.GetField("DateTimeStrategy", BindingFlags.NonPublic | BindingFlags.Instance)
				.GetValue(combProvider) as ICombDateTimeStrategy;

	}

}
