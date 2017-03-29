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

namespace RT.Comb {

	public delegate DateTime TimestampProvider();

	public class UtcNoRepeatTimestampProvider {

		private DateTime lastValue = DateTime.MinValue;
		private object locker = new object();
		
		// By default, increment any subsequent requests by 4ms, which overcomes the resolution of 1/300s of SqlDateTimeStrategy
		// If using UnixDateTimeStrategy, you can set this to as low as 1ms.
		public double IncrementMs { get; set; } = 4;

		public DateTime GetTimestamp() {
			var now = DateTime.UtcNow;
			lock(locker) {
				// Ensure the time difference between the last value and this one is at least the increment threshold
				if((now - lastValue).TotalMilliseconds < IncrementMs) {
					// now is too close to the last value, use the value with minimum distance from lastValue
					now = lastValue.AddMilliseconds(IncrementMs);
				}
				lastValue = now;
			}
			return now;
		}

	}

}