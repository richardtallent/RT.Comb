Purpose
=======
Invented by Jimmy Nilsson and first described in an article for InformIT in 2002, a COMB is a GUID with an embedded date/time value, making the values sequential over time.

This library is designed to create "COMB" `Guid` values in C#, and to be able to extract the `DateTime` value from an existing COMB value. I've found some random code here and there that purports to do something similar, but it either didn't support both variants of COMB (more on this later) or it was not independent enough to be grafted into my code.

Status
======
While I've created code to do this before, this library is my attempt to clean it up, add PostgreSQL support, and make it available as open source. Suggestions welcome.

Revision History
================
 - 1.1		2016-01		First release
 - 1.2		2016-01		Clean up, add unit tests
 - 1.3		2016-01-18	Major revision of interface
 - 1.4      2016-08-10  Upgrade to .NETCore 1.0, switch tests to Xunit
 - 2.0      2016-11-19	Corrected byte-order placement, reorganized for better DI, upgraded to .NETCore 1.1, downgraded to .NET 4.5.1

Background
==========
When GUIDs (`uniqueidentifier` values in MSSQL parlance, `UUID` in PostgreSQL) are part of a database index, the randomness of new values can result in reduced performance, as insertions can fragment the index (or, for a clustered index, the table). In SQL Server 2005, Microsoft provided the `NEWSEQUENTIALID()` function to alleviate this issue, but despite the name, generated GUID values from that function are still not guaranteed to be sequential over time, nor do multiple instances of MSSQL produce values that would be sequential in relationship to one another.

The COMB method, however, takes advantage of the database's native sort ordering for GUID values and replaces the bytes that are sorted first with a date/time value, so values generated at the same time will always be generated in a *nearly* sequential order, regardless of server reboots or values generated on different machines (to the degree, of course, that the system clocks are synchronized).

As a side benefit, the COMB's sequential portion has the semantic value of being the date and time of insert, which can be useful from time to time.

While this does not alleviate all potential worries when deciding to use GUID fields in a table (they are still rather large, and can be unfriendly if exposed to end users), it does make them more palatable when they are called for.

Using this Library
====================
A NuGet package is available, dual-targeted for .NET Core 1.1 and Microsoft.NET 4.5.1:
https://www.nuget.org/packages/RT.Comb/

Everything is within the `RT.Comb` namespace.

The `ICombProvider` interface provides a `Create()` function (with several signature variants) to return a new COMB `Guid` value, and a `GetTimestamp()` function to return the embedded `DateTime` within an existing COMB value.

`CombProvider` implements this interface. Its constructor takes a single argument of type `CombProviderOptions`. The options currently include:
 - `GuidOffset` (`int`), the byte position where the embedded timestamp should begin.
 - `DateTimeStrategy` (`IDateTimeStrategy`), which handles the conversion between `DateTime` values and the COMB-embedded byte array.

The idea here is to make it possible to create new variations on the COMB structure to fit your needs without having to reimplement `ICombProvider`
from scratch.

Two `IDateTimeStrategy` implementations are included. Both happen to encode DateTime values in 6 bytes, but other implementations could use fewer or more bytes:
 - `SqlDateTimeStrategy`, which encodes the DateTime in a way that is highly compatible with Microsoft SQL Server's `datetime` data type.
 - `UnixDateTimeStrategy`, which encodes the DateTime using the millisecond version of the Unix epoch timestamp.

Your choice of strategy will probably be dictated by your chosen database platform, but the option is yours, or you can build your own strategy if you want higher or lower time resolution, a different epoch date, etc.

Prior to version 2.0, I used static classes with hard-coded options. With 2.0, I switched to a traditional class to provide more flexibility and to allow me to inject the preferred options as a Singleton using ASP.NET Core. The class is thread-safe.

GUID Data Structure
===================
*(For a more comprehensive treatment, see Wikipedia.)*

Standard GUIDs are 128-bit (16-byte) values, wherein most of those bits hold a random value. When viewed as a string, the bytes are represented in hex in five groups, separated by hyphens:

    xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
    00112233-4455-6677-8899-AABBCCDDEEFF
    version       ^
    variant            ^

