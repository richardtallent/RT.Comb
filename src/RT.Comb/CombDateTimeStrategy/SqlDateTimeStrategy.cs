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
	/// Represents a mechanism for converting DateTime values back and forth to byte arrays.
	/// Strategies can differ depending on how many bytes of the GUID you wish to overwrite,
	/// what time resolution you want, etc.
	/// </summary>
	public class SqlDateTimeStrategy : ICombDateTimeStrategy {

		private const double TicksPerDay = 86400d * 300d;

		private const double TicksPerMillisecond = 3d / 10d;

		public int NumDateBytes => 6;

		public DateTime MinDateTimeValue { get; } = new DateTime(1900, 1, 1);

		public DateTime MaxDateTimeValue => MinDateTimeValue.AddDays(ushort.MaxValue);

		public void WriteDateTime(Span<byte> destination, DateTime timestamp) {
			// Convert the time to 300ths of a second. SQL Server uses float math for this before converting to an integer, so this does as well
			// to avoid rounding errors. This is confirmed in MSSQL by SELECT CONVERT(varchar, CAST(CAST(2 as binary(8)) AS datetime), 121),
			// which would return .006 if it were integer math, but it returns .007.
			var ticks = (int)(timestamp.TimeOfDay.TotalMilliseconds * TicksPerMillisecond);
			var days = (ushort)(timestamp - MinDateTimeValue).TotalDays;
			BinaryPrimitives.WriteInt32BigEndian(destination[2..], ticks);
			BinaryPrimitives.WriteUInt16BigEndian(destination, days);
		}

		public DateTime ReadDateTime(ReadOnlySpan<byte> source) {
			// Attempt to convert the first 6 bytes.
			var days = BinaryPrimitives.ReadUInt16BigEndian(source[..2]);
			double ticks = BinaryPrimitives.ReadInt32BigEndian(source.Slice(2, 4));

			if (ticks < 0) {
				throw new ArgumentException("Not a COMB, time component is negative.");
			} else if (ticks > TicksPerDay) {
				throw new ArgumentException("Not a COMB, time component exceeds 24 hours.");
			}

			return MinDateTimeValue.AddDays(days).AddMilliseconds(ticks / TicksPerMillisecond);
		}

	}

}