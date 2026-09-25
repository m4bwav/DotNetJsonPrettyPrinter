DotNetJsonPrettyPrinter
=======================

Json Pretty Printer/Beautifier Library For .Net

[![NuGet](https://img.shields.io/nuget/v/JsonPrettyPrinter.svg)](https://www.nuget.org/packages/JsonPrettyPrinter/) [![CI](https://github.com/m4bwav/DotNetJsonPrettyPrinter/actions/workflows/ci.yml/badge.svg)](https://github.com/m4bwav/DotNetJsonPrettyPrinter/actions/workflows/ci.yml)

```
dotnet add package JsonPrettyPrinter
```

Targets `netstandard2.0` (any .NET Framework 4.6.2+, .NET Core, Mono, Unity) and `net10.0`. Try it live at https://www.markdavidrogers.com/tools or through the site's MCP server tool `prettify_json`.

Background article: https://www.markdavidrogers.com/json-pretty-printerbeautifier-library-for-net/  
NuGet package: https://www.nuget.org/packages/JsonPrettyPrinter/

This library is a simple and light weight json pretty printer. There are few other .net json pretty printers, but they are usually heavier and focus on some other aspect of javascript or have only been described in article format.

You can use the pretty printer object or just the extension methods provided. Json and beautifying extension methods are included for ease of use.

Example:
```csharp
using JsonPrettyPrinterPlus;

"{\"Lorem\":\"ipsum\",\"dolor\":{ \"sit\":\"amet\"},\"consectetur\":{\"adipisicing\":{\"sed\":\"do\"}}}".PrettyPrintJson()
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
    }
}
```

Serialising an object and pretty printing it in one go:

```csharp
using JsonPrettyPrinterPlus.JsonSerialization;

var json = new { Name = "Mark", Tags = new[] { "a", "b" } }.ToJSON(prettyPrint: true);
var back = json.DeserializeFromJson<MyType>();
```

## What changed in 2.0.0

- Rebuilt with the current .NET SDK for `netstandard2.0` and `net10.0`; the `net35` build is gone.
- `PrettyPrintJson()` output is unchanged, except that an empty object now prints as an opening and a closing line with nothing between (it used to leave an indented blank line).
- `ToJSON()` and `DeserializeFromJson()` use `System.Text.Json` instead of `JavaScriptSerializer` (which does not exist outside .NET Framework). Dates therefore serialise as ISO 8601 (`2010-01-01T00:00:00`) instead of `\/Date(1262325600000)\/`, and quotes inside strings are escaped as `"`. If you need the old serialiser, stay on 1.0.1.1.
- SourceLink, deterministic build, a symbols package and this README are in the package.

## Building and releasing

```
dotnet test
dotnet pack JsonPrettyPrinterPlus -c Release -o artifacts
```

CI (`.github/workflows/ci.yml`) builds, tests and packs on every push. To publish: bump `<Version>` in `JsonPrettyPrinterPlus.csproj`, tag the commit `v<version>` and push the tag; the `publish` job signs in to nuget.org with Trusted Publishing (GitHub OIDC, no stored API key; a `NUGET_USER` secret holding the nuget.org profile name lives in the `nuget` environment) and pushes the package.

## License

MIT, see `LICENSE`.