Each character represents a nybble, and each byte is shown with the most significant nybble first, as usual (e.g., "0F" is 16, "F0" is 128).

The most significant 2-3 bits of nybble 17 are not random -- they store a "variant" value. For standard GUIDs, the first two bits are "10", the other two bits are random, and the 13th nybble stores the "version" number (usually "4").

PostgreSQL stores and sorts (and tools like pgAdmin3 display) `UUID` values as a 16-byte array, left to right. This conforms with the RFC 4122 standards for UUIDs.

However, Microsoft.NET's `Guid` data type (and MSSQL's `uniqueidentifier` type), when converted to a string, come out with the bytes in this order in the string:

	3210-54-76-89-ABCDEF

This is because Microsoft's GUID is a structure made up of a DWORD (32-bit), two WORDs (16-bit), and an 8-byte array. The x86 platform uses Little-endian form for the DWORD and WORD values. So if the first 4 bytes of a GUID are "FE 00 00 00", the DWORD value is 254. When that DWORD value is then converted to a hex string, it comes out as "000000FE", the reverse of the byte order.

Microsoft.NET *sorts* `Guid` bytes in the *order they are displayed as a string*. In other words, it sorts first by the DWORD *value* (right to left), then by the two WORD values (right to left), and finally by the remaining 8 bytes (left to right).

