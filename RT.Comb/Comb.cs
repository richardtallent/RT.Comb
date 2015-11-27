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

namespace RT.CombUtils {

	/// <summary>
	/// A COMB is a GUID where 6 bytes are replaced with an SQL Server datetime value in a way that
	/// results in GUIDs that are in date/time order in SQL Server. The date portion of the datetime
	/// structure is truncated from four bytes to two, limiting the date range to 1900-01-01 to 2079-06-06.
	/// There are several ways of injecting this value into the GUID, depending on the database platform.
	/// </summary>

	public static class Comb {

		// SQL Server variant

		public static DateTime DecodeCombSqlOrder(Guid Value) {
			var gbytes = Value.ToByteArray();
			var dtbytes = new byte[6];
			Array.Copy(gbytes, 8, dtbytes, 0, 6);
			return BytesToDateTime(dtbytes);
		}

		public static Guid EncodeCombSqlOrder(Guid Value) {
			return EncodeCombSqlOrder(Value, DateTime.UtcNow);
		}
		
		public static Guid EncodeCombSqlOrder(Guid Value, DateTime Timestamp) {
			var gbytes = Value.ToByteArray();
			var dtbytes = DateTimeToBytes(Timestamp);
			Array.Copy(dtbytes, 0, gbytes, 10, 6);
			return new Guid(gbytes);
		}

		// Byte order variant (PostgreSql, others)

		public static DateTime DecodeCombByteOrder(Guid Value) {
			var gbytes = Value.ToByteArray();
			var dtbytes = new byte[6];
			Array.Copy(gbytes, 0, dtbytes, 0, 6);
			// Move nybbles 5-8 to 6-9, overwriting the existing 9 (version "4")
			dtbytes[4] = (byte)((byte)(dtbytes[3] << 4) | (byte)(dtbytes[4] & 0x0F));
			dtbytes[3] = (byte)((byte)(dtbytes[2] << 4) | (byte)(dtbytes[3] >> 4));
			dtbytes[2] = (byte)(dtbytes[2] >> 4);
			return BytesToDateTime(dtbytes);
		}

		public static Guid EncodeCombByteOrder(Guid Value) {
			return EncodeCombByteOrder(Value, DateTime.UtcNow);
		}

		public static Guid EncodeCombByteOrder(Guid Value, DateTime Timestamp) {
			var dtbytes = DateTimeToBytes(Timestamp);
			// Nybble 6-9 move left to 5-8
			// Nybble 9 is set to "4" (the version)
			dtbytes[2] = (byte)((byte)(dtbytes[2] << 4) | (byte)(dtbytes[3] >> 4));
			dtbytes[3] = (byte)((byte)(dtbytes[3] << 4) | (byte)(dtbytes[4] >> 4));
			dtbytes[4] = (byte)(0x40 | (byte)(dtbytes[4] & 0x0F));
			// Overwrite the first six bytes
			var gbytes = Value.ToByteArray();
			Array.Copy(dtbytes, 0, gbytes, 0, 6);
			return new Guid(gbytes);
		}

		// Reference values and Utilities

		private static readonly DateTime MinCombDate = new DateTime(1900, 1, 1);
		private static readonly DateTime MaxCombDate = MinCombDate.AddDays(ushort.MaxValue);
		private static readonly double TicksPerDay = 86400d * 300d;
		private static readonly double TicksPerMillisecond = 3d / 10d;

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
			var result = new Byte[6];
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