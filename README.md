<!-- markdownlint-disable-file MD034 -->

# RT.COMB

## Purpose

This small .NET Core library does two things:

1. generates "COMB" `Guid` values in C#; and,
2. extracts the `DateTime` value from an existing COMB `Guid`.

## Background

When GUIDs (`uniqueidentifier` values in MSSQL parlance, `UUID` in PostgreSQL) are part of a database index, and particularly when they are part of the clustered index, the randomness of new values can reduce performance when inserting new values.

In SQL Server 2005, Microsoft provided the `NEWSEQUENTIALID()` function to alleviate this issue, but despite the name, generated GUID values from that function are still not guaranteed to be sequential over time (rebooting impacts the sequence, for example), nor do multiple instances of MSSQL produce values that would be sequential in relationship to one another.

But back in 2002, in an article for InformIT, Jimmy Nilsson described the "COMB" technique, which replaces the portion of a GUID that is sorted first with a date/time value. This guarantees (within the precision of the system clock) that values will be sequential, even when the code runs on different machines. As a side benefit, the COMB's timestamp can be easily extracted, which can be useful from time to time if your table has no other field tracking the insertion date/time.

This library implements several modern variations, as well as the original technique.

## Simple Usage

A NuGet package is available, targeted for .NET Standard 2.1 (.NET 6.0+):

https://www.nuget.org/packages/RT.Comb/

Three static implementations are provided, each with a different strategy for generating the timestamp and inserting it into the GUID.

- `RT.Comb.Provider.Legacy`: The original technique. Only recommended if you need to support existing COMB values created using this technique.
- `RT.Comb.Provider.Sql`: This is the recommended technique for COMBs stored in Microsoft SQL Server.
- `RT.Comb.Provider.Postgre`: This is the recommended technique for COMBs stored in PostgreSQL.

Each of these has only **two public methods**:

- `Create()` returns a COMB GUID. You can _optionally_ pass your own baseline GUID and/or timestamp to embed.
- `GetTimestamp()` returns the timestamp embedded in a previously-created COMB GUID.

An example console application using the `Sql` version is provided in the `demo` folder showing a minimal-code use of both of these methods.

## Advanced Usage

If the default implementations don't work for you, you can roll your own, either changing up how the timestamp is determined, or changing how it is inserted into the GUID.

There are two core interfaces: `ICombProvider`, responsible for embedding and extracting timestamps (described above under Simple Usage), and `DateTimeStrategy`, responsible for encoding timestamp values to bytes and vice versa.

There are two included implementations of `ICombProvider`:

- `SqlCombProvider`: This creates and decodes COMBs in GUIDs that are compatible with the way Microsoft SQL Server sorts `uniqueidentifier` values -- _i.e._, starting at the 11th byte.
- `PostgreSqlCombProvider`: This creates and decodes COMBs in GUIDs that are compatible with the way PostgreSQL sorts `uuid` values -- _i.e._, starting with the first byte shown in string representations of a `Guid`.

Both take an `IDateTimeStrategy` argument in their constructor. Two strategies are included:

- `UnixDateTimeStrategy` encodes the DateTime using the millisecond version of the Unix epoch timestamp (recommended); and,
- `SqlDateTimeStrategy` encodes the DateTime using SQL Server's `datetime` data structure (only for compatibility with legacy COMB values).

You can implement either of these interfaces on your own, or both, to suit your needs.

## Database-side generation/decoding

This repository includes an `sql` folder containing scripts for two SQL Server 2016+ functions that implement `UnixDateTimeStrategy`:

- [NewComb](./sql/NewComb.sql): Generates a new COMB value; and
- [DateFromComb](./sql/DateFromComb.sql): Extracts the `DATETIME2` timestamp value from an existing COMB value.

The logic in these functions is fully compatible with the .NET code in this library.

_(I would be happy to accept a testable PR to add PostgreSql functions.)_

If you need similar functions for the `SqlDateTimeStrategy`, those are included as well as [NewCombLegacy](./sql/NewCombLegacy.sql) and [DateFromCombLegacy](./sql/DateFromCombLegacy.sql).

