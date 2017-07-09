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

    public class PostgreSqlCombProvider : BaseCombProvider {

		public PostgreSqlCombProvider(ICombDateTimeStrategy dateTimeStrategy, TimestampProvider customTimestampProvider = null, GuidProvider customGuidProvider = null) : base(dateTimeStrategy, customTimestampProvider, customGuidProvider) {}

		public override Guid Create(Guid value, DateTime timestamp) {
			var gbytes = value.ToByteArray();
			var dbytes = _dateTimeStrategy.DateTimeToBytes(timestamp);
			Array.Copy(dbytes, 0, gbytes, 0, _dateTimeStrategy.NumDateBytes);
			SwapByteOrderForStringOrder(gbytes);
			return new Guid(gbytes);
		}

		public override DateTime GetTimestamp(Guid comb) {
			var gbytes = comb.ToByteArray();
			var dbytes = new byte[_dateTimeStrategy.NumDateBytes];
			SwapByteOrderForStringOrder(gbytes);
			Array.Copy(gbytes, 0, dbytes, 0, _dateTimeStrategy.NumDateBytes);
			return _dateTimeStrategy.BytesToDateTime(dbytes);
		}

		// IDateTimeStrategy is required to provide the bytes we need in network byte order, and that's what we want
		// in PostgreSQL as well. However, internally, GUID bytes for Data1 and Data2 are stored in little endian
		// order, so we need to reverse one or both of those so the bytes we want are re-reversed to the correct 
		// order by Npgsql's GUID data type handler.
		private void SwapByteOrderForStringOrder(byte[] input) {
			Array.Reverse(input,0,4);				// Swap around the first 4 bytes
			if(input.Length==4) return;
			Array.Reverse(input,4,2);				// Swap around the next 2 bytes
		}

    }

}