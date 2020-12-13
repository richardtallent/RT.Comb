using System.Reflection;

namespace RT.Comb.AspNetCore.Tests {

	public static class TestUtils {

		public static ICombDateTimeStrategy GetCurrentDateTimeStrategy(ICombProvider combProvider) {
			var dateTimeStragetyField = combProvider
				.GetType()
				.GetField("_dateTimeStrategy", BindingFlags.NonPublic | BindingFlags.Instance);

			return dateTimeStragetyField.GetValue(combProvider) as ICombDateTimeStrategy;
		}

	}

}
