Purpose
=======
Invented by Jimmy Nilsson and first described in an article for InformIT in 2002, a COMB is a GUID with an embedded date/time value, making the values sequential over time.

This library is designed to create "COMB" `Guid` values in C#, and to be able to extract the `DateTime` value from an existing COMB value. I've found some random code here and there that purports to do something similar, but it either didn't support both variants of COMB (more on this later) or it was not independent enough to be grafted into my code.

Status
======
While I've created code to do this before, this library is my attempt to clean it up, add PostgreSQL support, and make it available as open source. The interface is still a bit in flux as I work with it, so please bear with me. Suggestions welcome.

Revision History
================
 - 1.1		2016-01		First release
 - 1.2		2016-01		Clean up, add unit tests
 - 1.3		2016-01-18	Major revision of interface
 - 1.4      2016-08-10  Upgrade to .NETCore 1.0, switch tests to Xunit

Using this Library
====================
This library includes two namespaces:

 - `RT.CombByteOrder`: COMB variant supporting byte-order sorting, such as PostgreSQL
 - `RT.CombSqlOrder`: COMB variant supporting SQL Server sorting.

Both of these have a static class called `Comb` that contains the core functions to create COMB Guids and extract dates from them. Both also have a class called `CombProvider` that implements `RT.ICombProvider`.

The `ICombProvider` interface is identical to that of the static classes. So, regardless which variant you use and whether you use the static classes or a provider instance, there are really only two methods to learn:

 - `Create()`, with overloads that take no arguments, a `Guid`, a `DateTime`, or both. All return a COMB Guid.
 - `GetTimestamp()`, which takes an existing COMB `Guid` and returns the embedded `DateTime` value.

While the static classes are more convenient, the `ICombProvider` interface is useful if you need to inject which variant to use rather than baking it in.

(I originally went down the path of having extension methods as well, but the semantics really don't work well, so I dropped them.)

When generating values, note that on Windows platforms, while a COMB's time resolution is around 3ms, the Windows system timer only has a resolution of around 15ms.

A NuGet package is available:

https://www.nuget.org/packages/RT.Comb/

Background
==========
When GUIDs (`uniqueidentifier` values in MSSQL parlance, `UUID` in PostgreSQL) are part of a database index, the randomness of new values can result in reduced performance, as insertions can fragment the index (or, for a clustered index, the table). In SQL Server 2005, Microsoft provided the `NEWSEQUENTIALID()` function to alleviate this issue, but despite the name, generated GUID values from that function are still not guaranteed to be sequential over time, nor do multiple instances of MSSQL produce values that would be sequential in relationship to one another.

The COMB method, however, takes advantage of the database's native sort ordering for GUID values and replaces the bytes that are sorted first with a date/time value, so values generated at the same time will always be generated in a *nearly* sequential order, regardless of server reboots or values generated on different machines (to the degree, of course, that the system clocks are synchronized).

As a side benefit, the COMB's sequential portion has the semantic value of being the date and time of insert, which can be useful from time to time.

While this does not alleviate all potential worries when deciding to use GUID fields in a table (they are still rather large, and can be unfriendly if exposed to end users), it does make them more palatable when they are called for.

GUID Data Structure
===================
*(For a more comprehensive treatment, see Wikipedia.)*

Standard GUIDs are 128-bit (16-byte) values, wherein most of those bits hold a random value. When viewed as a string, the bytes are represented in hex in five groups, separated by hyphens:

    xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
    00112233-4455-6677-8899-AABBCCDDEEFF
    version  ^
    variant            ^

Each character represents a nybble, and each byte is shown with the most significant nybble first, as usual (e.g., "0F" is 16, "F0" is 128).

The most significant 2-3 bits of nybble 17 are not random -- they store a "variant" value. For standard GUIDs, the first two bits are "10", the other two bits are random, and the 5th nybble stores the "version" number (usually "4").

PostgreSQL stores and sorts (and tools like pgAdmin3 display) `UUID` values as a 16-byte array, left to right. This conforms with the RFC 4122 standards for UUIDs.

However, Microsoft.NET's `Guid` data type (and MSSQL's `uniqueidentifier` type), when converted to a string, come out with the bytes in this order in the string:

	3210-54-76-89-ABCDEF

This is because Microsoft's GUID is a structure made up of a DWORD (32-bit), two WORDs (16-bit), and an 8-byte array. The x86 platform uses Little-endian form for the DWORD and WORD values. So if the first 4 bytes of a GUID are "FE 00 00 00", the DWORD value is 254. When that DWORD value is then converted to a hex string, it comes out as "000000FE", the reverse of the byte order.

Microsoft.NET sorts `Guid` bytes in the *order they are displayed as a string*. In other words, it sorts first by the DWORD *value* (right to left), then by the two WORD values (right to left), and finally by the remaining 8 bytes (left to right).

