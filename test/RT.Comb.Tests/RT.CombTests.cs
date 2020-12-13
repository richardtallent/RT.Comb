using System;
using Xunit;
using RT.Comb;

namespace RT.CombTests {

	public class MainTests {

		private static UtcNoRepeatTimestampProvider NoDupeProvider = new UtcNoRepeatTimestampProvider();

		private ICombProvider SqlNoRepeatCombs = new SqlCombProvider(new SqlDateTimeStrategy(), customTimestampProvider: NoDupeProvider.GetTimestamp);
		private UnixDateTimeStrategy UnixStrategy = new UnixDateTimeStrategy();

		/// <summary>
		/// Ensure that the date provided to be injected into a GUID is the same date we get back from it.
		/// COMB has a resolution of 1/300s (3.333...ms), so the original and decoded datetime should 
		/// never be off by more than 4ms.
		/// </summary>
		[Fact]
		public void TestIfSqlStyleReversible() => IsReversible(Provider.Legacy, 4);

		[Fact]
		public void TestIfPostgreStyleReversible() => IsReversible(Provider.PostgreSql, 0);

		[Fact]
		public void TestIfHybridStyleReversible() => IsReversible(Provider.Sql, 0);

		[Fact]
		public void TestDateTimeToMs() {
			var d = UnixStrategy.MinDateTimeValue.AddMilliseconds(1);
			var ms = UnixStrategy.ToUnixTimeMilliseconds(d);
			Assert.Equal<long>(1, ms);
		}

		[Fact]
		public void TestFromUnitMs() =>
			Assert.Equal(UnixStrategy.MinDateTimeValue.AddMilliseconds(1), UnixStrategy.FromUnixTimeMilliseconds(1));

		/// <summary>
		/// Ensure that the date provided to be injected into a GUID is the same date we get back from it.
		/// Allowed for an optional resolution loss in the time. Calculates the delta using ticks to
		/// avoid floating point drift in using TotalMilliseconds.
		/// </summary>
		private void IsReversible(ICombProvider Comb, int ClockDriftAllowedMs) {
			var dt = DateTime.UtcNow;
			var comb = Comb.Create(dt);
			var dtDecoded = Comb.GetTimestamp(comb);
			var delta = (dtDecoded.Ticks - dt.Ticks) / 10000;
			Assert.InRange(delta, -ClockDriftAllowedMs, ClockDriftAllowedMs);
		}

		/// <summary>
		/// Utility test function to return a GUID where the majority of the bits are set to 1.
		/// </summary>
		private Guid MaxGuid() =>
			new Guid(int.MaxValue, short.MaxValue, short.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

		/// <summary>
		/// Ensure that SQL Server will sort our COMB values in date/time order, without regard for
		/// the other bytes. To test this, we cast out Guid values as SqlGuid, which has a comparison
		/// logic as MSSQL. Note that comb1 is maxed out other than in the timestamp bits, and comb2
		/// is zeroed out other than the timestamp. Thus, if the result shows that comb1 is less than
		/// comb2, the DateTime values are being sorted before any of the other bits (as expected).
		/// </summary>
		[Fact]
		public void TestIfSqlStyleSortsProperly() {
			var g1 = MaxGuid();
			var g2 = Guid.Empty;
			// Inject the dates
			var d = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			System.Data.SqlTypes.SqlGuid comb1 = Provider.Legacy.Create(g1, d);
			System.Data.SqlTypes.SqlGuid comb2 = Provider.Legacy.Create(g2, d.AddMilliseconds(10));
			Assert.Equal(-1, comb1.CompareTo(comb2));
		}

		/// <summary>
		/// Ensure that SQL Server will sort our COMB values in date/time order, without regard for
		/// the other bytes. To test this, we cast out Guid values as SqlGuid, which has a comparison
		/// logic as MSSQL. Note that comb1 is maxed out other than in the timestamp bits, and comb2
		/// is zeroed out other than the timestamp. Thus, if the result shows that comb1 is less than
		/// comb2, the DateTime values are being sorted before any of the other bits (as expected).
		/// </summary>
		[Fact]
		public void TestIfHybridStyleSortsProperly() {
			var g1 = MaxGuid();
			var g2 = Guid.Empty;
			// Inject the dates
			var d = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			System.Data.SqlTypes.SqlGuid comb1 = Provider.Sql.Create(g1, d);
			System.Data.SqlTypes.SqlGuid comb2 = Provider.Sql.Create(g2, d.AddMilliseconds(1));
			Assert.Equal(-1, comb1.CompareTo(comb2));
		}

		/// <summary>
		/// Ensure that PostgreSQL will sort our COMB values in date/time order, without regard for
		/// the other bytes. To test this, we create two COMBs, 1ms apart, but with the lower time
		/// version being maxed out in all other bits, and the higher time version being zeroed on 
		/// all non-timestamp bits. Thus, if the result shows that comb1 is less than comb2, the 
		/// DateTime values are being sorted before any of the other bits (as expected).
		/// Since we don't have a PostgreSQL server handy during the actual test, we rely on the fact that
		/// both PostgreSQL and .NET Guids sort based on the string order, not the .NET byte order.
		/// </summary>
		[Fact]
		public void TestIfPostgreStyleSortsProperly() {
			var g1 = MaxGuid();
			var g2 = Guid.Empty;
			// Inject the dates
			var d = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			var comb1 = Provider.PostgreSql.Create(g1, d);
			var comb2 = Provider.PostgreSql.Create(g2, d.AddMilliseconds(1));
			Assert.Equal(-1, comb1.CompareTo(comb2));
		}

		/// <summary>
		/// Ensure that the UtcNoRepeatTimestampProvider will not generate COMBs with the same or
		/// out of order timestamps. Uses SqlDateTimeStrategy because it has the lowest resolution
		/// and thus the highest potential for a collision.
		/// </summary>
		[Fact]
		public void TestUtcNoRepeatTimestampProvider() {
			var g1 = SqlNoRepeatCombs.Create();
			var dt1 = SqlNoRepeatCombs.GetTimestamp(g1);
			var inSequenceCount = 0;
			for (var i = 0; i < 1000; i++) {
				var g2 = SqlNoRepeatCombs.Create();
				var dt2 = SqlNoRepeatCombs.GetTimestamp(g2);
				if (dt2 > dt1) {
					inSequenceCount++;
				} else {
					Console.WriteLine($"{dt1:MM/dd/yyyy hh:mm:ss.fff} > {dt2:MM/dd/yyyy hh:mm:ss.fff}");
				}
				dt1 = dt2;
			}
			Assert.Equal(1000, inSequenceCount);
		}

	}

}