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

namespace RT.CombSql {

	/// <summary>
	/// Provides T-SQL expressions to generate COMB values that are compatible with the ones produced in C#.
	/// Intended primarily for testing.
	/// </summary>
	public static class CombSql {

		private static string DefaultGuidSql = @"NEWID()";
		private static string DefaultDateSql = @"GETUTCDATE()";
		private static string DateTimeSqlFormat = @"yyyy-MM-dd HH:mm:ss.fff";
		private static string GuidSqlFormat = @"D";

		public static string GetCombSql() {
			return GetCombSql(DefaultGuidSql, DefaultDateSql);
		}

		public static string GetCombSql(Guid guidBase, DateTime dateTimeStamp) {
			Comb.ThrowOnInvalidCombDate(dateTimeStamp);
			return GetCombSql(FormatLiteralSql(guidBase), FormatLiteralSql(dateTimeStamp));
		}

		public static string GetCombSql(Guid value) {
			return GetCombSql(FormatLiteralSql(value), DefaultDateSql);
		}

		public static string GetCombSql(DateTime value) {
			Comb.ThrowOnInvalidCombDate(value);
			return GetCombSql(DefaultGuidSql, FormatLiteralSql(value));
		}

		private static string GetCombSql(string guidSql, string dateSql) {
			return $"CAST(CAST({guidSql} AS binary(10)) + CAST({dateSql} AS binary(6)) AS uniqueidentifier)"; 
		}

		private static string FormatLiteralSql(Guid value) {
			return $"CAST('{value.ToString(GuidSqlFormat)}' AS uniqueidentifier)";
		}

		private static string FormatLiteralSql(DateTime value) {
			return  $"'{value.ToString(DateTimeSqlFormat)}'";
		}

	}

}