using System;
using Xunit;
using RT.Comb;

namespace RT.CombTests {

	public class MainTests {

		private ICombProvider SqlCombs = new CombProvider(new CombProviderOptions(new SqlDateTimeStrategy(), Constants.SqlServerGuidOffset));
		private ICombProvider PostgreCombs = new CombProvider(new CombProviderOptions(new UnixDateTimeStrategy(), Constants.PostgreSqlGuidOffset));
		private ICombProvider HybridCombs = new CombProvider(new CombProviderOptions(new UnixDateTimeStrategy(), Constants.SqlServerGuidOffset));
		private UnixDateTimeStrategy UnixStrategy = new UnixDateTimeStrategy();

		/// <summary>
		/// Ensure that the date provided to be injected into a GUID is the same date we get back from it.
		/// COMB has a resolution of 1/300s (3.333...ms), so the original and decoded datetime should 
		/// never be off by more than 4ms.
		/// </summary>
		[Fact]
		public void TestIfSqlStyleReversible() => IsReversible(SqlCombs, 4);

		[Fact]
		public void TestIfPostgreStyleReversible() => IsReversible(PostgreCombs, 0);

		[Fact]
		public void TestIfHybridStyleReversible() => IsReversible(HybridCombs, 0);

		[Fact]
		public void TestBinaryLong() {
			long i = 1;
			string bin = Convert.ToString(i, 2).PadLeft(64, '0');
			Assert.Equal<string>(bin, (new String('0',63)) + "1");
		}

		[Fact]
		public void TestHexLong() {
			long i = 1;
			string bin = Convert.ToString(i, 16).PadLeft(16, '0');
			Assert.Equal<string>(bin, (new String('0',15)) + "1");
		}

		[Fact]
		public void TestDateTimeToMs() {
			var d = UnixStrategy.MinDateTimeValue.AddMilliseconds(1);
			var ms = UnixStrategy.ToUnixTimeMilliseconds(d); 
			Assert.Equal<long>(1, ms);
		}

		[Fact] void TestFromUnitMs() {
			Assert.Equal(UnixStrategy.MinDateTimeValue.AddMilliseconds(1), UnixStrategy.FromUnixTimeMilliseconds(1));
		}

		/// <summary>
		/// Ensure that the date provided to be injected into a GUID is the same date we get back from it.
		/// Allowed for an optional resolution loss in the time. Calculates the delta using ticks to
		/// avoid floating point drift in using TotalMilliseconds.
		/// </summary>
		private void IsReversible(ICombProvider Comb, int ClockDriftAllowedMs) {
			var dt = DateTime.UtcNow;
			var comb = Comb.Create(dt);
			var dtDecoded = Comb.GetTimestamp(comb);
			var delta = (dtDecoded.Ticks - dt.Ticks) / RT.Comb.Constants.TicksPerMillisecond;
			Assert.InRange(delta, -ClockDriftAllowedMs, ClockDriftAllowedMs);
		}

		/// <summary>
		/// Utility test function to return a GUID where the majority of the bits are set to 1.
		/// </summary>
		private Guid MaxGuid() {
			return new Guid(int.MaxValue, short.MaxValue, short.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		}

		/// <summary>
		/// Ensure that SQL Server will sort our COMB values in date/time order, without regard for
		/// the other (random) bytes. To test this, we cast out Guid values as SqlGuid, which has a
		/// comparison method that sorts the same way as MSSQL. Note that comb1 has all bits set to
		/// 1 other than the ones from our DateTime value, and comb2 has all bits set to 0 other than
		/// our other, slightly more recent DateTime value. Thus, if the result shows that comb1 is
		/// less than comb2, the DateTime values are being sorted before any of the other bits (this
		/// is the expected outcome).
		/// </summary>
		/*
		[Fact]
		public void TestIfSqlStyleSortsProperly() {
			var g1 = MaxGuid();
			var g2 = Guid.Empty;
			// Inject the dates
			System.Data.SqlTypes.SqlGuid comb1 = SqlCombs.Create(g1, new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc));
			System.Data.SqlTypes.SqlGuid comb2 = SqlCombs.Create(g2, new DateTime(2000, 1, 1, 0, 0, 1, DateTimeKind.Utc));
			// Now they sort the opposite way as they would have without the dates
			Assert.True((bool)(comb1 < comb2));
		}

		/// <summary>
		/// Ensure that SQL Server will sort our COMB values in date/time order, without regard for
		/// the other (random) bytes. To test this, we cast out Guid values as SqlGuid, which has a
		/// comparison method that sorts the same way as MSSQL. Note that comb1 has all bits set to
		/// 1 other than the ones from our DateTime value, and comb2 has all bits set to 0 other than
		/// our other, slightly more recent DateTime value. Thus, if the result shows that comb1 is
		/// less than comb2, the DateTime values are being sorted before any of the other bits (this
		/// is the expected outcome).
		/// </summary>
		[Fact]
		public void TestIfHybridStyleSortsProperly() {
			var g1 = MaxGuid();
			var g2 = Guid.Empty;
			// Inject the dates
			System.Data.SqlTypes.SqlGuid comb1 = HybridCombs.Create(g1, new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc));
			System.Data.SqlTypes.SqlGuid comb2 = HybridCombs.Create(g2, new DateTime(2000, 1, 1, 0, 0, 1, DateTimeKind.Utc));
			// Now they sort the opposite way as they would have without the dates
			Assert.True((bool)(comb1 < comb2));
		}

		/// <summary>
		/// Ensure that PostgreSQL will sort our COMB values in date/time order, without regard for
		/// the other (random) bytes. To test this, we cast out Guid values as a byte array and
		/// compare the bytes in turn, which simulates how UUID values are sorted in PostgreSQL.
		/// Note that comb1 has all bits set to 1 other than the ones from our DateTime value, and
		/// comb2 has all bits set to 0 other than our other, slightly more recent DateTime value.
		/// Thus, if the result shows that comb1 is less than comb2, the DateTime values are being 
		/// sorted before any of the other bits (this is the expected outcome).
		/// </summary>
		[Fact]
		public void TestIfPostgreStyleSortsProperly() {
			var g1 = MaxGuid();
			var g2 = Guid.Empty;
			// Inject the dates
			var comb1 = PostgreCombs.Create(g1, new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)).ToByteArray();
			var comb2 = PostgreCombs.Create(g2, new DateTime(2000, 1, 1, 0, 0, 1, DateTimeKind.Utc)).ToByteArray();
			// Now they sort the opposite way as they would have without the dates
			int result = 0;
			for(var i = 0; i < 16; i++) {
				result = comb1[i].CompareTo(comb2[i]);
				if(result != 0) break;
			}
			Assert.Equal(-1, result);
		}*/

	}

}