
Purpose
=======
Invented by Jimmy Nilsson and first described in an article for InformIT in 2002, a COMB is a GUID with an embedded date/time value, making the values sequential over time.

This library is designed to create "COMB" Guid values in C#, and to be able to extract the Date/Time value from an existing COMB value.

Background
==========
When GUIDs (`uniqueidentifier` values in MSSQL parlance, `UUID` in PostgreSql) are part of a database index, the randomness of the values can result in reduced performance, as insertions of new data result in page splits. In SQL Server 2005, Microsoft provided the `NEWSEQUENTIALID()` function to alleviate this issue, but despite the name, generated GUID values from that function are still not guaranteed to be sequential over time.

The COMB method, however, takes advantage of the database's sort ordering for GUID values and replaces the bytes that are sorted first with a date/time value, so values generated at the same time will always be generated in order or very close to it, regardless of regardless of server reboots or values generated on different machines (to the degree, of course, that the system clocks are synchronized).

As a side benefit, the COMB's sequential portion has the semantic value of being the date and time of insert, which can be useful from time to time.

While this does not alleviate all potential worries when deciding to use GUID fields in a table (they are still rather large, and can be unfriendly if exposed to end users), it does make them more palatable when they are called for.

GUID Data Structure
===================
*(For a more comprehensive treatment, see Wikipedia.)*

Understanding how GUIDs are generated, stored, and sorted is key to proper implementation of a COMB value.

Standard GUIDs are 128-bit (16-byte) values, wherein most of those bits hold a random value. When viewed as a string, the bytes are represented in hex in five groups, separated by hyphens:

    xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
    00112233-4455-6677-8899-AABBCCDDEEFF
    version  ^
    variant            ^

Each character represents a nybble, and each byte is shown with the most significant nybble first, as usual (i.e., "0F" is 16, "F0" is 128).

The most significant 2-3 bits of nybble 17 are not random -- they store a "variant" value. For standard GUIDs, the first two bits are "10", the other two are random, and the 5th nybble stores the "version" number (usually "4").

PostgreSQL store and sort (and tools like pgAdmin show) `UUID` values as if they were a 16-byte array, left to right, i.e.:

    0123456789ABCDEF

This conforms with the RFC 4122 standards for UUIDs.

However, Microsoft.NET's `Guid` data type (and MSSQL's `uniqueidentifier` type), when converted to a string, come out with the bytes out of order in the string:

	3210-54-76-89-ABCDEF

This is because Microsoft's GUID (both in .NET and MSSQL) is a structure made up of a DWORD (32-bit), two WORDs (16-bit), and an 8-byte array. The x86 platform uses Little-endian form for the DWORD and WORD values. So if the first 4 bytes of a GUID are "FE 00 00 00", these are converted into a DWORD value of 254. When that DWORD is then converted to a hex string, it comes out as "000000FE", the reverse of the byte order.

Microsoft.NET sorts `Guid` bytes in the order they are shown as a string. In other words, it sorts first by the DWORD *value*, then by the two WORD values, and finally by the remaining 8 bytes (left to right). Here is the sort order of the *bytes*:

    3210547689ABCDEF

