using System;
using Xunit;
using RT.Comb;

namespace RT.CombTests {

	public class Uuid7Tests {

		private static readonly DateTime _d = new(2025, 3, 29, 12, 34, 56, 789, DateTimeKind.Utc);

		[Fact]
		public void SetsVersion7AndRfcVariant() {
			for (var i = 0; i < 1000; i++) {
				var bytes = Provider.Uuid7.Create().ToByteArray(bigEndian: true);
				Assert.Equal(0x70, bytes[6] & 0xF0);
				Assert.Equal(0x80, bytes[8] & 0xC0);
			}
			Assert.Equal('7', Provider.Uuid7.Create().ToString()[14]);
		}

		[Fact]
		public void VersionAndVariantSurviveAllZeroAndAllOneRandomBits() {
			foreach (var g in new[] { Guid.Empty, new Guid(new byte[] { 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255 }) }) {
				var s = Provider.Uuid7.Create(g, _d).ToString();
				Assert.Equal('7', s[14]);
				Assert.Contains(s[19], "89ab");
			}
		}

		[Fact]
		public void TimestampIsFirst48BitsInStringOrder() {
			var comb = Provider.Uuid7.Create(_d);
			var expectedMs = new DateTimeOffset(_d).ToUnixTimeMilliseconds();
			Assert.Equal(expectedMs.ToString("x12"), comb.ToString("N")[..12]);
			Assert.Equal(_d, Provider.Uuid7.GetTimestamp(comb));
		}

		[Fact]
		public void ReadsTimestampFromFrameworkUuid7() {
			var ts = new DateTimeOffset(_d);
			var g = Guid.CreateVersion7(ts);
			Assert.Equal(_d, Provider.Uuid7.GetTimestamp(g));
		}

		[Fact]
		public void MatchesFrameworkUuid7Structure() {
			var framework = Guid.CreateVersion7(new DateTimeOffset(_d)).ToByteArray(bigEndian: true);
			var ours = Provider.Uuid7.Create(_d).ToByteArray(bigEndian: true);
			Assert.Equal(framework[..6], ours[..6]);
			Assert.Equal(framework[6] & 0xF0, ours[6] & 0xF0);
			Assert.Equal(framework[8] & 0xC0, ours[8] & 0xC0);
		}

		[Fact]
		public void SortsByBigEndianBytes() {
			var a = Provider.Uuid7.Create(Guid.NewGuid(), _d).ToByteArray(bigEndian: true);
			var b = Provider.Uuid7.Create(Guid.Empty, _d.AddMilliseconds(1)).ToByteArray(bigEndian: true);
			Assert.True(a.AsSpan().SequenceCompareTo(b) < 0);
			Assert.True(string.CompareOrdinal(new Guid(a, bigEndian: true).ToString(), new Guid(b, bigEndian: true).ToString()) < 0);
		}

		[Fact]
		public void UnixStrategyMaxDateIsUsable() {
			var s = new UnixDateTimeStrategy();
			Assert.True(s.MaxDateTimeValue > s.MinDateTimeValue);
			Assert.Equal(DateTime.MaxValue, s.MaxDateTimeValue);
		}

	}

}
