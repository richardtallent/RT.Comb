using System;
using System.Buffers.Binary;
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
	/// This strategy stores the Unix epoch timestamp, scaled to milliseconds, in 48 unsigned bits.
	/// The allowed date range is 1970-01-01 to 8419-05-26.
	/// </summary>
	public class UnixDateTimeStrategy : ICombDateTimeStrategy {

		private const int FixedNumDateBytes = 6;
		private const int RemainingBytesFromInt64 = 8 - FixedNumDateBytes;

		public int NumDateBytes => FixedNumDateBytes;

		public DateTime MinDateTimeValue { get; } = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

		public DateTime MaxDateTimeValue => MinDateTimeValue.AddMilliseconds(2 ^ (8 * NumDateBytes));

		public void WriteDateTime(Span<byte> destination, DateTime timestamp) {
			var ms = ToUnixTimeMilliseconds(timestamp);
			Span<byte> msBytes = stackalloc byte[8];
			BinaryPrimitives.WriteInt64BigEndian(msBytes, ms);
			msBytes[RemainingBytesFromInt64..].CopyTo(destination);
		}

		public DateTime ReadDateTime(ReadOnlySpan<byte> source) {
			// Attempt to convert the first 6 bytes.
			Span<byte> msBytes = stackalloc byte[8];
			source[..FixedNumDateBytes].CopyTo(msBytes[RemainingBytesFromInt64..]);
			msBytes[..RemainingBytesFromInt64].Clear();
			var ms = BinaryPrimitives.ReadInt64BigEndian(msBytes);
			return FromUnixTimeMilliseconds(ms);
		}

		public long ToUnixTimeMilliseconds(DateTime timestamp) => (long)(timestamp.ToUniversalTime() - MinDateTimeValue).TotalMilliseconds;

		public DateTime FromUnixTimeMilliseconds(long ms) => MinDateTimeValue.AddMilliseconds(ms);

	}

}