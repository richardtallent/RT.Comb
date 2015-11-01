using System;
/*
   Copyright 2015 Richard S. Tallent, II

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

namespace RT.CombUtils {

	/// <summary>
	/// A COMB is a GUID where 6 bytes are replaced with an SQL Server datetime value in a way that
	/// results in GUIDs that are in date/time order in SQL Server. The date portion of the datetime
	/// structure is truncated from four bytes to two, limiting the date range to 1900-01-01 to 2079-06-06.
	/// </summary>
	public static class Comb {

		private static readonly DateTime MinCombDate = new DateTime(1900, 1, 1);
		private static readonly DateTime MaxCombDate = MinCombDate.AddDays(ushort.MaxValue);
		private static readonly double TicksPerDay = 86400d * 300d;
		private static readonly double TicksPerMillisecond = 3d / 10d;
		private const int DayByteIndex = 10;        // Byte position where day value starts
		private const int TickByteIndex = 12;       // Byte position where time value starts

		/// <summary>
		/// Generates a COMB with a random Guid and the current UTC datetime. This is probably the method that will be used the most often.
		/// </summary>
		public static Guid NewComb() {
			return NewComb(Guid.NewGuid(), DateTime.UtcNow);
		}

		/// <summary>
		/// Combine a specified Guid and DateTime value.
		/// </summary>
		public static Guid NewComb(Guid guid, DateTime embedDateTime) {
			ThrowOnInvalidCombDate(embedDateTime);
			// Convert the time to 300ths of a second. SQL Server uses float math for this before converting to an integer, so this does as well
			// to avoid rounding errors. This is confirmed in MSSQL by SELECT CONVERT(varchar, CAST(CAST(2 as binary(8)) AS datetime), 121),
			// which would return .006 if it were integer math, but it returns .007.
			var ticks = (int)(embedDateTime.TimeOfDay.TotalMilliseconds * TicksPerMillisecond); // Convert to nearest 300ths of a second
			var tickBytes = BitConverter.GetBytes(ticks);
			var days = (ushort)(embedDateTime - MinCombDate).TotalDays;
			var dayBytes = BitConverter.GetBytes(days);
			if(BitConverter.IsLittleEndian) {
				// GUID uses Big Endian, so if the platform uses Little Endian, reverse the byte order.
				Array.Reverse(dayBytes);
				Array.Reverse(tickBytes);
			}
			var result = guid.ToByteArray();
			Array.Copy(dayBytes, 0, result, DayByteIndex, 2);
			Array.Copy(tickBytes, 0, result, TickByteIndex, 4);
			return new Guid(result);
		}

		/// <summary>
		/// Extracts the DateTime from an existing COMB. May be accessed as an extension on Guid.
		/// </summary>
		public static DateTime ExtractCombDateTime(this Guid value, bool throwOnFail = true) {
			var guidBytes = value.ToByteArray();
			var dayBytes = new byte[2];                     // Empty ushort
			var tickBytes = new byte[4];                    // Empty int
			Array.Copy(guidBytes, DayByteIndex, dayBytes, 0, 2);
			Array.Copy(guidBytes, TickByteIndex, tickBytes, 0, 4);
			if(BitConverter.IsLittleEndian) {
				// GUID uses Big Endian, so if the platform uses Little Endian, reverse the byte order.
				Array.Reverse(dayBytes);
				Array.Reverse(tickBytes);
			}
			var days = BitConverter.ToUInt16(dayBytes, 0);
			var ticks = (double)BitConverter.ToInt32(tickBytes, 0);
			if(ticks < 0f) {
				if(throwOnFail) {
					throw new ArgumentException("A negative time component is unexpected.");
				} else {
					ticks = 0f;
				}
			} else if(ticks > TicksPerDay) {
				if(throwOnFail) {
					throw new ArgumentException("A time component exceeding 24 hours is unexpected.");
				} else {
					ticks = 0f;
				}
			}
			return MinCombDate.AddDays(days).AddMilliseconds(ticks / TicksPerMillisecond);
		}

		public static void ThrowOnInvalidCombDate(DateTime value) {
			if (value < MinCombDate) throw new ArgumentException($"COMB values only support dates on or after {MinCombDate}");
			if (value > MaxCombDate) throw new ArgumentException($"COMB values only support dates through {MaxCombDate}");
		}

	}

}