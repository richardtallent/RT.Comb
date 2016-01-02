using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RT.CombUtils;

namespace RT.CombTests {

	[TestClass]
	public class MainTests {

		/// <summary>
		/// Ensure that the date provided to be injected into a GUID is the same date we get back from it.
		/// COMB has a resolution of 1/300s (3.333...ms), so the original and decoded datetime should 
		/// never be off by more than 4ms.
		/// </summary>
		[TestMethod]
		public void TestIfSqlOrderReversible() {
			var dt = DateTime.UtcNow;
			var comb = Comb.ToComb(Guid.NewGuid(), dt, CombVariant.SqlServer);
			var dtDecoded = Comb.FromComb(comb, CombVariant.SqlServer);
			var delta = Math.Abs((dtDecoded - dt).TotalMilliseconds);
			Assert.IsTrue(delta <= 4);
		}

		/// <summary>
		/// Ensure that the date provided to be injected into a GUID is the same date we get back from it.
		/// COMB has a resolution of 1/300s (3.333...ms), so the original and decoded datetime should 
		/// never be off by more than 4ms.
		/// </summary>
		[TestMethod]
		public void TestIfByteOrderReversible() {
			var dt = DateTime.UtcNow;
			var comb = Comb.ToComb(Guid.NewGuid(), dt, CombVariant.ByteOrder);
			var dtDecoded = Comb.FromComb(comb, CombVariant.ByteOrder);
			var delta = Math.Abs((dtDecoded - dt).TotalMilliseconds);
			Assert.IsTrue(delta <= 4);
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
		[TestMethod]
		public void TestIfSqlOrderSortsInMSSQL() {
			var g1 = MaxGuid();
			var g2 = Guid.Empty;
			// Inject the dates
			System.Data.SqlTypes.SqlGuid comb1 = Comb.ToComb(g1, new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc), CombVariant.SqlServer);
			System.Data.SqlTypes.SqlGuid comb2 = Comb.ToComb(g2, new DateTime(2000, 1, 1, 0, 0, 1, DateTimeKind.Utc), CombVariant.SqlServer);
			// Now they sort the opposite way as they would have without the dates
			Assert.IsTrue((bool)(comb1 < comb2));
		}

		/// <summary>
		/// Ensure that PostgreSql will sort our COMB values in date/time order, without regard for
		/// the other (random) bytes. To test this, we cast out Guid values as a byte array and
		/// compare the bytes in turn, which simulates how UUID values are sorted in PostgreSql.
		/// Note that comb1 has all bits set to 1 other than the ones from our DateTime value, and
		/// comb2 has all bits set to 0 other than our other, slightly more recent DateTime value.
		/// Thus, if the result shows that comb1 is less than comb2, the DateTime values are being 
		/// sorted before any of the other bits (this is the expected outcome).
		/// </summary>
		[TestMethod]
		public void TestIfByteOrderSortsAsBytes() {
			var g1 = MaxGuid();
			var g2 = Guid.Empty;
			// Inject the dates
			var comb1 = Comb.ToComb(g1, new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc), CombVariant.ByteOrder).ToByteArray();
			var comb2 = Comb.ToComb(g2, new DateTime(2000, 1, 1, 0, 0, 1, DateTimeKind.Utc), CombVariant.ByteOrder).ToByteArray();
			// Now they sort the opposite way as they would have without the dates
			int result = 0;
			for(var i = 0; i < 16; i++) {
				result = comb1[i].CompareTo(comb2[i]);
				if(result != 0) break;
			}
			Assert.AreEqual(-1, result);
		}

	}

}