# 2026-09-25: modernization plan executed, 2.1.0 and 3.0.0 released

All six stages of [the modernization plan](../plans/modernization-plan.md) were done in one session on the branch `master`, as two tagged releases. This note records what was decided that the code and CHANGELOG do not say, and what to watch after the tags were pushed.

## 2.1.0 (stages 1 to 5)

### Design choice: deferred line breaks instead of backtracking

The 2.0 context wrote a newline and indent immediately after `{`, `[` and `,`, then on `}` tried to delete trailing spaces from the `StringBuilder` (never the newline, hence the `{\n}` output). 2.1 instead records that a line break is owed (`_pendingBreak`) and writes it just before the next character. A closing bracket whose previous significant character was the matching opener cancels the pending break. Consequences:

- `{}`, `[]`, `{ }`, `[ ]` all print as `{}` / `[]` at any depth without touching the output already written.
- The sink can be any `TextWriter`, which gave the streaming overloads for free. The `StringBuilder` overload of `JsonPPStrategyContext.PrettyPrintCharacter` wraps a `StringWriter` over the caller's builder and is kept only for 2.x callers.
- `RemoveTrailingIndent()` is now "cancel the pending break"; it still strips spaces when the output is a `StringBuilder` so old callers get the old effect.

### Design choice: escapes handled before strategy dispatch

`JsonPPStrategyContext.PrettyPrintCharacter` handles `\` inside a string itself: it toggles `_escapeNext`, writes the character, and never dispatches to a strategy. The quote strategies therefore no longer look at `WasLastCharacterABackSlash` (kept, hidden, unused). This fixes `\\"` and also makes `\uXXXX`, `\n` and `\"` trivially right.

### Design choice: skipped whitespace does not become the previous character

The empty-scope check compares the previous *significant* character. Whitespace outside strings is skipped by the strategies and now also skipped by the previous-character bookkeeping.

### What was kept public and why

Package validation (`EnablePackageValidation`, baseline 2.0.0, APICompat) passes, so 2.1.0 is binary compatible with 2.0.0. Everything that is only there for compatibility carries `[EditorBrowsable(Never)]` and a "removed in 3.0" note: the `JsonPrettyPrinter(JsonPPStrategyContext)` constructor, the `IsProcessingVariableAssignment` and `SpacesPerIndent` fields, `WasLastCharacterABackSlash`. `SpacesPerIndent` is still honoured (the indent string is rebuilt when the field changes), because 2.0 users could set it.

### Analyzer suppressions and why

- CA1510 (`ThrowIfNull`) off in `.editorconfig`: the API does not exist on netstandard2.0.
- CA1001 on the context: the `StringWriter` wraps a caller-owned `StringBuilder`.
- CA1051 around the two public fields, CA1720 around `JsonScope.Object`, CA1708 on `ToJSON`/`ToJson`: all 2.x compatibility, all gone in 3.0.
- CA1707 off for the test project: test names use underscores.

### Test matrix

The test project targets `net10.0;net48` unconditionally so the lock file is the same on every OS; CI runs the net48 leg on Windows only (`dotnet test -f net48`). net48 is what actually executes the netstandard2.0 build of the library. Consequence for test code: no `HashCode.Combine`, no `string.GetHashCode(StringComparison)`, no `init`.

### CI notes

- `dotnet restore --locked-mode` with committed `packages.lock.json` in all three projects (the benchmark project too, or locked mode fails on it).
- `.gitattributes` forces LF so `dotnet format --verify-no-changes` gives the same answer on the Windows runner; `.editorconfig` says `end_of_line = lf`.
- The tag/version check is a `sed` on the csproj in the build job; the pack still uses `<Version>` (MinVer was not adopted, to keep the csproj the single place the version lives).
- Coverage is collected with coverlet on the net10.0 leg only and uploaded with the trx files; `dorny/test-reporter` runs in a separate job with `checks: write`, so the build job keeps `contents: read`.

### Benchmark

`benchmarks/JsonPrettyPrinterPlus.Benchmarks`, 1 MB generated document, .NET 10, Windows 11, `SimpleJob(warmup 3, iterations 10)` with `MemoryDiagnoser`:

| Version | Mean | Allocated |
| --- | --- | --- |
| 2.0.0 code | 16.77 ms | 29.07 MB |
| 2.1.0 code | 11.09 ms | 10.75 MB |

Most of the allocation win is not copying the input into a `StringBuilder` and not allocating a `DefaultCharacterStrategy` per character; the remaining 10.75 MB is the output builder growing plus the final `ToString()`.

## 3.0.0 (stage 6)

See the second half of this note, written after the 3.0.0 work.

## Release procedure followed

1. `CHANGELOG.md` entry, `<Version>` bump, commit.
2. `git tag v2.1.0`, `git push origin master v2.1.0`.
3. The `publish` job waits for approval of the `nuget` environment in GitHub Actions. Approve it, then check https://www.nuget.org/packages/JsonPrettyPrinter.
4. After 2.1.0 is on nuget.org, `PackageValidationBaselineVersion` can be raised to 2.1.0 for the next 2.x release. 3.0.0 drops the baseline (see below).

## Related

- Review notes that produced the plan: `markdavidrogers-site/ai-docs/research/2026-09-25-jsonprettyprinter-2.0-review.md` (in the sibling repo).
- Sibling library with the same plan shape: `DotNetRandomNameGenerator/ai-docs/plans/modernization-plan.md`.