However, MSSQL (and .NET's `System.Data.SqlTypes.SqlGuid` type) sort Guids in s different order. Here is the order in which they are sorted:

    ABCDEF8967450123

In other words, byte offset `A` is sorted first, then `B`, etc., with the byte at offset `3` being the least significant sort position. Here is a fiddler illustrating this: https://dotnetfiddle.net/X5Sk3C

COMB Data Structure
===================

In a COMB value, we replace six bytes with part of an MSSQL `datetime` value.

The `datetime` type in MSSQL is an 8-byte structure. The first four bytes are a *signed* integer representing the number of days since January 1, 1900. Negative values are permitted to go back to 1752-01-01 (for Reasons), and positive values are permitted through 9999-12-12.

The remaining four bytes represent the time as the number of 300ths of a second since midnight. Since a day is always 24 hours, this integer never exceeds 25 bits of data, and since only whole numbers are supported, the sign is unused, so you can think of this as unsigned.

For the purposes of a COMB, we use all four bytes of the time, but only two bytes of the date. This means our date has no sign bit and has a maximum value of 65535. This limits our date range to January 1, 1900 through June 5, 2079.

We want to overwrite the bytes that will be sorted *first*, so our GUIDs are always in date/time order. This is why understanding the byte order and sorting is so important.

As mentioned above, MSSQL sorts the last six bytes are sorted first, left to right, so we overwrite our date/time value as follows:

    00112233-4455-6677-8899-AABBCCDDEEFF
    xxxxxxxx 4xxx xxxx Vxxx DDDD0TTTTTTT
                            554433221100
	                        date    time
	V = b10xx
	x = random

(Note that I've marked the most significant nybble of the time above as "0" rather than "T", since its value will always be 0.)

For PostgreSQL, the *first* six bytes are sorted first, so those are the ones we want to overwrite. But there's a problem -- the most significant four bits of the 5th byte should store the UUID version number (4), not our timestemp.

Here, we can take advantage of the fact that our time value only needs 28 of its 32 bits. We shift the first two bytes of the time left by four bits, put a "4" in the 5th nybble. We actually only use 25 of the bits, not the full 28, so we won't come anywhere near the sign bit.

    DDDDTTTT-4TTT-xxxx-Vxxx-xxxxxxxxxxxx
    00112334  455
    date      time
	V = b100x
	x = random

Note above that byte position 2 is only mentioned once -- we only represent its least significant nybble.

When PostgreSQL `UUID` values are read into .NET applications as `Guid` values, the string representation and default sort will still be different from the byte order.

This still leaves us 2^74 possible values for every 1/300th of a second, so there should be no cause for concern about the embedded timestamp creating any significant risk of GUID value collision.

Converting To/From COMB values in T-SQL
=======================================

For reference, here is how to create an equivalent COMB `uniqueidentifier` in T-SQL with the current date and time:

    CAST(CAST(NEWID() AS binary(10)) + CAST(GETUTCDATE() AS binary(6)) AS uniqueidentifier)

Here is how one would extract the `datetime` value from a COMB `uniqueidentifier`:

	CAST(SUBSTRING(CAST(0 AS binary(2)) + CAST(value AS binary(16), 10, 6) AS datetime)

The overhead for both of these in MSSQL is minimal.

How To Contribute
=================
Some missing pieces:
- It would be nice to have a custom `IComparer` method that sorts `Guid` values the same way as `SqlGuid`, and another that would compare them in byte order for COMBs generated for PostgreSQL.
- More unit tests.
- Please keep all contributions compatible with .NET Core.
- Please use the same style (tabs, same-line opening braces, compact line spacing, etc.)
- Please keep project/solution files compatible with Visual Studio 2015 Community Edition.

Security and Performance
========================
(1) It's a good idea to always use UTC date/time values for COMBs, so they remain highly sequential regardless of server locale or daylight savings, etc. This is even more important if the values will be generated on multiple machines that may have different time zone settings.

(2) It should go without saying, but using a COMB "leaks" the date/time to anyone knows how to decode it, so don't use one if this information (or even the relative order of their creation) should remain private.

(3) Don't use this with MySQL if you plan to store GUIDs as varchar(36) -- performance will be terrible.

(4) Test. Don't assume that a GUID key will be significantly slower for you than a 32-bit `int` field. Don't assume it will be roughly the same. The relative size of your tables (and especially of your indexed columns) compared to your primary key column will impact the overall database size and relative performance. I use COMB values frequently in moderate-sized databases without any performance issues, but YMMV.

(5) The best use for a COMB is where (a) you want the range and randomness of the GUID structure without index splits under load, and (b) any actual use of the embedded timestamp is rare (for example, for debugging purposes).

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