using System;
/*
	Copyright 2015-2025 Richard S. Tallent, II

	Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files
	(the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge,
	publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to
	do so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
	MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
	LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
	CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

namespace RT.Comb {

	/// <summary>
	/// Creates RFC 9562 UUIDv7 values: a 48-bit big-endian Unix-millisecond timestamp, version nibble 7,
	/// RFC variant bits (10), and random data elsewhere. Sorts chronologically on any platform that orders
	/// UUIDs by their string/network byte order (PostgreSQL, MySQL BINARY(16) via ToByteArray(bigEndian: true), etc.).
	/// </summary>
	public class Uuid7CombProvider(
		TimestampProvider? customTimestampProvider = null,
		GuidProvider? customGuidProvider = null
	) : BaseCombProvider(new UnixDateTimeStrategy(), customTimestampProvider, customGuidProvider) {

		private const long TicksPerMillisecond = TimeSpan.TicksPerMillisecond;
		private static readonly long _unixEpochTicks = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;

		// Same result as UnixDateTimeStrategy (48-bit big-endian Unix milliseconds), but built directly from the
		// Guid's fields. The fields display in string order, so no byte swapping is needed.
		public override Guid Create(Guid value, DateTime timestamp) {
			var ms = (timestamp.ToUniversalTime().Ticks - _unixEpochTicks) / TicksPerMillisecond;
			Span<byte> r = stackalloc byte[16];
			value.TryWriteBytes(r);
			// Keep the random 12 low bits of Data3 (bytes 6-7, little-endian) under the version nibble.
			var data3 = (short)((((r[7] << 8) | r[6]) & 0x0FFF) | 0x7000);
			return new Guid(
				(int)(ms >> 16), (short)ms, data3,
				(byte)((r[8] & 0x3F) | 0x80), r[9], r[10], r[11], r[12], r[13], r[14], r[15]);
		}

		public override DateTime GetTimestamp(Guid comb) {
			Span<byte> r = stackalloc byte[16];
			comb.TryWriteBytes(r);
			// Data1 (bytes 0-3) and Data2 (bytes 4-5) are little-endian and hold the high 32 and low 16 bits.
			var ms = ((long)(uint)(r[0] | (r[1] << 8) | (r[2] << 16) | (r[3] << 24)) << 16) | (long)(r[4] | (r[5] << 8));
			return new DateTime(_unixEpochTicks + ms * TicksPerMillisecond, DateTimeKind.Utc);
		}

	}

}