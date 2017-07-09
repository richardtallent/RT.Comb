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

    public class SqlCombProvider : BaseCombProvider {

		private const int EmbedAtIndex = 10;

		public SqlCombProvider(ICombDateTimeStrategy dateTimeStrategy, TimestampProvider customTimestampProvider = null, GuidProvider customGuidProvider = null) : base(dateTimeStrategy, customTimestampProvider, customGuidProvider) {}

		public override Guid Create(Guid value, DateTime timestamp) {
			var gbytes = value.ToByteArray();
			var dbytes = _dateTimeStrategy.DateTimeToBytes(timestamp);
			Array.Copy(dbytes, 0, gbytes, EmbedAtIndex, _dateTimeStrategy.NumDateBytes);
			return new Guid(gbytes);
		}

		public override DateTime GetTimestamp(Guid comb) {
			var gbytes = comb.ToByteArray();
			var dbytes = new byte[_dateTimeStrategy.NumDateBytes];
			Array.Copy(gbytes, EmbedAtIndex, dbytes, 0, _dateTimeStrategy.NumDateBytes);
			return _dateTimeStrategy.BytesToDateTime(dbytes);
		}

    }

}