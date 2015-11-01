using System;
using RT.CombUtils;
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