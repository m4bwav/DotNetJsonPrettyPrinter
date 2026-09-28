DotNetJsonPrettyPrinter
=======================

Json Pretty Printer/Beautifier Library For .Net

[![NuGet](https://img.shields.io/nuget/v/JsonPrettyPrinter.svg)](https://www.nuget.org/packages/JsonPrettyPrinter/) [![CI](https://github.com/m4bwav/DotNetJsonPrettyPrinter/actions/workflows/ci.yml/badge.svg)](https://github.com/m4bwav/DotNetJsonPrettyPrinter/actions/workflows/ci.yml) [![Downloads](https://img.shields.io/nuget/dt/JsonPrettyPrinter.svg)](https://www.nuget.org/packages/JsonPrettyPrinter/)

```
dotnet add package JsonPrettyPrinter
```

Targets `netstandard2.0` (any .NET Framework 4.6.2+, .NET Core, Mono, Unity) and `net10.0` (trim and native AOT compatible).

Background article: https://www.markdavidrogers.com/json-pretty-printerbeautifier-library-for-net/  
NuGet package: https://www.nuget.org/packages/JsonPrettyPrinter/  
Changes: [CHANGELOG.md](CHANGELOG.md)  
Documentation, recipes and the upgrade notes: https://github.com/m4bwav/DotNetJsonPrettyPrinter/wiki

This library is a simple and light weight json pretty printer. There are few other .net json pretty printers, but they are usually heavier and focus on some other aspect of javascript or have only been described in article format.

You can use the pretty printer object or just the extension methods provided. Json and beautifying extension methods are included for ease of use.

Example:
```csharp
using JsonPrettyPrinterPlus;

"{\"Lorem\":\"ipsum\",\"dolor\":{ \"sit\":\"amet\"},\"consectetur\":{\"adipisicing\":{\"sed\":\"do\"}},\"empty\":{}}".PrettyPrintJson()
```
becomes:

```
{
    "Lorem": "ipsum",
    "dolor": {
        "sit": "amet"
    },
    "consectetur": {
        "adipisicing": {
            "sed": "do"
        }
    },
    "empty": {}
}
```

Every value goes on its own line, indented four spaces per level, with one space after each colon. Empty objects and arrays print as `{}` and `[]`. Lines end with `"\n"` on every OS (2.x used `Environment.NewLine`; set `NewLine` below to get that back).

## Options

```csharp
var options = new JsonPrettyPrintOptions { IndentSize = 2, NewLine = Environment.NewLine }; // or UseTabs = true
var pretty = json.PrettyPrintJson(options);

// The same printer can be reused (it is not thread-safe; PrettyPrintJson() keeps one per thread for you).
var printer = new JsonPrettyPrinter(options);
printer.PrettyPrint(json, Console.Out);          // straight into any TextWriter
printer.PrettyPrint(json.AsSpan());              // ReadOnlySpan<char> input
```

Defaults are `IndentSize = 4`, `UseTabs = false` and `NewLine = "\n"`.

## Not a validator

The printer tracks strings, escapes and bracket nesting and copies everything else through as it finds it. Trailing commas, single-quoted strings and bare words come out indented rather than rejected. Comments do not survive intact: spaces inside a block comment are dropped (`/* keep me */` becomes `/*keepme*/`), a quote or apostrophe inside one opens a string that swallows the rest of the document, and a `//` comment is worse: the printer drops the spaces inside it and the line break that ends it, so it runs into what follows (`// note`, a line break, then `"b":2` prints as `//note"b": 2`). Strip comments before printing a document that has them. For input it raises only `ArgumentNullException` for null and `FormatException` (with the character index) for a closing bracket that has nothing to close or closes the wrong kind of scope; `JsonPrettyPrintOptions` refuses a negative `IndentSize` with `ArgumentOutOfRangeException`. A document that ends before its last bracket is printed as far as it goes.

## Serialising

Serialising an object and pretty printing it in one go:

```csharp
using JsonPrettyPrinterPlus.JsonSerialization;

var json = new { Name = "Mark", Tags = new[] { "a", "b" } }.ToJson(prettyPrint: true);
var back = json.DeserializeFromJson<MyType>();

// With your own System.Text.Json options, or source-generated metadata for trimmed and AOT apps:
var camel = thing.ToJson(new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }, prettyPrint: true);
var aot = thing.ToJson(MyContext.Default.MyType, prettyPrint: true);
```

The helpers use `System.Text.Json`, which the `netstandard2.0` build references as a package (with `System.Memory`, its only dependencies); the `net10.0` build uses the one in the framework.

On .NET Framework, which loads the `netstandard2.0` build, System.Text.Json writes doubles with up to 17 significant digits: `0.1` serialises as `0.10000000000000001` and `-0.0` as `0`. On .NET 10 they come out as `0.1` and `-0`.

## Upgrading from 1.x

Most installs are still 1.0.1.1 (2014, .NET Framework 3.5). Running the same cases against 1.0.1.1 and 3.x shows what changes:

- Printing: empty objects and arrays print as `{}` and `[]`; an escaped backslash before a closing quote no longer stops the formatting of the rest of the document; a stray closing bracket throws `FormatException` with its index instead of `InvalidOperationException`; null throws `ArgumentNullException`; lines end with `"\n"`.
- `ToJSON` is now `ToJson`, over System.Text.Json: dates are ISO 8601 strings instead of `/Date(...)/`, public fields are no longer written, NaN and infinities throw, and characters outside ASCII are escaped.
- `DeserializeFromJson` is strict and case-sensitive: a lower-case property name no longer fills the property, and single quotes, unquoted keys and numbers written as strings throw `JsonException`. Pass `new JsonSerializerOptions { PropertyNameCaseInsensitive = true }` for the old name matching.
- On .NET Core and .NET 5 or later, 1.x's `ToJSON` and `DeserializeFromJson` never worked (they need System.Web.Extensions); only its printer did.

The full list, case by case, is on the wiki's [Versions and upgrading](https://github.com/m4bwav/DotNetJsonPrettyPrinter/wiki/Versions-and-Upgrading) page.

## Upgrading from 2.x to 3.0

- Output lines end with `"\n"`. If you compared against `Environment.NewLine`, pass `new JsonPrettyPrintOptions { NewLine = Environment.NewLine }`.
- `ToJSON()` is now `ToJson()`.
- The `JsonPrettyPrinterInternals` namespace (strategy classes, `JsonPPStrategyContext`, `PPScopeState`, `SpacesPerIndent`) is gone; use `JsonPrettyPrintOptions`. If you customised a strategy, open an issue describing what it did.

## Building and releasing

```
dotnet test
dotnet pack JsonPrettyPrinterPlus -c Release -o artifacts
dotnet run -c Release --project benchmarks/JsonPrettyPrinterPlus.Benchmarks   # optional
```

CI (`.github/workflows/ci.yml`) restores in locked mode, checks formatting, builds, audits the packages, runs the unit tests and the golden replay of 3.0.1's recorded answers (net10.0 on Ubuntu and Windows, net48 on Windows), packs, checks the package's contents and runs fresh consumers of it. To publish: add the version to `CHANGELOG.md`, set `<Version>` in `JsonPrettyPrinterPlus.csproj`, merge, wait for CI on master, then tag the commit `v<version>` and push the tag. `release.yml` refuses a tag that does not match the version or is not on master, builds and tests on Linux and Windows, attests the package, and waits for the maintainer's approval of the `nuget` environment before it pushes through Trusted Publishing (GitHub OIDC, no stored API key) and creates the GitHub Release. `verify-published.yml` then checks the version from nuget.org on Linux, macOS and Windows.

## License

MIT, see `LICENSE`.
