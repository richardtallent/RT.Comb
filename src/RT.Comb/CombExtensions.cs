using System;
using RT.CombUtils;
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

namespace RT.CombExtensions {

	/// <summary>
	/// Provides handy extension methods for Combs that can be used on Guid and DateTime instances.
	/// </summary>
	public static class CombExtensions {

		public static Guid ToComb(this DateTime value) {
			return Comb.NewComb(Guid.NewGuid(), value);
		}

		public static Guid ToComb(this Guid value) {
			return Comb.NewComb(value, DateTime.UtcNow);
		}

		public static DateTime CombToDateTime(this Guid value, bool throwOnFail = true) {
			return Comb.ExtractCombDateTime(value, throwOnFail);
		}

	}

}