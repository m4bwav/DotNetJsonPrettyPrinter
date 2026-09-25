# JsonPrettyPrinter modernization plan

Written 2026-09-25 from a full read of the repo at 2.0.0 (commit `2af876a`), with the three bugs below reproduced against the built library. Meant to be executed in fresh sessions, one stage per session or PR. Tick boxes as work lands. Stages 1 to 5 shipped as **2.1.0** on 2026-09-25 (all in one session; see `ai-docs/log.md`); stage 6 is **3.0.0**.

Repo: https://github.com/m4bwav/DotNetJsonPrettyPrinter. Package: https://www.nuget.org/packages/JsonPrettyPrinter. Release procedure: bump `<Version>` in `JsonPrettyPrinterPlus/JsonPrettyPrinterPlus.csproj`, push a `v<version>` tag, approve the `nuget` deployment in Actions (Trusted Publishing, see README "Releasing"). Longer review notes: `markdavidrogers-site/ai-docs/research/2026-09-25-jsonprettyprinter-2.0-review.md`.

## Already good, do not redo

- csproj: `LangVersion latest`, `GenerateDocumentationFile`, `PackageLicenseExpression MIT`, `PackageReadmeFile`, `PackageTags`, `PackageReleaseNotes`, repository metadata, `PublishRepositoryUrl`, `EmbedUntrackedSources`, `snupkg` symbols, `Deterministic`, `ContinuousIntegrationBuild` on GitHub. System.Text.Json referenced only for netstandard2.0.
- CI: least-privilege permissions, `fetch-depth: 0`, pack once and publish that artifact, `environment: nuget`, `NuGet/login@v1`, `--skip-duplicate`, current action majors.
- NUnit 4.6.1, Test.Sdk 18, adapter 6.3; `.slnx`; README with install, example, 2.0 changes and release steps.

## Stage 1: correctness (S each, output changes only for broken input)

Reproduced 2026-09-25 with `PrettyPrintJson()`:

| Input | Output today | Expected |
| --- | --- | --- |
| `{"a":"x\\","b":1}` | `{\n    "a": "x\\","b":1}` (rest of document swallowed) | `"b": 1` on its own line |
| `{ }` | `{\n    \n}` | `{}` |
| `{"a":{}}` | `"a": {\n}` with `}` at column 0 | `"a": {}` |

