using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace RT.Comb.AspNetCore {

	/// <summary>
	/// Extension methods for <see cref="IServiceCollection"/> 
	/// that allow adding a Comb Guid provider to the application's DI system.
	/// </summary>
	public static class SqlCombProviderServiceCollectionExtensions {

		/// <summary>
		/// Registers a <see cref="SqlCombProvider"/> in the <see cref="IServiceCollection"/> 
		/// using a <see cref="SqlDateTimeStrategy"/>
		/// </summary>
		/// <remarks>
		/// The <see cref="SqlCombProvider"/> is configured with the default <see cref="TimestampProvider"/> (DateTime.Utcnow)
		/// and <see cref="GuidProvider"/> (Guid.NewGuid()). These can be overridden by providing values to the arguments.
		/// </remarks>
		/// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
		/// <param name="customTimestampProvider">A delegate to a parameter-less function that returns a DateTime value </param>
		/// <param name="customGuidProvider"> A delegate to a parameter-less function that returns a Guid value</param>
		public static void AddSqlCombGuidWithSqlDateTime(
			this IServiceCollection services,
			TimestampProvider? customTimestampProvider = null,
			GuidProvider? customGuidProvider = null
		) => services.TryAddSingleton<ICombProvider>(
				AddSqlCombGuid(new SqlDateTimeStrategy(), customTimestampProvider, customGuidProvider));

		/// <summary>
		/// Registers a <see cref="SqlCombProvider"/> in the <see cref="IServiceCollection"/> 
		/// using a <see cref="UnixDateTimeStrategy"/>
		/// </summary>
		/// <remarks>
		/// The <see cref="SqlCombProvider"/> is configured with the default <see cref="TimestampProvider"/> (DateTime.Utcnow)
		/// and <see cref="GuidProvider"/> (Guid.NewGuid()). These can be overridden by providing values to the arguments.
		/// </remarks>
		/// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
		/// <param name="customTimestampProvider">A delegate to a parameter-less function that returns a DateTime value </param>
		/// <param name="customGuidProvider"> A delegate to a parameter-less function that returns a Guid value</param>
		public static void AddSqlCombGuidWithUnixDateTime(
			this IServiceCollection services,
			TimestampProvider? customTimestampProvider = null,
			GuidProvider? customGuidProvider = null
		) => services.TryAddSingleton<ICombProvider>(
				AddSqlCombGuid(new UnixDateTimeStrategy(), customTimestampProvider, customGuidProvider));

		private static SqlCombProvider AddSqlCombGuid(
			ICombDateTimeStrategy dateTimeStrategy,
			TimestampProvider? customTimestampProvider,
			GuidProvider? customGuidProvider
		) => new(dateTimeStrategy, customTimestampProvider, customGuidProvider);

	}

}
