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
- Write what you learned or decided to `ai-docs/log/` before finishing a task.
