using System;
/*
	Copyright 2015-2016 Richard S. Tallent, II

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

    public class CombProvider : ICombProvider {

		private readonly CombProviderOptions _options = null;

		public CombProvider(CombProviderOptions options) {
			_options = options;
		}

		public Guid Create() => Create(NewGuid(), DateTime.UtcNow);

		public Guid Create(Guid value) => Create(value, DateTime.UtcNow);

		public Guid Create(DateTime timestamp) => Create(NewGuid(), timestamp);

		public Guid Create(Guid value, DateTime timestamp) {
			var gbytes = value.ToByteArray();
			var dbytes = _options.DateTimeStrategy.DateTimeToBytes(timestamp);
			Array.Copy(dbytes, 0, gbytes, _options.GuidOffset, _options.DateTimeStrategy.NumDateBytes);
			return new Guid(gbytes);
		}

		public DateTime GetTimestamp(Guid comb) {
			var gbytes = comb.ToByteArray();
			var dbytes = new byte[_options.DateTimeStrategy.NumDateBytes];
			Array.Copy(gbytes, _options.GuidOffset, dbytes, 0, _options.DateTimeStrategy.NumDateBytes);
			return _options.DateTimeStrategy.BytesToDateTime(dbytes);
		}

		private Guid NewGuid() => Guid.NewGuid();

    }

}