- [x] Escaped backslash before a closing quote. `DoubleQuoteStrategy.cs` line 7 and `SingleQuoteStrategy.cs` line 7 use `WasLastCharacterABackSlash`, so `\\"` reads as an escaped quote. Replace with an `escapeNext` flag in `JsonPPStrategyContext` (lines 87 to 100): set on `\` inside a string, cleared by the next character.
- [x] Empty scopes. `_previousChar` is updated for skipped whitespace (`JsonPPStrategyContext.cs` line 99), so `{ }` is not seen as empty; `CloseSquareBracketStrategy.cs` has no empty-array handling; `CloseBracketStrategy.cs` lines 24 to 28 strip the indent and never re-indent for nested empty objects. Make all of `{}`, `[]`, `{ }`, `[ ]` print as `{}` or `[]` at any nesting depth.
- [x] `PrettyPrint(null)` throws NullReferenceException (`JsonPrettyPrinter.cs` line 43): throw `ArgumentNullException`. A stray `}` throws InvalidOperationException from `Stack.Pop` (`PPScopeState.cs` line 32): throw `FormatException` with the character index.
- [x] `JsonPPStrategyContext` never resets its quote and assignment flags, so a reused `JsonPrettyPrinter` inherits state after malformed input. Reset at the start of `PrettyPrint`.
- [x] Tests for every row above plus `\"`, `\n`, `\uXXXX` escapes, nested arrays, idempotence (pretty printing twice gives the same text), and a round trip through `JsonNode.DeepEquals`. Today's four tests use `"` escapes and never hit the quote logic.

## Stage 2: performance and dead code (S, internal only)

- [x] `JsonPrettyPrinter.cs` line 46 copies the input into a `StringBuilder` just to index it; iterate the string directly.
- [x] `JsonPPStrategyContext.cs` lines 91 to 93 allocate a `DefaultCharacterStrategy` per ordinary character and do two dictionary lookups; cache one instance and use `TryGetValue`.
- [x] `InitializeIndent` (line 78) builds the indent by concatenation; use `new string(' ', n)`.
- [x] Add a BenchmarkDotNet project under `benchmarks/` (not packed) with a 1 MB document, so before and after numbers go in the changelog.
- [x] Dead code: `OpenBracketStrategy.IsBeginningOfNewLineAndIndentionLevel` (lines 26 to 29) always returns true because `IsStart` is checked after the brace is appended and `IsInArrayScope` is always false there; `IsProcessingVariableAssignment` is only read by it. `ClearStrategies`, `AddCharacterStrategy`, the injected-context constructor and the `PopJsonType` return value are unused. Remove what is not public; mark public leftovers `[EditorBrowsable(Never)]` until 3.0.
- [x] `.gitignore` lines 1 to 38 are superseded by lines 41 to 43; trim.

## Stage 3: build settings, docs, metadata (S each unless noted)

- [x] `Nullable` enable in both projects (annotations only). `ImplicitUsings` can stay off for netstandard2.0.
- [x] `TreatWarningsAsErrors`, `EnableNETAnalyzers`, `AnalysisLevel latest-recommended`, `EnforceCodeStyleInBuild`, an `.editorconfig`, `global.json` pinning SDK 10 with `rollForward: latestFeature`. Fix what surfaces (M if the analyzers are loud).
- [x] Remove `NoWarn CS1591` and write XML docs on the public entry points (`JsonPrettyPrinter`, `JsonExtensions`, the options type from stage 4). Only `RemoveTrailingIndent` and `JsonExtensions` have docs today.
- [x] `IsTrimmable` and `IsAotCompatible` on net10.0; the System.Text.Json reflection helpers in `JsonExtensions` will warn, so either add `JsonSerializerOptions` and `JsonTypeInfo` overloads or suppress with a documented reason.
- [x] `EnablePackageValidation` with `PackageValidationBaselineVersion` 2.0.0.
- [x] `PackageIcon`, `Copyright` 2014 to 2026 (also `LICENSE`), `PackageReleaseNotes` pointing at the changelog instead of inline text.
- [x] `CHANGELOG.md` (Keep a Changelog): move the README "What changed in 2.0" section there, add 2.1.0.
- [x] README: correct the `{}` claim (line 55) once stage 1 lands, document `SpacesPerIndent` and the OS newline, add a "not a validator" note, mention the options type.
- [x] Derive the version from the tag in CI (`-p:Version=${GITHUB_REF_NAME#v}` on the pack step, or MinVer) so `<Version>` cannot drift from the tag. S to M.

## Stage 4: additive API (M, ship in 2.1.0)

- [x] `JsonPrettyPrintOptions` record: `IndentSize` (default 4), `UseTabs`, `NewLine` (default `Environment.NewLine` for compatibility; `"\n"` becomes the default at 3.0). Constructor `JsonPrettyPrinter(JsonPrettyPrintOptions)` and `PrettyPrintJson(this string, JsonPrettyPrintOptions)`. Note `_indent` is cached in `JsonPPStrategyContext` lines 25 to 37, so a later `SpacesPerIndent` change is ignored today; the options type replaces that field.
- [x] `TextWriter` and `ReadOnlySpan<char>` overloads of `PrettyPrint` so large documents do not need a second string.
- [x] Add `ToJson` beside `ToJSON` in `JsonExtensions.cs` line 16; mark `ToJSON` `[Obsolete]` at 3.0.
- [x] Add `JsonSerializerOptions` overloads to the serializer helpers.

## Stage 5: CI and test matrix (S each, M for the matrix)

- [x] `.github/dependabot.yml` for `github-actions` and `nuget`, weekly.
- [x] `setup-dotnet` with `cache: true` plus `packages.lock.json`.
- [x] `concurrency` group on pull requests; `dotnet format --verify-no-changes` once `.editorconfig` exists.
- [x] Coverage with `coverlet.collector` and `--collect:"XPlat Code Coverage"`, uploaded as an artifact; `--logger trx` with a test reporter action.
- [x] Windows leg in the matrix (catches the `Environment.NewLine` dependence) and a `net48` target on the test project so the netstandard2.0 asset actually executes. M.
- [x] Publish job: fail if the tag does not equal the packed version; create a GitHub Release with generated notes and the nupkg and snupkg attached (`softprops/action-gh-release`, `contents: write` on that job only).
- [x] Tests: drop the `Console.WriteLine` noise (lines 73 to 77); `TestObjects` override `Equals` without `GetHashCode` and compare only lengths and years, so tighten the round-trip assertions.

## Stage 6: 3.0 (L, breaking)

- Make the strategy machinery internal: `JsonPPStrategyContext`, `PPScopeState`, `ICharacterStrategy`, the ten strategy classes and the public fields `IsProcessingVariableAssignment` and `SpacesPerIndent`. Then replace dictionary plus interface dispatch with a `switch` on the character.
- Default `NewLine` to `"\n"`. Remove `ToJSON`.
- Decide the fate of the System.Text.Json helpers on netstandard2.0: STJ 10.0.12 pulls a sizeable dependency tree into a package sold as dependency-light. Options: drop the helpers, move them to a second package, or keep them and say so.

## Related

- The site (`m4bwav/markdavidrogers-web`) formats JSON with System.Text.Json in `Tools/JsonPrettifier.cs` instead of this package. Once stage 1 ships, switching the site's `/tools` page and `prettify_json` MCP tool back to this package is an option; note the site tolerates trailing commas and comments, which this library does not validate.
- Sibling library plan: `DotNetRandomNameGenerator/ai-docs/plans/modernization-plan.md`.
