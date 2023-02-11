# RT.Comb ASP.NET Core Integration

This package is a wrapper to make it easy to use RT.Comb in ASP.NET Core applications.

## Usage

Install the package in the ASP.NET Core application:

```shell
dotnet add package RT.Comb.AspNetCore
```

Then simply choose one of the extensions and add it inside `ConfigureServices` (`Startup.cs`) like:

```C#
services.AddSqlCombGuidWithSqlDateTime();
```

## Extensions available

The main package `RT.Comb` offers two implementations of `ICombProvider`: `SqlCombProvider` and `PostgreSqlCombProvider`. This package offers extensions to configure both providers, with either the `SqlDateTimeStrategy` or `UnixDateTimeStrategy`.

### SqlCombProvider

`services.AddSqlCombGuidWithSqlDateTime`:

Registers a `SqlCombProvider` using the `SqlDateTimeStrategy` and the default Timestamp and Guid strategies (`DateTime.Utc` and `Guid.NewGuid()`)

`services.AddSqlCombGuidWithUnixDateTime`:

Registers a `SqlCombProvider` using the `UnixDateTimeStrategy` and the default Timestamp and Guid strategies (`DateTime.Utc` and `Guid.NewGuid()`)

### PostgreSqlCombProvider

`services.AddPostgreSqlCombGuidWithSqlDateTime`:

Registers a `PostgreSqlCombProvider` using the `SqlDateTimeStrategy` and the default Timestamp and Guid strategies (`DateTime.Utc` and `Guid.NewGuid()`)

`services.AddPostgreSqlCombGuidWithUnixDateTime`:

Registers a `PostgreSqlCombProvider` using the `UnixDateTimeStrategy` and the default Timestamp and Guid strategies (`DateTime.Utc` and `Guid.NewGuid()`)

### Override default providers

All extensions methods above register the `ICombProvider` with the defaults Timestamp and Guid providers, but those can be customized by providing values to the optional parameters:

```C#

// Defined somewhere in your app..
public class CustomGuidProvider {
    // some custom logic here..
    public Guid GetGuid() => Guid.NewGuid();
}

public class CustomTimestampProvider {
    // always create with +2 seconds.. (example custom logic)
    public DateTime GetTimestamp() => DateTime.UtcNow.AddSeconds(2);
}

// In ConfigureServices:

// create the custom providers
var myGuidProvider = new CustomGuidProvider();
var myTimestampProvider = new CustomTimestampProvider();

// Add the ICombProvider with the custom providers
services.AddPostgreSqlCombGuidWithSqlDateTime(
    myTimestampProvider.GetTimestamp, myGuidProvider.GetGuid);

```

The `RT.Comb` library comes already with a handy timestamp provider: [UtcNoRepeatTimestampProvider](https://github.com/richardtallent/RT.Comb#utcnorepeattimestampprovider) (Click to read more about it)
