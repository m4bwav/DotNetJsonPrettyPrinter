# Changelog

All notable changes to the `JsonPrettyPrinter` package. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project uses [Semantic Versioning](https://semver.org/).

## [Unreleased]

## [3.0.2-beta.1] - 2026-09-28

No change to the library. Every answer the published 3.0.1 gives is now recorded (1,174 cases, on .NET Framework 4.8 and on .NET 10) and checked on every build, so the package behaves exactly as 3.0.1. This release corrects the README that nuget.org shows and is the first through the new release workflow.

### Changed

- README: a `//` comment runs into what follows it (the old text said comments pass through); the "try it live" sentence is gone, because that site does not format with this package; a note on how .NET Framework writes doubles; an "Upgrading from 1.x" section; a downloads badge; a link to the wiki.
- Package validation compares the API with 3.0.1.
- Releases go through `release.yml`: the tag must match the version and sit on master, the tests run on Linux and Windows, the package gets a build provenance attestation, and the push waits for the maintainer's approval. It replaces the publish job in `ci.yml`. `verify-published.yml` checks each release from nuget.org on three operating systems.

### Upgrading from 1.0.1.1, measured

About nine in ten downloads of this package are 1.0.1 or 1.0.1.1, and the 2.0.0 entry below described the change to System.Text.Json in two sentences. The cases recorded for 3.0.1 were run against 1.0.1.1 as far as its API allows (446 of them; 1.0.0 and 1.0.1 give the same answers). On .NET Framework, 273 answers are identical, 60 differ only in line endings and 110 differ:

- Printing: `{}` and `[]` instead of an opening line, a blank indented line and a closing line; an escaped backslash before a closing quote no longer leaves the rest of the document unformatted; a stray or surplus closing bracket throws `FormatException` naming the index (1.x: `InvalidOperationException` "Stack empty."); a closing bracket of the wrong kind throws (1.x printed it); unclosed documents and trailing commas no longer leave lines of indentation only; null throws `ArgumentNullException` (1.x: `NullReferenceException`); a reused printer no longer carries string or nesting state into the next document; lines end with `"\n"` (1.x: `Environment.NewLine`). Comments, single quotes and bare words print as they did.
- `ToJSON` became `ToJson` over System.Text.Json: dates as ISO 8601 (1.x: `/Date(milliseconds)/`, and a `DateTime` of unspecified kind taken as local time), `DateTimeOffset` with its offset, public fields no longer written, `TimeSpan` and `Version` as strings, byte arrays as base64, dictionaries with integer keys supported, NaN and infinities throw (1.x wrote them, which is not valid JSON), `<`, `>`, `&`, `'` and `"` escaped with upper-case hex, characters outside ASCII escaped, a lone surrogate replaced by U+FFFD, a reference cycle throws `JsonException`, and on .NET Framework doubles with 17 significant digits.
- `DeserializeFromJson` is strict and case-sensitive: 1.x accepted lower-case property names, single quotes, unquoted keys, numbers written as strings and strings written as numbers, `1e3` for an integer and enum names, and it turned ISO dates into local time. 3.x throws `JsonException` for each of those, except that a lower-case property name is ignored without an error. An `object` target gives a `JsonElement` (1.x: dictionaries and arrays), and the `/Date(...)/` strings 1.x wrote cannot be read back.
- On .NET Core and .NET 5 or later, 1.x's `ToJSON` and `DeserializeFromJson` throw `FileNotFoundException` for System.Web.Extensions on every call; only its printer works there.

From 2.1.1 only the line endings change (see 3.0.0). The recordings and the comparison reports are in `tests/Golden/upgrade/`.

## [3.0.1] - 2026-09-25

Identical to 3.0.0, whose tag pointed at a commit with a broken CI publish step and so never reached nuget.org. The 2.1.x line was published as 2.1.1 for the same reason.

## [3.0.0] - 2026-09-25 (never published)

Breaking release. Output for well-formed input is unchanged apart from the line terminator.

### Changed

- `JsonPrettyPrintOptions.NewLine` defaults to `"\n"` instead of `Environment.NewLine`, so output is the same on every OS. Pass `new JsonPrettyPrintOptions { NewLine = Environment.NewLine }` for the 2.x behaviour.
- The strategy machinery is gone from the public API and replaced by one internal engine with a `switch` per character: `JsonPPStrategyContext`, `PPScopeState`, `ICharacterStrategy`, the ten strategy classes, the `JsonPrettyPrinter(JsonPPStrategyContext)` constructor and the fields `IsProcessingVariableAssignment` and `SpacesPerIndent` no longer exist. Use `JsonPrettyPrintOptions` for indentation. `JsonPrettyPrinter` is now `sealed`.
- The `netstandard2.0` build keeps its `System.Text.Json` dependency for the serialisation helpers. This is deliberate and documented in the README; the alternative (a second package) was judged not worth it for a library this small.

### Removed

- `ToJSON()`; use `ToJson()`, which has been there since 2.1.0.

### Performance

- 1 MB document: 11.1 ms and 10.8 MB allocated in 2.1.0, 4.5 ms and 10.4 MB in 3.0.0 (same benchmark project and machine). The switch replaces a dictionary lookup and an interface call per character.

## [2.1.1] - 2026-09-25

Identical to 2.1.0, whose tag was never published; see 3.0.1.

## [2.1.0] - 2026-09-25 (never published)

Output is unchanged for well-formed input, except that empty objects and arrays now print on one line.

### Fixed

- An escaped backslash before a closing quote (`"x\\"`) was read as an escaped quote, so the string never ended and the rest of the document was copied through unformatted. Escapes are now consumed one character at a time, so `\\`, `\"`, `\n` and `\uXXXX` all behave.
- Empty objects and arrays print as `{}` and `[]` at any nesting depth. `{}` used to print as an opening and a closing line, `{ }` (with whitespace) as three lines, and a nested `{}` put its closing brace at column 0.
- `PrettyPrint(null)` throws `ArgumentNullException` instead of `NullReferenceException`.
- A closing bracket with nothing to close, or the wrong kind of closing bracket, throws `FormatException` naming the character index instead of `InvalidOperationException` from the scope stack.
- A reused `JsonPrettyPrinter` no longer inherits string or scope state from an earlier malformed document.

### Added

- `JsonPrettyPrintOptions` (`IndentSize`, `UseTabs`, `NewLine`), `new JsonPrettyPrinter(options)` and `PrettyPrintJson(this string, options)`. The defaults reproduce the 2.0 output: four spaces and `Environment.NewLine`.
- `PrettyPrint(ReadOnlySpan<char>)` and `PrettyPrint(string | ReadOnlySpan<char>, TextWriter)` overloads, so large documents can be formatted straight into a stream.
- `ToJson()` beside `ToJSON()`, `JsonSerializerOptions` overloads of `ToJson` and `DeserializeFromJson`, and `JsonTypeInfo<T>` overloads that are safe for trimming and native AOT. The reflection-based overloads are annotated with `RequiresUnreferencedCode` and `RequiresDynamicCode` on net10.0.
- `IsTrimmable` and `IsAotCompatible` on the net10.0 build; nullable reference type annotations on the whole public surface; XML documentation on every public member.
- Package icon, `EnablePackageValidation` against 2.0.0, and a BenchmarkDotNet project under `benchmarks/`.

### Changed

- The printer no longer copies the input into a `StringBuilder`, allocates a strategy per ordinary character, or backtracks the output to remove indentation; it writes to a `TextWriter` and defers each line break until the next character. On a 1 MB document: 16.8 ms and 29.1 MB allocated before, 11.1 ms and 10.8 MB after (BenchmarkDotNet, .NET 10, Windows 11, 10 iterations).
- `PrettyPrintJson()` reuses one printer per thread instead of building the strategy table on every call.
- The strategy machinery (`JsonPPStrategyContext`, `PPScopeState`, `ICharacterStrategy`, the strategy classes, the `JsonPrettyPrinter(JsonPPStrategyContext)` constructor and the public fields `IsProcessingVariableAssignment` and `SpacesPerIndent`) is still public but hidden from IntelliSense with `EditorBrowsable(Never)`. It becomes internal in 3.0.
- CI runs on Ubuntu and Windows, runs the net48 test leg on Windows (so the netstandard2.0 build is executed), collects coverage, checks `dotnet format`, verifies the tag matches the packed version, and creates a GitHub Release with the packages attached.

## [2.0.0] - 2026-09-24

- Rebuilt with the current .NET SDK for `netstandard2.0` and `net10.0`; the `net35` build is gone.
- `PrettyPrintJson()` output is unchanged, except that an empty object prints as an opening and a closing line with nothing between (it used to leave an indented blank line).
- `ToJSON()` and `DeserializeFromJson()` use `System.Text.Json` instead of `JavaScriptSerializer` (which does not exist outside .NET Framework). Dates therefore serialise as ISO 8601 (`2010-01-01T00:00:00`) instead of `\/Date(1262325600000)\/`, and quotes inside strings are escaped as `\u0022` (corrected on 2026-09-28: this entry said `\"`; the other serializer changes are listed under 3.0.2-beta.1). If you need the old serialiser, stay on 1.0.1.1.
- SourceLink, deterministic build, a symbols package and the README are in the package.

## [1.0.1.1] - 2014

- Original `net35` release.

[Unreleased]: https://github.com/m4bwav/DotNetJsonPrettyPrinter/compare/v3.0.2-beta.1...HEAD
[3.0.2-beta.1]: https://github.com/m4bwav/DotNetJsonPrettyPrinter/compare/v3.0.1...v3.0.2-beta.1
[3.0.1]: https://github.com/m4bwav/DotNetJsonPrettyPrinter/compare/v2.1.1...v3.0.1
[3.0.0]: https://github.com/m4bwav/DotNetJsonPrettyPrinter/compare/v2.1.0...v3.0.0
[2.1.1]: https://github.com/m4bwav/DotNetJsonPrettyPrinter/compare/v2.0.0...v2.1.1
[2.1.0]: https://github.com/m4bwav/DotNetJsonPrettyPrinter/compare/v2.0.0...v2.1.0
[2.0.0]: https://github.com/m4bwav/DotNetJsonPrettyPrinter/compare/1.0...v2.0.0
[1.0.1.1]: https://github.com/m4bwav/DotNetJsonPrettyPrinter/releases/tag/1.0