However, MSSQL (and .NET's `System.Data.SqlTypes.SqlGuid` type) sort Guids in a different order. Here is the order in which they are sorted:

    ABCDEF8967450123

In other words, byte offset `A` is sorted first, then `B`, etc., with the byte at offset `3` being the least significant sort position. Here is a fiddler illustrating this: https://dotnetfiddle.net/X5Sk3C

SqlDateTimeStrategy Data Structure
==================================
This strategy replaces 6 bytes of the GUID value with *part* of an MSSQL `datetime` value. Assuming you use the same offset (10), this strategy is compatible with the original COMB article, and is easy to convert back and forth to `datetime` values using T-SQL. Overwriting six bytes still leaves us 2^74 possible random values for every 1/300th of a second, so there should be no cause for concern about the embedded timestamp creating any significant risk of GUID value collision.

The `datetime` type in MSSQL is an 8-byte structure. The first four bytes are a *signed* integer representing the number of days since January 1, 1900. Negative values are permitted to go back to 1752-01-01 (for Reasons), and positive values are permitted through 9999-12-12. The remaining four bytes represent the time as the number of 300ths of a second since midnight. Since a day is always 24 hours, this integer never exceeds 25 bits of data and is never negative, so you can think of this value as either signed or unsigned.

For the COMB, we use all four bytes of the time, but only two bytes of the date. This means our date has no sign bit and has a maximum value of 65535. This limits our date range to January 1, 1900 through June 5, 2079 -- a very reasonable spread for either historical or future transactional data.

This is the recommended implementation for use with MSSQL *if* you want to easily encode or decode the timestamp directly in T-SQL without use of .NET functions. Here is some reference code for doing so (using the recommended GuidOffset, keep reading for more on that option):

Creating a COMB `uniqueidentifier` in T-SQL with the current date and time:

    CAST(CAST(NEWID() AS binary(10)) + CAST(GETUTCDATE() AS binary(6)) AS uniqueidentifier)

Extracting a `datetime` value from a COMB `uniqueidentifier` created using the above T-SQL:

	CAST(SUBSTRING(CAST(0 AS binary(2)) + CAST(value AS binary(16), 10, 6) AS datetime)

The overhead for both of these in MSSQL is minimal.

UnixDateTimeStrategy Data Structure
===================================
This implementation was inspired by work done on the Marten project, which uses a modified version of RT.Comb. This also overwrites 6 bytes of the GUID, and as with SqlDateTimeStrategy, the other 74 random bits provide well beyond a reasonable amount of collision protection within each millisecond. The value is a 48-bit unsigned integer representing the number of *milliseconds* since the UNIX epoch date (1970-01-01). Since this method is far more space-efficient than the MSSQL `datetime` structure, it will cover you well into the 87th Century. This is the recommended implementation for use with non-Microsoft RDBMS packages such as PostgreSQL.

Guid Offsets
============
Regardless which strategy is used for encoding the timestamp, we need to overwrite the portion of the GUID that is sorted *first*, so our GUIDs are sortable in date/time order and minimize index page splits. This differs by database platform, so it is important to understand how your chosen platform sorts GUID values and choose the appropriate offset.

MSSQL sorts the last six bytes are first, left to right, so we plug our timestamp into the GUID structure starting at byte offset 10 (byte #11), as follows:

    00112233-4455-6677-8899-AABBCCDDEEFF
    xxxxxxxx xxxx 4xxx Vxxx MMMMMMMMMMMM  UnixDateTimeStrategy, milliseconds
    xxxxxxxx xxxx 4xxx Vxxx DDDDTTTTTTTT  SqlDateTimeStrategy, days and 1/300s
	4 = version
    V = variant
	x = random

For PostgreSQL, the *first* six bytes are sorted first, so those are the ones we want to overwrite.

    MMMMMMMM MMMM 4xxx Vxxx xxxxxxxxxxxx  UnixDateTimeStrategy, milliseconds
    DDDDTTTT TTTT 4xxx Vxxx xxxxxxxxxxxx  SqlDateTimeStrategy, days and 1/300s
	4 = version
	V = variant
	x = random

The `Constants` static class provides default offset values for MSSQL and PostgreSQL that you can use in your `CombOptions`.

NOTE: in version 1.4 and prior, I made the misplaced the version number in byte 5 and did some bit-gymnastics to move the time around where I thought the number "4" would be. My error was kindly pointed out by Barry Hagan and has been fixed. **As a result, COMB values created with version 1.4 and below using the byte order method are not compatible with version 2.0 and above.** If this creates a headache for you, please let me know and we can come up with a solution. Fortunately I don't believe my little library is in heavy use in PostgreSQL environments to date (I'm just starting my first project using it myself).

When PostgreSQL `UUID` values are read into .NET applications as `Guid` values, the string representation and default sort will still be different from the byte order.

How To Contribute
=================
Some missing pieces:
- It would be nice to have a custom `IComparer` method that sorts `Guid` values using the same order that we're expecting from our database platform (i.e., like `SqlGuid` is sorted in MSSQL, and in byte order for PostgreSql).
- More unit tests.
- Please keep all contributions compatible with .NET Core.
- Please use the same style (tabs, same-line opening braces, compact line spacing, etc.)
- Please keep project/solution files compatible with Visual Studio 2015 Community Edition and Visual Studio Code.

Security and Performance
========================
(1) It's a good idea to always use UTC date/time values for COMBs, so they remain highly sequential regardless of server locale or daylight savings, etc. This is even more important if the values will be generated on multiple machines that may have different time zone settings.

(2) It should go without saying, but using a COMB "leaks" the date/time to anyone knows how to decode it, so don't use one if this information (or even the relative order of their creation) should remain private.

(3) Don't use this with MySQL if you plan to store GUIDs as varchar(36) -- performance will be terrible.

(4) Test. Don't assume that a GUID key will be significantly slower for you than a 32-bit `int` field. Don't assume it will be roughly the same. The relative size of your tables (and especially of your indexed columns) compared to your primary key column will impact the overall database size and relative performance. I use COMB values frequently in moderate-sized databases without any performance issues, but YMMV.

(5) The best use for a COMB is where (a) you want the range and randomness of the GUID structure without index splits under load, and (b) any actual use of the embedded timestamp is rare (for example, for debugging purposes).

(6) When generating values, note that on Windows platforms, the Windows system timer only has a resolution of around 15ms. But even with a higher-resolution timer, multiple COMB values can have the same timestamp if you generate them quickly enough, so don't rely on the COMB alone if you need to sort your values in exactly the same order as they were created.

More Information
=================================
The original COMB article:
http://www.informit.com/articles/article.aspx?p=25862

Another implementation (not compatible):
http://www.siepman.nl/blog/post/2013/10/28/ID-Sequential-Guid-COMB-Vs-Int-Identity-using-Entity-Framework.aspx

License (MIT "Expat")
=====================
Copyright 2015-2016 Richard S. Tallent, II

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.