Both of the COMB-generation functions rely on a view, [vwNewGuid](./sql/vwNewGuid.sql), since `NEWID()` is non-deterministic and can't be called directly from within a function.

_If you need to support SQL Server versions prior to 2016, look at the commit history for this README file, it contains older versions of the code that support some older versions._

## Gory Details about UUIDs and GUIDs

Note: _(For a more comprehensive treatment, see Wikipedia.)_

Standard UUIDs/GUIDs are 128-bit (16-byte) values, wherein most of those bits hold a random value. When viewed as a string, the bytes are represented in hex in five groups, separated by hyphens:

```text
xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
version       ^
variant            ^
```

Structurally, this breaks down into one 32-bit unsigned integer (`Data1`), two 16-bit unsigned integers (`Data2`, `Data3`), and 8 bytes (`Data4`). The most significant nybble of Data3 (the 7th byte in string order, 8th in the internal bytes of `System.Guid`) is the GUID "version" number--generally 4 (0100b). Also, the most significant 2-3 bits of the first byte of Data4 is the "variant" value. For common GUIDs, the first two of these bits will be `10`, and the third is random. The remaining bits for a version 4 GUID are random.

The UUID standard (RFC 4122) specifies that the `Data1`, `Data2`, and `Data3` integers are stored and shown in _network byte order_ ("big-endian" or most significant byte first). However, while Microsoft's GUID implementation _shows and sorts_ the values as if they were in big-endian form, internally it stores the bytes for these values in the native form for the processor (which, on x86, means little-endian).

This means that the bytes you get from `Guid.ToString()` are actually stored in the GUID's internal byte array in the following order:

```text
33221100-5544-7766-8899-AABBCCDDEEFF
```

Npgsql, the standard .NET library for working with PostgreSQL databases, reads incoming bytes for these three fields using `GetInt32()` and `GetInt16()`, which assume the bytes are in little-endian form. This ensures that .NET will return the same _string_ as non-.NET tools that show PostgreSQL UUID values, but the bytes inside .NET and inside PostgreSQL are stored in a different order. Reference:

https://github.com/npgsql/npgsql/blob/4ef74fa78cffbb4b1fdac00601d0ee5bff5e242b/src/Npgsql/TypeHandlers/UuidHandler.cs

## IDateTimeStrategy

`IDateTimeStrategy` implementations are responsible for returning a byte array, most significant byte first, representing the timestamp. This byte order is independent of whatever swapping we might have to do to embed the value in a GUID at a certain index. They can return either 4 or 6 bytes, both built-in implementations use 6 bytes.

### UnixDateTimeStrategy

This implementation was inspired by work done on the Marten project, which uses a modified version of RT.Comb. This also returns 6 bytes, but as a 48-bit unsigned integer representing the number of _milliseconds_ since the UNIX epoch date (1970-01-01). Since this method is far more space-efficient than the MSSQL `datetime` structure, it will cover you well into the 85th Century. This is the recommended implementation.

### SqlDateTimeStrategy

This strategy returns bytes for a timestamp with _part_ of an MSSQL `datetime` value. The `datetime` type in MSSQL is an 8-byte structure. The first four bytes are a _signed_ integer representing the number of days since January 1, 1900. Negative values are permitted to go back to `1752-01-01` (for Reasons), and positive values are permitted through `9999-12-12`. The remaining four bytes represent the time as the number of unsigned 300ths of a second since midnight. Since a day is always 24 hours, this integer never uses more than 25 bits.

For the COMB, we use all four bytes of the time and two bytes of the date. This means our date has no sign bit and has a maximum value of 65,535, limiting our date range to January 1, 1900 through June 5, 2079 -- a very reasonable spread.

If you use this in conjunction with `SqlCombProvider`, your COMB values will be compatible with the original COMB article, and you can easily use plain T-SQL to encode and decode the timestamps without requiring .NET (the overhead for doing this in T-SQL is minimal):

## ICombProvider

Regardless which strategy is used for encoding the timestamp, we need to overwrite the portion of the GUID that is sorted _first_, so our GUIDs are sortable in date/time order and minimize index page splits. This differs by database platform.

