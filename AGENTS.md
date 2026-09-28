# DotNetJsonPrettyPrinter: notes for coding agents

NuGet package `JsonPrettyPrinter` (namespace `JsonPrettyPrinterPlus`). Library in `JsonPrettyPrinterPlus/`, NUnit tests in `JsonPrettyPrinterPlusTests/`, BenchmarkDotNet project in `benchmarks/`. Plans and session logs live in `ai-docs/`; read `ai-docs/plans/modernization-plan.md` before changing the API.

## Build and test

```
dotnet restore --locked-mode          # lock files are committed; run plain `dotnet restore` after changing a PackageReference and commit the lock file
dotnet format --verify-no-changes     # .editorconfig is enforced in CI and in the build
dotnet build -c Release               # TreatWarningsAsErrors + latest-recommended analyzers on both projects
dotnet test -c Release                # net10.0 and net48 (net48 executes only on Windows)
dotnet pack JsonPrettyPrinterPlus -c Release -o artifacts   # runs package validation against the baseline version in the csproj
dotnet run -c Release --project benchmarks/JsonPrettyPrinterPlus.Benchmarks   # 1 MB document; put before/after numbers in CHANGELOG.md
```

## Rules

- Output for well-formed input is the contract. Any change to what `PrettyPrintJson()` writes needs a test and a CHANGELOG entry, and is a minor bump at most when it only affects broken input.
- The library multi-targets `netstandard2.0` and `net10.0`: no `ArgumentNullException.ThrowIfNull`, `HashCode`, or other net5+ APIs without an `#if` or polyfill. `IsExternalInit.cs` is the one polyfill so far.
- `net10.0` is trim and AOT compatible. Anything that touches reflection-based System.Text.Json must carry `RequiresUnreferencedCode` and `RequiresDynamicCode` under `#if NET5_0_OR_GREATER`, and get a `JsonTypeInfo<T>` overload beside it.
- Files are LF (`.gitattributes` and `.editorconfig`); do not commit CRLF.
- Releasing: add the version to `CHANGELOG.md`, bump `<Version>` in `JsonPrettyPrinterPlus/JsonPrettyPrinterPlus.csproj`, tag `v<version>` and push the tag. The `publish` job checks tag against version, needs the `nuget` environment approval, pushes through Trusted Publishing and creates a GitHub Release. Never add an AI byline or trailer to commits, PRs or files.
- Write what you learned or decided to `ai-docs/` before finishing a task (everlast, below).

## everlast (session knowledge, load on demand)

- `ai-docs/INDEX.md` lists what past sessions learned here (solutions with verified commands, decisions with reasons, plans). At the start of a task, scan it and open only the entries whose title or tags match; no line matches: `everlast.py search "<key terms>"` before concluding nothing was recorded. Read `ai-docs/HANDOFF.md` when continuing unfinished work (everlast-resume skill).
- Before acting on an entry marked `(recheck due)`, run `everlast.py recheck <entry>`, re-run its Verified-by command only when that is read-only or safe (a build, a test, a version query), then record `everlast.py verify <entry>` or `verify <entry> --failed "what broke"`; a fix that changed is superseded, never reused blindly.
- Before finishing a task that hit a dead end, verified a non-obvious command, made a design choice, or taught you something about the user, record it (everlast-capture skill, or `everlast.py note` / `handoff`); rewrite `HANDOFF.md` when work is left unfinished. Say "nothing to record" when that is true.
- Anything naming a person, an internal host or name, a credential, or an opinion about people goes to the private sidecar (`--private`), never here. Lessons about the user or this machine go to the user tier (`--user`).
- Rules go in this file, system layout in CODEMAP.md; the doc set holds only what could not be re-derived from the code in a minute.
- Link documents together with relative markdown links: every markdown folder is reachable from an index whose lines say when to read each file (`ai-docs/INDEX.md` is generated from frontmatter; give entries a one-line `summary`), and an entry links the entries it relates to on a typed `Related:` line (`supersedes`, `contradicts`, `builds on`, `see also`). The set then reads as a graph for people in Obsidian and for agents alike. No wikilinks in the repo.
