
Purpose
=======
Invented by Jimmy Nilsson and first described in an article for InformIT in 2002, a COMB is a GUID ("uniqueidentifier" in MSSQL) with an embedded date/time value, making the values sequential over time.

This library is designed to create "COMB" Guid values in C#, and to be able to extract the Date/Time value from an existing COMB value.

Background
==========
When GUIDs (`uniqueidentifier` values in MSSQL parlance) are part of a database index, the randomness of the values can result in reduced performance, as insertions of new data result in page splits. In SQL Server 2005, Microsoft provided the `NEWSEQUENTIALID()` function to alleviate this issue, but despite the name, generated GUID values from that function are still not guaranteed to be sequential over time.

The COMB method, however, takes advantage of SQL Server's sort ordering for `uniqueidentifier` values and replaces the  bytes that are sorted first with a date/time value, so values generated at the same time will always be generated in order or very close to it, regardless of regardless of server reboots or values generated on different machines (to the degree, of course, that the system clocks are synchronized).

As a side benefit, the COMB's sequential portion has the semantic value of being the date and time of insert, which can be useful from time to time.

While this does not alleviate all potential worries when deciding to use GUID fields in a table (they are still rather large, and can be unfriendly if exposed to end users), it does make them more palatable when they are called for.

Data Structure
==============
*(Some details here are Microsoft-isms, not necessarily part of GUID or UUID standards universally. For a more comprehensive treatment, see Wikipedia.)*

Standard GUIDs are 128-bit (16-byte) values, wherein 125 of those bits hold a random value. When viewed as a string, the bytes are represented in hex in five groups, separated by hyphens:

					xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx

This string is not showing the values in byte order. Here's are the byte indexes in the order shown in the string (numbered 0-F):

					3210-54-76-89-ABCDEF

The reason for this is that Microsoft uses an DWORD (32-bit) and two WORDs (16-bit) to store the first half of the structure, and an 8-byte array for the second half. On the x86 platform, the DWORD and WORD values are *stored* in Little-Endian form (most significant bytes on the right), but are shown in Big-Endian form (most significant bytes to the left).

Looking at a string representation of a GUID, SQL Server sorts with this order of precedence (most significant byte = 0,least significant = F):

					FEDC-BA-98-67-012345

.NET does the same for `System.Data.SqlTypes.SqlGuid` values, but for `System.Guid` values, it sorts the bytes in the order they appear in string form, *i.e.*:

					0123-45-67-89-ABCDEF

Fiddler illustrating this: https://dotnetfiddle.net/X5Sk3C

In a COMB value, we replace the last six bytes with part of an MSSQL `datetime` value. The `datetime` type in MSSQL is an 8-byte structure. The first four bytes are a *signed* integer representing the number of days since January 1, 1900. Negative values are permitted to go back to 1752-01-01 (for reasons), and positive values are permitted through 9999-12-12. The remaining four bytes represent the time as the number of 300ths of a second since midnight. Since a day is always 24 hours, this integer never exceeds 25 bits of data, and since only whole numbers are supported, the sign is unused, so you can think of this as either signed or unsigned.

For the purposes of a COMB, we inject the two least significant bytes of the date portion and all four bytes from the time into the GUID. This leaves us with a GUID broken down like this:

								XXXXXXXX-XXXX-XXXX-XXXX-DDDDTTTTTTTT
								random-ish				date    time

Thus, MSSQL (And .NET when using the `SqlGuid` type) will sort these in date/time order. Since we reduce the date portion from 4 bytes to two, we have no sign bit and we're limited to a top value of 65535. This limits our date range to January 1, 1900 through June 6, 2979. This will do nicely for a lifetime of timestamps.

Implementation/Usage
====================
Rather than creating a new struct or class, this library simply provides some utility methods for dealing with COMB Guid values, including some extension methods on Guid and DateTime.

Helper methods are also provided to generate sample T-SQL to create COMB values. It is expected that these would only be used for testing, I can't imagine they would be practical for much else.

Some minor features require C# 6. If this is a major stumbling block, I can move back to code that can target earlier versions of .NET.

Since the C# code uses an `int` for the time and `ushort` for the date, when the code converts these to bytes to inject in the GUID, it verifies that the platform stores these words in Little-Endian form and reverses the byte order (since we want those values in Big-Endian order in our final byte array). All .NET implementations I know of are on Little-Endian platforms, but I added the check anyway.

The methods are all static, and are mostly overloads where one or both of the `Guid` and `DateTime` components are either provided by the caller or generated automatically. `Guid` values generated automatically will be random, and `DateTime` values generated automatically use the current UTC timestamp. Note that on Windows platforms, the system timer resolution is around 15ms, but the time portion of the COMB can resolve down to around 3ms. This slightly increases the chances that you might otherwise expect to get the same date/time value for high-frequency calls.

How To Contribute
=================
Some missing pieces:

1. I'm having trouble adding an Xunit project to create unit tests, so I could use some help on that front.
2. The code currently only works with Guid values, I'd like to have it work with `SqlGuid` values as well.
3. It would be nice to have a custom `IComparer` method that sorts `Guid` values the same way as `SqlGuid`.
4. It might be nice to have an alternate implementation that uses `GetSystemTimeAsFileTime()` to provide time resolution that is equivalent with MSSQL's `DATETIME2` type and `SYSUTCDATETIME` function. Perhaps call it COMB2?

I'm using Visual Studio 2015 Community Edition (over VirtualBox on my home Mac), targeted to stable .NET versions, so please keep contributions compatible with that configuration. Also, I do use tabs and I like compact code (minimal line breaks). I know I'm in the minority on this coding style, but please don't send a pull request just to "fix" my formatting.

Security and Performance
========================
(1) It's a good idea to always use UTC date/time values for COMBs, so they remain highly sequential regardless of server locale or daylight savings, etc. This is even more important if the values will be generated on multiple machines that may have different time zone settings.

(2) It should go without saying, but using a COMB "leaks" the date/time to anyone knows how to decode it, so don't use one if this information (or, more generally, the relative order of their creation) should remain private.

(3) While this is not my area of expertise, because of the increased predictability of a COMB value, it may be a poor choice to use as a cryptographic seed.

(4) Don't use this with MySQL -- GUIDs are stored as varchar(36), so performance will be terrible.

(5) Test, test, test. Don't assume that a 128-bit key will be significantly slower for you than a 32-bit traditional int key. Don't assume it will be roughly the same. The relative size of your tables (and especially of your indexed columns) compared to your primary key column will have a big impact on the overall database size and performance hit. I use COMB values frequently in moderate-sized databases without any issues, but YMMV.

(6) It's best to use these in a database table only if there are no other date/time stamp columns that duplicate this information, otherwise you have a functional dependency. The best use for a COMB is where (a) you want the range and randomness of the GUID structure without page splits under load, and (b) any actual use of the date/time information is rare (for example, for debugging purposes).

More Information
=================================

The original COMB article:
http://www.informit.com/articles/article.aspx?p=25862

Another implementation (not compatible):
http://www.siepman.nl/blog/post/2013/10/28/ID-Sequential-Guid-COMB-Vs-Int-Identity-using-Entity-Framework.aspx

License (MIT "Expat")
=====================
Copyright 2015 Richard S. Tallent, II

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