MSSQL and `System.Data.SqlTypes.SqlGuid` sort _first_ by the _last 6_ `Data4` bytes, _left to right_, then the first two bytes of `Data4` (again, left to right), then `Data3`, `Data2`, and `Data1` _right to left_. This means for COMB purposes, we want to overwrite the last 6 bytes of `Data4` (byte index 10), left to right.

However, PostgreSQL and `System.Guid` sort GUIDs in the order shown as a string, which means for PostgreSQL COMB values, we want to overwrite the bytes that are _shown_ first from the left. Since `System.Guid` _shows_ bytes for `Data1`, `Data2`, and `Data3` in a different order than it _stores_ them internally, we have to account for this when overwriting those bytes. For example, our most significant byte inside a `Guid` will be at index 3, not index 0.

Here is a fiddler illustrating some of this:

https://dotnetfiddle.net/rW9vt7

### SqlCombProvider

As mentioned above, MSSQL sorts the _last_ 6 bytes first, left to right, so we plug our timestamp into the GUID structure starting at the 11th byte, most significant byte first:

```text
00112233-4455-6677-8899-AABBCCDDEEFF
xxxxxxxx xxxx 4xxx Vxxx MMMMMMMMMMMM  UnixDateTimeStrategy, milliseconds
xxxxxxxx xxxx 4xxx Vxxx DDDDTTTTTTTT  SqlDateTimeStrategy, days and 1/300s
4 = version
V = variant
x = random
```

### PostgreSqlCombProvider

For PostgreSQL, the _first_ bytes are sorted first, so those are the ones we want to overwrite.

```text
MMMMMMMM MMMM 4xxx Vxxx xxxxxxxxxxxx  UnixDateTimeStrategy, milliseconds
DDDDTTTT TTTT 4xxx Vxxx xxxxxxxxxxxx  SqlDateTimeStrategy, days and 1/300s
4 = version
V = variant
x = random
```

Recall that `System.Guid` stores its bytes for `Data1` and `Data2` in a different order than they are shown, so we have to reverse the bytes we're putting into those areas so they are stored and sorted in PostgreSQL in network-byte order.

**This is a breaking change for RT.Comb v.1.4.** In prior versions, I wasn't actually able to test on PostgreSQL and got this all wrong, along with misplacing where the version nybble was and doing unnecessary bit-gymnastics to avoid overwriting it. My error was kindly pointed out by Barry Hagan and has been fixed.

### BinaryCombProvider

Use this version for Oracle or for binary SqlLite columns. This places the timestamp at the binary beginning of the COMB.

### Comparing COMB to UUIDv7

Here is more information on UUIDv7: https://www.ietf.org/archive/id/draft-peabody-dispatch-new-uuid-format-04.html#v7

`PostgreSqlCombProvider` with the default `UnixDateTimeStrategy` (above) is compatible with UUIDv7.

However, while `SqlCombProvider` with the default `UnixDateTimeStrategy` also uses a 48-bit UNIX timestamp, the location of the timestamp bytes is different, based on how SQL Server sorts `UNIQUEIDENTIFIER` values.

### Note about entropy

The default implementations overwrite 48 bits of random data with the timestamp, and another 6 bits are also pre-determined (the version and variant). This still leaves 74 random bits per unit of time (1/300th of a second for SqlDateTimeStrategy, 1/1000th of a second for UnixDateTimeStrategy). This provides well beyond a reasonable amount of protection against collision.

## TimestampProvider / GuidProvider

By default, SqlCombProvider and PostgreSqlCombProvider use `DateTime.UtcNow` when a timestamp argument is not provided for `Create()`. If you want the convenience of using `Create` with fewer arguments but need to choose the timestamp another way, you can set the `TimestampProvider` delegate to a parameter-less function that returns a `DateTime` value. Another delegate, `GuidProvider`, provides the same functionality for overriding how the base GUID is created.

## UtcNoRepeatTimestampProvider

The DateTime strategies described above are limited to 1-3ms resolution, which means if you create many COMB values per second, there is a chance you'll create two with the same timestamp value.