However, MSSQL (and .NET's `System.Data.SqlTypes.SqlGuid` type) use a much different sort order. Here is the sort order of the bytes:

    ABCDEF8967450123

Fiddler illustrating this: https://dotnetfiddle.net/X5Sk3C

COMB Data Structure
===================

In a COMB value, we replace six bytes with part of an MSSQL `datetime` value.

The `datetime` type in MSSQL is an 8-byte structure. The first four bytes are a *signed* integer representing the number of days since January 1, 1900. Negative values are permitted to go back to 1752-01-01 (for reasons), and positive values are permitted through 9999-12-12.

The remaining four bytes represent the time as the number of 300ths of a second since midnight. Since a day is always 24 hours, this integer never exceeds 25 bits of data, and since only whole numbers are supported, the sign is unused, so you can think of this as unsigned.

For the purposes of a COMB, we use all four bytes of the time, but only two bytes of the date portion. This means our date has no sign bit and has a maximum value of 65535. This limits our date range to January 1, 1900 through June 6, 2979. This will do nicely for a lifetime of timestamps.

We want to overwrite the bytes that will be sorted *first*, so our GUIDs are always in date/time order. This is why understanding the byte order and sorting is so important.

For MSSQL, the last six bytes are sorted first, left to right, so we overwrite our date/time value as follows:

    00112233-4455-6677-8899-AABBCCDDEEFF
    xxxxxxxx 4xxx xxxx Vxxx DDDD0TTTTTTT
                       		554433221100
    random-ish				date    time
	V = b100x

(Note that I've marked the most significant nybble of the time above as "0" rather than "T", since its value will always be 0.)

For PostgreSql, the *first* six bytes are sorted first, so those are the ones we want to overwrite. But there's a problem -- the most significant four bits of the 5th byte are supposed to store the UUID version number (4), not our time.

Here, we can take advantage of the fact that our time value only uses 28 of its 32 bits. We shift the first two bytes of the time left by four bits, put a "4" in the 5th nybble. We actually only use 25 of the bits, not the full 28, so we won't come anywhere near the sign bit.

    DDDDTTTT-4TTT-xxxx-Vxxx-xxxxxxxxxxxx
    00112334  455
    date      time            random-ish
	V = b100x

Note above that byte position 2 is only mentioned once -- we only represent its least significant nybble.

When PostgreSQL `UUID` values are read into .NET applications as `Guid` values, the string representation and default sort will still be different from the byte order.

Implementation/Usage
====================
After playing with both classes and structs, I decided that the best way to implement both of these variations was through static utility classes that "encode" and "decode" the DateTime in a given GUID.

Helper methods are also provided to generate sample T-SQL to create COMB values in MSSQL. These were only included for testing purposes.

Some minor features require C# 6. If this is a major stumbling block, I can move back to code that can target earlier versions of .NET.

When generating values, note that on Windows platforms, while a COMB's time resolution is around 3md, the Windows system timer only has a resolution of around 15ms.

Extension methods seemed like they would have the potential to create confusion since they return a new Guid rather than modifying "this," so I didn't go down that road.

How To Contribute
=================
Some missing pieces:

1. I'm having trouble adding an Xunit project to create unit tests, so I could use some help on that front.
2. The code currently only works with Guid values, I'd like to have it work with `SqlGuid` values as well.
3. It would be nice to have a custom `IComparer` method that sorts `Guid` values the same way as `SqlGuid`, and another that would compare them in byte order for COMBs generated for PostgreSQL.
4. It might be nice to have an alternate implementation that uses `GetSystemTimeAsFileTime()` to provide time resolution that is equivalent with MSSQL's `DATETIME2` type and `SYSUTCDATETIME` function. Perhaps call it COMB2?

I'm using Visual Studio 2015 Community Edition (over VirtualBox on my home Mac), targeted to stable .NET versions, so please keep contributions compatible with that configuration. Also, I do use tabs and I like compact code (minimal line breaks). I know I'm in the minority on this coding style, but please don't send a pull request just to "fix" my formatting.

Security and Performance
========================
(1) It's a good idea to always use UTC date/time values for COMBs, so they remain highly sequential regardless of server locale or daylight savings, etc. This is even more important if the values will be generated on multiple machines that may have different time zone settings.

(2) It should go without saying, but using a COMB "leaks" the date/time to anyone knows how to decode it, so don't use one if this information (or, more generally, the relative order of their creation) should remain private.

(3) While this is not my area of expertise, because of the increased predictability of a COMB value, it may be a poor choice to use as a cryptographic seed.

(4) Don't use this with MySQL if you plan to store GUIDs as varchar(36) -- performance will be terrible.

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

