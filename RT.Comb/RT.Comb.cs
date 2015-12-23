using System;
/*
	Copyright 2015 Richard S. Tallent, II

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

namespace RT {

	/// <summary>
	/// Utilities for decoding and encoding the date in a Comb Guid.
	/// </summary>
	public static class Comb {

		// Various constants
		private static readonly int StartIndexSqlServer = 10;
		private static readonly int CombDateSize = 6;
		private static readonly DateTime MinCombDate = new DateTime(1900, 1, 1);
		private static readonly DateTime MaxCombDate = MinCombDate.AddDays(ushort.MaxValue);
		private static readonly double TicksPerDay = 86400d * 300d;
		private static readonly double TicksPerMillisecond = 3d / 10d;

		/// <summary>
		/// Return a new GUID COMB of the specified variant.
		/// </summary>
		public static Guid New(CombVariant variant) {
			return ToComb(Guid.NewGuid(), DateTime.UtcNow, variant);
		}

		/// <summary>
		/// Retrieve the DateTime value previously stored in a COMB Guid value.
		/// </summary>
		public static DateTime FromComb(Guid comb, CombVariant variant) {
			var bytes = comb.ToByteArray();
			var dtbytes = new byte[CombDateSize];

			if(variant == CombVariant.SqlServer) {
				Array.Copy(bytes, StartIndexSqlServer, dtbytes, 0, CombDateSize);
			} else {
				Array.Copy(bytes, 0, dtbytes, 0, CombDateSize);
				// Move nybbles 5-8 to 6-9, overwriting the existing 9 (version "4")
				dtbytes[4] = (byte)((byte)(dtbytes[3] << 4) | (byte)(dtbytes[4] & 0x0F));
				dtbytes[3] = (byte)((byte)(dtbytes[2] << 4) | (byte)(dtbytes[3] >> 4));
				dtbytes[2] = (byte)(dtbytes[2] >> 4);
			}

			return BytesToDateTime(dtbytes);
		}

		/// <summary>
		/// Encode a given DateTime value in the given Guid value.
		/// </summary>
		public static Guid ToComb(Guid value, DateTime datetime, CombVariant variant) {
			var bytes = value.ToByteArray();
			var dtbytes = DateTimeToBytes(datetime);

			if(variant == CombVariant.SqlServer) {
				Array.Copy(dtbytes, 0, bytes, StartIndexSqlServer, CombDateSize);
			} else {
				// Nybble 6-9 move left to 5-8. Nybble 9 is set to "4" (the version)
				dtbytes[2] = (byte)((byte)(dtbytes[2] << 4) | (byte)(dtbytes[3] >> 4));
				dtbytes[3] = (byte)((byte)(dtbytes[3] << 4) | (byte)(dtbytes[4] >> 4));
				dtbytes[4] = (byte)(0x40 | (byte)(dtbytes[4] & 0x0F));
				// Overwrite the first six bytes
				Array.Copy(dtbytes, 0, bytes, 0, CombDateSize);
			}

			return new Guid(bytes);
		}

		// Private methods

		private static byte[] DateTimeToBytes(DateTime value) {
			if (value < MinCombDate) throw new ArgumentException($"COMB values only support dates on or after {MinCombDate}");
			if (value > MaxCombDate) throw new ArgumentException($"COMB values only support dates through {MaxCombDate}");
			// Convert the time to 300ths of a second. SQL Server uses float math for this before converting to an integer, so this does as well
			// to avoid rounding errors. This is confirmed in MSSQL by SELECT CONVERT(varchar, CAST(CAST(2 as binary(8)) AS datetime), 121),
			// which would return .006 if it were integer math, but it returns .007.
			var ticks = (int)(value.TimeOfDay.TotalMilliseconds * TicksPerMillisecond);
			var days = (ushort)(value - MinCombDate).TotalDays;
			var tickBytes = BitConverter.GetBytes(ticks);
			var dayBytes = BitConverter.GetBytes(days);

			if(BitConverter.IsLittleEndian) {
				// x86 platforms store the LEAST significant bytes first, we want the opposite for our arrays
				Array.Reverse(dayBytes);
				Array.Reverse(tickBytes);
			}

			var result = new byte[6];
			Array.Copy(dayBytes, 0, result, 0, 2);
			Array.Copy(tickBytes, 0, result, 2, 4);
			return result;
		}
		
		private static DateTime BytesToDateTime(byte[] value) {
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

			return MinCombDate.AddDays(days).AddMilliseconds((double)ticks / TicksPerMillisecond);
		}

	}

}