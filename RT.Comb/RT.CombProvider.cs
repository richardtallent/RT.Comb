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
	/// Default implementation of ICombProvider.
	/// </summary>
	public class CombProvider : ICombProvider {

		/// <summary>
		/// The variant of COMB that this provider creates.
		/// </summary>
		public CombVariant Variant { get; set; } = CombVariant.SqlServer;

		/// <summary>
		/// Creates a new COMB GUID using the current Variant, a new random GUID, and the current UTC timestamp.
		/// </summary>
		/// <returns>A new COMB GUID</returns>
		public Guid Create() => Comb.Create(Variant);

		/// <summary>
		/// Extracts the DateTime embedded in a COMB GUID of the current Variant.
		/// </summary>
		/// <param name="value">COMB GUID</param>
		/// <returns>DateTime embedded in <paramref name="value"/></returns>
		public DateTime FromComb(Guid value) => Comb.FromComb(value, Variant);

		/// <summary>
		/// Combines a given GUID and DateTime into a COMB GUID.
		/// </summary>
		/// <param name="value">Base GUID value</param>
		/// <param name="timestamp">DateTime value to combine with <paramref name="value"/></param>
		/// <returns>A new COMB that combines <paramref name="value"/> and <paramref name="timestamp"/></returns>
		public Guid ToComb(Guid value, DateTime timestamp) => Comb.ToComb(value, timestamp, Variant);

	}

}
