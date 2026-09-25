# Changelog

All notable changes to the `JsonPrettyPrinter` package. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project uses [Semantic Versioning](https://semver.org/).

## [Unreleased]

## [3.0.0] - 2026-09-25

Breaking release. Output for well-formed input is unchanged apart from the line terminator.

### Changed

- `JsonPrettyPrintOptions.NewLine` defaults to `"\n"` instead of `Environment.NewLine`, so output is the same on every OS. Pass `new JsonPrettyPrintOptions { NewLine = Environment.NewLine }` for the 2.x behaviour.
- The strategy machinery is gone from the public API and replaced by one internal engine with a `switch` per character: `JsonPPStrategyContext`, `PPScopeState`, `ICharacterStrategy`, the ten strategy classes, the `JsonPrettyPrinter(JsonPPStrategyContext)` constructor and the fields `IsProcessingVariableAssignment` and `SpacesPerIndent` no longer exist. Use `JsonPrettyPrintOptions` for indentation. `JsonPrettyPrinter` is now `sealed`.
- The `netstandard2.0` build keeps its `System.Text.Json` dependency for the serialisation helpers. This is deliberate and documented in the README; the alternative (a second package) was judged not worth it for a library this small.

### Removed

- `ToJSON()`; use `ToJson()`, which has been there since 2.1.0.

### Performance

- 1 MB document: 11.1 ms and 10.8 MB allocated in 2.1.0, 4.5 ms and 10.4 MB in 3.0.0 (same benchmark project and machine). The switch replaces a dictionary lookup and an interface call per character.

## [2.1.0] - 2026-09-25

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
- `ToJSON()` and `DeserializeFromJson()` use `System.Text.Json` instead of `JavaScriptSerializer` (which does not exist outside .NET Framework). Dates therefore serialise as ISO 8601 (`2010-01-01T00:00:00`) instead of `\/Date(1262325600000)\/`, and quotes inside strings are escaped as `\"`. If you need the old serialiser, stay on 1.0.1.1.
- SourceLink, deterministic build, a symbols package and the README are in the package.

## [1.0.1.1] - 2014

- Original `net35` release.

[Unreleased]: https://github.com/m4bwav/DotNetJsonPrettyPrinter/compare/v3.0.0...HEAD
[3.0.0]: https://github.com/m4bwav/DotNetJsonPrettyPrinter/compare/v2.1.0...v3.0.0
[2.1.0]: https://github.com/m4bwav/DotNetJsonPrettyPrinter/compare/v2.0.0...v2.1.0
[2.0.0]: https://github.com/m4bwav/DotNetJsonPrettyPrinter/compare/1.0...v2.0.0
[1.0.1.1]: https://github.com/m4bwav/DotNetJsonPrettyPrinter/releases/tag/1.0
