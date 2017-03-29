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
	/// This strategy stores the Unix epoch timestamp, scaled to milliseconds, in 48 unsigned bits.
	/// The allowed date range is 1970-01-01 to 8419-05-26.
	/// </summary>
    public class UnixDateTimeStrategy : ICombDateTimeStrategy {

		public int NumDateBytes { get { return 6; } }

		public DateTime MinDateTimeValue { get; } = new DateTime(1970,1,1,0,0,0,0,System.DateTimeKind.Utc);

		public DateTime MaxDateTimeValue { get { return MinDateTimeValue.AddMilliseconds(2 ^ (8 * NumDateBytes)); } }

		public byte[] DateTimeToBytes(DateTime timestamp) {
			var ms = ToUnixTimeMilliseconds(timestamp);
			var msBytes = BitConverter.GetBytes(ms);
			if(BitConverter.IsLittleEndian) Array.Reverse(msBytes);
			var result = new byte[NumDateBytes];
			var index = msBytes.GetUpperBound(0) + 1 - NumDateBytes;
			Array.Copy(msBytes, index, result, 0, NumDateBytes);
			return result;
		}

		public DateTime BytesToDateTime(byte[] value) {
			// Attempt to convert the first 6 bytes.
			var msBytes = new byte[8];
			var index = 8 - NumDateBytes;
			Array.Copy(value, 0, msBytes, index, NumDateBytes);
			if(BitConverter.IsLittleEndian) Array.Reverse(msBytes);
			var ms = BitConverter.ToInt64(msBytes, 0);
			return FromUnixTimeMilliseconds(ms);
		}

		// From private DateTime.TicksPerMillisecond
		private const long TicksPerMillisecond = 10000;

		// We purposefully are not using the FromUnixTimeMilliseconds and ToUnixTimeMilliseconds to remain compatible with .NET 4.5.1
		//(long)(timestamp.ToUniversalTime() - MinDateTimeValue).TotalMilliseconds;
		//public DateTime FromUnixTimeMilliseconds(long ms) => MinDateTimeValue.AddMilliseconds(ms);

		public long ToUnixTimeMilliseconds(DateTime timestamp) => (timestamp.Ticks - MinDateTimeValue.Ticks) / TicksPerMillisecond; 

		public DateTime FromUnixTimeMilliseconds(long ms) => MinDateTimeValue.AddTicks(ms * TicksPerMillisecond);

    }

}