using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace RT.Comb.AspNetCore
{
	/// <summary>
	/// Extension methods for <see cref="IServiceCollection"/> 
	/// that allow adding a Comb Guid provider to the application's DI system.
	/// </summary>
	public static class PostgresSqlCombProviderServiceCollectionExtensions
	{
		/// <summary>
		/// Registers a <see cref="PostgreSqlCombProvider"/> in the <see cref="IServiceCollection"/> 
		/// using a <see cref="SqlDateTimeStrategy"/>
		/// </summary>
		/// <remarks>
		/// The <see cref="PostgreSqlCombProvider"/> is configured with the default <see cref="TimestampProvider"/> (DateTime.Utcnow)
		/// and <see cref="GuidProvider"/> (Guid.NewGuid()). These can be overridden by providing values to the arguments.
		/// </remarks>
		/// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
		/// <param name="customTimestampProvider">A delegate to a parameter-less function that returns a DateTime value </param>
		/// <param name="customGuidProvider"> A delegate to a parameter-less function that returns a Guid value</param>
		public static void AddPostgreSqlCombGuidWithSqlDateTime(
			this IServiceCollection services,
			TimestampProvider customTimestampProvider = null,
			GuidProvider customGuidProvider = null)
		{
			services.TryAddSingleton<ICombProvider>(
				AddPostgreSqlCombGuid(new SqlDateTimeStrategy(), customTimestampProvider, customGuidProvider));
		}

		/// <summary>
		/// Registers a <see cref="PostgreSqlCombProvider"/> in the <see cref="IServiceCollection"/> 
		/// using a <see cref="UnixDateTimeStrategy"/>
		/// </summary>
		/// <remarks>
		/// The <see cref="PostgreSqlCombProvider"/> is configured with the default <see cref="TimestampProvider"/> (DateTime.Utcnow)
		/// and <see cref="GuidProvider"/> (Guid.NewGuid()). These can be overridden by providing values to the arguments.
		/// </remarks>
		/// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
		/// <param name="customTimestampProvider">A delegate to a parameter-less function that returns a DateTime value </param>
		/// <param name="customGuidProvider"> A delegate to a parameter-less function that returns a Guid value</param>
		public static void AddPostgreSqlCombGuidWithUnixDateTime(
			this IServiceCollection services,
			TimestampProvider customTimestampProvider = null,
			GuidProvider customGuidProvider = null)
		{
			services.TryAddSingleton<ICombProvider>(
				AddPostgreSqlCombGuid(new UnixDateTimeStrategy(), customTimestampProvider, customGuidProvider));
		}

		private static PostgreSqlCombProvider AddPostgreSqlCombGuid(
			ICombDateTimeStrategy dateTimeStrategy,
			TimestampProvider customTimestampProvider,
			GuidProvider customGuidProvider)
		{
			return new PostgreSqlCombProvider(dateTimeStrategy, customTimestampProvider, customGuidProvider);
		}
	}
}
