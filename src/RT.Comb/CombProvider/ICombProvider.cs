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

	/// <summary>
	/// An interface for working with COMB GUIDs, implemented for both variants.
	/// </summary>
	public interface ICombProvider {

		/// <summary>
		/// Returns a new Guid COMB, consisting of a random Guid combined with the current UTC timestamp.
		/// </summary>
		Guid Create();

		/// <summary>
		/// Returns a new Guid COMB, consisting of the specified Guid combined with the current UTC timestamp.
		/// </summary>
		Guid Create(Guid value);

		/// <summary>
		/// Returns a new Guid COMB, consisting of a random Guid combined with the provided timestamp.
		/// </summary>
		Guid Create(DateTime timestamp);

		/// <summary>
		/// Returns a new Guid COMB, consisting of the provided Guid and provided timestamp.
		/// </summary>
		Guid Create(Guid value, DateTime timestamp);

		/// <summary>
		/// Extracts the DateTime embedded in a COMB GUID of the current Variant.
		/// </summary>
		/// <param name="value">COMB GUID</param>
		/// <returns>DateTime embedded in <paramref name="value"/></returns>
		DateTime GetTimestamp(Guid value);

		/// <summary>
		/// Provides the timestamp value for Create() calls that don't include a timestamp argument.
		/// </summary>
		//TimestampProvider TimestampProvider { get; }
		//GuidProvider GuidProvider { get; }

	}

}