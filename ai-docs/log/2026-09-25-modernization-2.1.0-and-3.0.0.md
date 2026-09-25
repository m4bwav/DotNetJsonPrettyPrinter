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

- The whole `JsonPrettyPrinterInternals` namespace was deleted and replaced by `JsonPrettyPrinterPlus/PrettyPrintEngine.cs`: an internal sealed class with a `switch` per character, a `bool[]` scope stack (true = array) and the same deferred-break rule as 2.1. `JsonPrettyPrinter` is sealed and owns one engine, reset at the start of every `Print`.
- Benchmark on the same 1 MB document: 4.49 ms and 10.35 MB (2.1.0: 11.09 ms and 10.75 MB). The allocation floor is the output `StringBuilder` plus `ToString()`; the `TextWriter` overloads avoid it.
- `NewLine` default is `"\n"`. The tests now use a `const NewLine = "\n"` and one test asks for `Environment.NewLine` explicitly.
- `ToJSON` removed; the CA1708 suppression went with it. The `System.Diagnostics.CodeAnalysis` using is inside `#if NET5_0_OR_GREATER` because nothing else in that file needs it on netstandard2.0.
- System.Text.Json on netstandard2.0: kept. Reasoning: dropping it removes `ToJson`/`DeserializeFromJson` for exactly the consumers most likely to want a one-liner (Unity, old Framework apps), and a second package for three extension methods is more maintenance than it saves. README says it is the only dependency.
- `PackageValidationBaselineVersion` removed for 3.0.0 (no compatible baseline exists). `EnablePackageValidation` stays on so the TFM compatibility checks still run. Re-add the baseline as 3.0.0 after it is published.

## CI fix after the first 2.1.0 push

The v2.1.0 tag was first pushed at commit `cc4f0ca`, whose Windows leg failed: the net48 section of the test project's lock file listed `Microsoft.NETFramework.ReferenceAssemblies` (this machine has no .NET Framework targeting pack, so the SDK adds the package implicitly) while the Windows runner, which has the pack, did not. Fix in `c7373bd`: an explicit `PackageReference` for net48 with `PrivateAssets=all`, so every machine restores the same graph. The same commit gave the `test-report` job a checkout (dorny/test-reporter runs `git ls-files`) and folded in the five dependabot PRs (checkout 7, setup-dotnet 6, test-reporter 3, action-gh-release 3, coverlet.collector 10.0.1), which were then closed.

Moving the `v2.1.0` tag to the fixed commit needs a force push of the tag, which this session was not allowed to do. Until it is moved, the 2.1.0 publish job never runs (its build failed), so nothing was published by mistake.

The v3.0.0 tag (commit `d431ed6`) built green on both OSes but its publish job failed at `actions/setup-dotnet`: the job has no checkout, so `global-json-file: global.json` could not be found. Fixed in the next commit by pinning `dotnet-version: 10.0.x` in that job only. Note the `nuget` environment ran the job at once, so it has no required reviewers: a green tag build publishes without a manual approval. Both `v2.1.0` (`cc4f0ca`) and `v3.0.0` (`d431ed6`) therefore need moving to a commit at or after the fix before anything reaches nuget.org.

Package versions were checked with `dotnet list package --outdated` on 2026-09-25: every package in the three projects is at its latest (System.Text.Json 10.0.12, System.Memory 4.6.3, NUnit 4.6.1, NUnit3TestAdapter 6.3.0, Microsoft.NET.Test.Sdk 18.10.1, coverlet.collector 10.0.1, BenchmarkDotNet 0.15.8, Microsoft.NETFramework.ReferenceAssemblies 1.0.3).

Branch `release-2.1.0` (tip `3e49d65`) is `c7373bd` plus the publish fix, so `v2.1.0` can be moved there; `v3.0.0` goes to master `2568ccd` or later. Tag moves are `git tag -f <tag> <commit>` then `git push -f origin <tag>`; this session could not force-push.

Lesson for this harness: bash heredocs mangled `\\n` inside Python string literals twice in this session; the Edit tool was reliable for every replacement that contained backslashes.

## Release procedure followed

1. `CHANGELOG.md` entry, `<Version>` bump, commit.
2. `git tag v2.1.0`, `git push origin master v2.1.0`.
3. The `publish` job waits for approval of the `nuget` environment in GitHub Actions. Approve it, then check https://www.nuget.org/packages/JsonPrettyPrinter.
4. After 2.1.0 is on nuget.org, `PackageValidationBaselineVersion` can be raised to 2.1.0 for the next 2.x release. 3.0.0 drops the baseline (see below).

## Related

- Review notes that produced the plan: `markdavidrogers-site/ai-docs/research/2026-09-25-jsonprettyprinter-2.0-review.md` (in the sibling repo).
- Sibling library with the same plan shape: `DotNetRandomNameGenerator/ai-docs/plans/modernization-plan.md`.

## Published as 2.1.1 and 3.0.1

Force-pushing tags is blocked for this session by the auto-mode classifier even with Mark authorising it, so the fixed commits were released under new numbers instead: `v2.1.1` on branch `release-2.1.0` and `v3.0.1` on master. The `v2.1.0` and `v3.0.0` tags stay where they are and were never published. Next baseline for package validation: 3.0.1.

State at hand-off: runs 36088374794 (v2.1.1) and 36088390174 (v3.0.1) are green through build and test-report and their publish jobs are waiting for `nuget` environment approval. Approving through `gh api .../pending_deployments` was blocked by the auto-mode classifier, so Mark approves in the browser (Review deployments button on each run). Once 3.0.1 is on nuget.org, set PackageValidationBaselineVersion to 3.0.1.
