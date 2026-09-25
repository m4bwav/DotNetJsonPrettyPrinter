DotNetJsonPrettyPrinter
=======================

Json Pretty Printer/Beautifier Library For .Net

[![NuGet](https://img.shields.io/nuget/v/JsonPrettyPrinter.svg)](https://www.nuget.org/packages/JsonPrettyPrinter/) [![CI](https://github.com/m4bwav/DotNetJsonPrettyPrinter/actions/workflows/ci.yml/badge.svg)](https://github.com/m4bwav/DotNetJsonPrettyPrinter/actions/workflows/ci.yml)

```
dotnet add package JsonPrettyPrinter
```

Targets `netstandard2.0` (any .NET Framework 4.6.2+, .NET Core, Mono, Unity) and `net10.0` (trim and native AOT compatible). Try it live at https://www.markdavidrogers.com/tools or through the site's MCP server tool `prettify_json`.

Background article: https://www.markdavidrogers.com/json-pretty-printerbeautifier-library-for-net/  
NuGet package: https://www.nuget.org/packages/JsonPrettyPrinter/  
Changes: [CHANGELOG.md](CHANGELOG.md)

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

Every value goes on its own line, indented four spaces per level, with one space after each colon. Empty objects and arrays print as `{}` and `[]`. Lines end with `Environment.NewLine`, so the output differs between Windows and other systems unless you set `NewLine` (below).

## Options

```csharp
var options = new JsonPrettyPrintOptions { IndentSize = 2, NewLine = "\n" }; // or UseTabs = true
var pretty = json.PrettyPrintJson(options);

// The same printer can be reused (it is not thread-safe; PrettyPrintJson() keeps one per thread for you).
var printer = new JsonPrettyPrinter(options);
printer.PrettyPrint(json, Console.Out);          // straight into any TextWriter
printer.PrettyPrint(json.AsSpan());              // ReadOnlySpan<char> input
```

Defaults are `IndentSize = 4`, `UseTabs = false` and `NewLine = Environment.NewLine`. In 3.0 the default `NewLine` becomes `"\n"`.

## Not a validator

The printer tracks strings, escapes and bracket nesting and copies everything else through as it finds it. Trailing commas, comments, single-quoted strings or bare words come out indented rather than rejected. The only errors it raises are `ArgumentNullException` for null input and `FormatException` (with the character index) for a closing bracket that has nothing to close or closes the wrong kind of scope. A document that ends before its last bracket is printed as far as it goes.

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

`ToJSON()` (upper case) still works and is removed in 3.0. The helpers use `System.Text.Json`, which the `netstandard2.0` build references as a package; the `net10.0` build uses the one in the framework.

## Building and releasing

```
dotnet test
dotnet pack JsonPrettyPrinterPlus -c Release -o artifacts
dotnet run -c Release --project benchmarks/JsonPrettyPrinterPlus.Benchmarks   # optional
```

CI (`.github/workflows/ci.yml`) builds, tests (net10.0 on Ubuntu and Windows, net48 on Windows), checks formatting, collects coverage and packs on every push. To publish: add the version to `CHANGELOG.md`, bump `<Version>` in `JsonPrettyPrinterPlus.csproj`, tag the commit `v<version>` and push the tag. The `publish` job refuses a tag that does not match the packed version, signs in to nuget.org with Trusted Publishing (GitHub OIDC, no stored API key; a `NUGET_USER` secret holding the nuget.org profile name lives in the `nuget` environment), pushes the package and creates a GitHub Release with the `.nupkg` and `.snupkg` attached.

## License

MIT, see `LICENSE`.