This won't result in a database collision--the remaining random bits in the GUID protect you there. But COMBs with exactly the same timestamp value aren't _guaranteed_ to sort in order of insertion because once the timestamp bytes are sorted, the sort order will rely on the random bytes after that. **This is expected behavior**. As with any timestamp-based field, COMBs are not guaranteed to be sequential once you're inserting records faster than the stored clock resolution. Also, on Windows platforms, the system timer only has a resolution of 15.625ms, which amplifies this problem. So, in general, don't rely on COMBs to have values that sort in _exactly_ the same order as they were inserted.

If your sort order must be guaranteed, I've come up with a workaround -- a `TimestampProvider` delegate called `UtcNoRepeatTimestampProvider.GetTimestamp`. This method checks to ensure that the current time is at least `IncrementMs` milliseconds more recent than the previous value it generated. If not, it returns the previous value plus `IncrementMs` instead. Either way, it then keeps track of what it returned for the next round. This checking is thread-safe. By default, `IncrementMs` is set to 4ms, which is sufficient to ensure that `SqlDateTimeStrategy` timestamp values won't collide (which has a ~3ms resolution). If you're using `UnixDateTimeStrategy`, you can optionally set this to a lower value (such as 1ms) instead. The table below shows some examples values you might get from `DateTime.UtcNow` in a tight loop vs. what `UtcNoRepeatTimestampProvider` would return:

```text
02:08:50.613    02:08:50.613
02:08:50.613    02:08:50.617
02:08:50.613    02:08:50.621
02:08:50.617    02:08:50.625
02:08:50.617    02:08:50.629
02:08:50.617    02:08:50.632
```

Note that you're trading a "time slip" of a few milliseconds during high insert rates for a guarantee that the `UtcNoRepeatTimestampProvider` won't repeat its timestamp, so COMBs will always sort exactly in insert order. This is fine if your transaction rate just has occasional bumps, but if you're constantly writing thousands of records per second, the time slip could accumulate into something real, especially with the 4ms default increment.

To use this workaround, you'll need to create your own ICombProvider instance rather than using the built-in static instances in `RT.Comb.Provider`, and pass this delegate in the provider constructor. You can find an example of this in the test suite.

## How To Contribute

Some missing pieces:

- More unit tests.
- Please keep all contributions compatible with .NET Core.
- Please use the same style (tabs, same-line opening braces, compact line spacing, etc.)
- Please keep project/solution files compatible with Visual Studio Code.

## Security and Performance

(1) It's a good idea to always use UTC date/time values for COMBs, so they remain highly sequential regardless of server locale or daylight savings, etc. This is even more important if the values will be generated on multiple machines that may have different time zone settings.

(2) It should go without saying, but using a COMB "leaks" the date/time to anyone knows how to decode it, so don't use one if this information (or even the relative order of their creation) should remain private.

(3) Don't use this with MySQL if you plan to store GUIDs as varchar(36) -- performance will be terrible.

(4) Test. Don't assume that a GUID key will be significantly slower for you than a 32-bit `int` field. Don't assume it will be roughly the same. The relative size of your tables (and especially of your indexed columns) compared to your primary key column will impact the overall database size and relative performance. I use COMB values frequently in moderate-sized databases without any performance issues, but YMMV.

(5) The best use for a COMB is where (a) you want the range and randomness of the GUID structure without index splits under load, and (b) any actual use of the embedded timestamp is incidental (for example, for debugging purposes).

(6) As described under `UtcNoRepeatTimestampProvider`, timestamps are only as precise as the timer resolution, so unless you use the aforementioned functionality to work around this, COMBs generated within the same timestamp period are not guaranteed to sort in the order of generation.

## More Information

The original COMB article:
http://www.informit.com/articles/article.aspx?p=25862

Another implementation (not compatible):
http://www.siepman.nl/blog/post/2013/10/28/ID-Sequential-Guid-COMB-Vs-Int-Identity-using-Entity-Framework.aspx

## License (MIT "Expat")

Copyright 2015-2025 Richard S. Tallent, II

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
