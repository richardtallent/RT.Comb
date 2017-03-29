using System;
/*
	Copyright 2015-2017 Richard S. Tallent, II

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

		public int NumDateBytes { get; } = 6;

		public DateTime MinDateTimeValue { get; } = new DateTime(1900, 1, 1);

		public DateTime MaxDateTimeValue { get { return MinDateTimeValue.AddDays(ushort.MaxValue); } }

		public byte[] DateTimeToBytes(DateTime timestamp) {
			// Convert the time to 300ths of a second. SQL Server uses float math for this before converting to an integer, so this does as well
			// to avoid rounding errors. This is confirmed in MSSQL by SELECT CONVERT(varchar, CAST(CAST(2 as binary(8)) AS datetime), 121),
			// which would return .006 if it were integer math, but it returns .007.
			var ticks = (int)(timestamp.TimeOfDay.TotalMilliseconds * TicksPerMillisecond);
			var days = (ushort)(timestamp - MinDateTimeValue).TotalDays;
			var tickBytes = BitConverter.GetBytes(ticks);
			var dayBytes = BitConverter.GetBytes(days);

			if(BitConverter.IsLittleEndian) {
				// x86 platforms store the LEAST significant bytes first, we want the opposite for our arrays
				Array.Reverse(dayBytes);
				Array.Reverse(tickBytes);
			}

			var result = new byte[NumDateBytes];
			Array.Copy(dayBytes, 0, result, 0, 2);
			Array.Copy(tickBytes, 0, result, 2, 4);
			return result;
		}

		public DateTime BytesToDateTime(byte[] value) {
			// Attempt to convert the first 6 bytes.
			var dayBytes = new byte[2];                     // Empty ushort
			var tickBytes = new byte[4];                    // Empty int
			Array.Copy(value, 0, dayBytes, 0, 2);
			Array.Copy(value, 2, tickBytes, 0, 4);

			if(BitConverter.IsLittleEndian) {
				// COMBs store the MOST significant bytes first, we need the opposite to convert back to x86-form int/ushort
				Array.Reverse(dayBytes);
				Array.Reverse(tickBytes);
			}

			var days = BitConverter.ToUInt16(dayBytes, 0);
			var ticks = BitConverter.ToInt32(tickBytes, 0);

			if(ticks < 0f) {
				throw new ArgumentException("Not a COMB, time component is negative.");
			} else if(ticks > TicksPerDay) {
				throw new ArgumentException("Not a COMB, time component exceeds 24 hours.");
			}

			return MinDateTimeValue.AddDays(days).AddMilliseconds((double)ticks / TicksPerMillisecond);
		}

    }

}