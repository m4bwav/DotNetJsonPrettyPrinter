# DotNetJsonPrettyPrinter: notes for coding agents

NuGet package `JsonPrettyPrinter` (namespace `JsonPrettyPrinterPlus`). Library in `JsonPrettyPrinterPlus/`, NUnit tests in `JsonPrettyPrinterPlusTests/`, the golden replay and API test in `tests/JsonPrettyPrinterPlus.GoldenTests/`, the recordings in `tests/Golden/`, package consumers in `tests/consumers/`, BenchmarkDotNet project in `benchmarks/`. Plans, decisions and notes live in `ai-docs/` (see its INDEX); read the plans there before changing the API.

## Build and test

```
dotnet restore --locked-mode          # lock files are committed; run plain `dotnet restore` after changing a PackageReference and commit the lock file
dotnet format --verify-no-changes     # .editorconfig is enforced in CI and in the build
dotnet build -c Release               # TreatWarningsAsErrors + latest-recommended analyzers on both projects
dotnet test -c Release                # unit tests and the golden replay; net10.0 and net48 (net48 executes only on Windows)
dotnet pack JsonPrettyPrinterPlus -c Release -o artifacts   # runs package validation against the baseline version in the csproj
dotnet run -c Release --project benchmarks/JsonPrettyPrinterPlus.Benchmarks   # 1 MB document; put before/after numbers in CHANGELOG.md
```

## Rules

- Output for well-formed input is the contract. Any change to what `PrettyPrintJson()` writes needs a test and a CHANGELOG entry, and is a minor bump at most when it only affects broken input.
- The library multi-targets `netstandard2.0` and `net10.0`: no `ArgumentNullException.ThrowIfNull`, `HashCode`, or other net5+ APIs without an `#if` or polyfill. `IsExternalInit.cs` is the one polyfill so far.
- `net10.0` is trim and AOT compatible. Anything that touches reflection-based System.Text.Json must carry `RequiresUnreferencedCode` and `RequiresDynamicCode` under `#if NET5_0_OR_GREATER`, and get a `JsonTypeInfo<T>` overload beside it.
- Files are LF (`.gitattributes` and `.editorconfig`); do not commit CRLF.
- The golden files are the contract: `tests/Golden/` (the 3.0.1 recordings, the capture program, the API list, the upgrade recordings) never changes after commit 08b777a. When `tests/JsonPrettyPrinterPlus.GoldenTests` fails, fix the library, or bring the difference to the maintainer as a named exception; never edit or regenerate a recording. `git diff --exit-code 08b777a -- tests/Golden` must stay empty.
- Releasing: add a dated `## [X.Y.Z] - YYYY-MM-DD` section to `CHANGELOG.md` (release.yml takes the notes from it), set `<Version>` in `JsonPrettyPrinterPlus/JsonPrettyPrinterPlus.csproj`, merge through a pull request (master requires the `ci` check), wait for `ci` to pass on master, then tag that commit `vX.Y.Z` and push the tag. `release.yml` checks the tag against the version and master, tests, attests, and waits for the maintainer to approve the `nuget` environment before pushing through Trusted Publishing; the policy on nuget.org names `release.yml`. Afterwards run `verify-published.yml` with the version and raise `PackageValidationBaselineVersion`. Only repository admins can push tags. Never add an AI byline or trailer to commits, PRs or files.
- Workflows: actions are pinned to commit SHAs with the version in a comment; run `actionlint` and `uvx zizmor --offline .` after any change. `tests/consumers/run.sh VERSION artifacts` runs the packed package in fresh net10.0 (and, on Windows, net48) projects.
- Write what you learned or decided to `ai-docs/` before finishing a task (everlast, below).

## everlast (session knowledge, load on demand)

- `ai-docs/INDEX.md` lists what past sessions learned here (solutions with verified commands, decisions with reasons, plans). At the start of a task, scan it and open only the entries whose title or tags match; no line matches: `everlast.py search "<key terms>"` before concluding nothing was recorded. Read `ai-docs/HANDOFF.md` when continuing unfinished work (everlast-resume skill).
- Before acting on an entry marked `(recheck due)`, run `everlast.py recheck <entry>`, re-run its Verified-by command only when that is read-only or safe (a build, a test, a version query), then record `everlast.py verify <entry>` or `verify <entry> --failed "what broke"`; a fix that changed is superseded, never reused blindly.
- Before finishing a task that hit a dead end, verified a non-obvious command, made a design choice, or taught you something about the user, record it (everlast-capture skill, or `everlast.py note` / `handoff`); rewrite `HANDOFF.md` when work is left unfinished. Say "nothing to record" when that is true.
- Anything naming a person, an internal host or name, a credential, or an opinion about people goes to the private sidecar (`--private`), never here. Lessons about the user or this machine go to the user tier (`--user`).
- Rules go in this file, system layout in CODEMAP.md; the doc set holds only what could not be re-derived from the code in a minute.
- Link documents together with relative markdown links: every markdown folder is reachable from an index whose lines say when to read each file (`ai-docs/INDEX.md` is generated from frontmatter; give entries a one-line `summary`), and an entry links the entries it relates to on a typed `Related:` line (`supersedes`, `contradicts`, `builds on`, `see also`). The set then reads as a graph for people in Obsidian and for agents alike. No wikilinks in the repo.
