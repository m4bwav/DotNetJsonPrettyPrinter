# Log

Append-only. One line per operation: `## [YYYY-MM-DD] op | title` where op is one of add, update, supersede, verify, verify-failed, prune, handoff, index. Newest at the bottom. Never edited, only appended; this is the history the entries themselves do not carry.

## [2026-09-28] init | scaffolded

## [2026-09-28] add | GitHub wiki written and published for 3.0.1 (moved from the old log/ folder)
- Eleven wiki pages, a sidebar and a footer were written for https://github.com/m4bwav/DotNetJsonPrettyPrinter/wiki and pushed as wiki commit `c6b285c` from the sibling working copy `D:\m4bwa\Claude\Projects\Ai\labs\DotNetJsonPrettyPrinter.wiki`. Every example was run against the published 3.0.1 package first (the program is `ai-docs/notes/2026-09-28-wiki-verify.cs`); the repository's 37 tests pass on net10.0 and net48.
- Also confirmed today: 2.1.1 and 3.0.1 are live on nuget.org (published 2026-09-25), so the publish hand-off from the 2026-09-25 log is closed.
- Found and not fixed, because they ship inside the package and need a release: the README's claim that comments pass through (a `//` comment swallows the next token), the CHANGELOG 2.0.0 line about `\"` (System.Text.Json writes `\u0022`), the README's "try it live" sentence (the site uses System.Text.Json, not this package), and the csproj's pending `PackageValidationBaselineVersion` 3.0.1. Details, the verification facts and the update procedure: `ai-docs/notes/2026-09-28-github-wiki.md`.
## [2026-09-28] index | rebuilt (3 entries)

## [2026-09-28] add | Retrofit Phase 0: survey, baseline, golden capture, upgrade recordings, gap audit (Windows 11, SDK 9.0.317 and 10.0.401, gh)
- package-modernize and wikiwright pulled with `git -C` (both already up to date). Branch `v3-retrofit` from master 08836a7.
- `survey-nuget.sh JsonPrettyPrinter m4bwav/DotNetJsonPrettyPrinter` (runs survey-github.sh too) and the baseline, README-image and package checks > notes/2026-09-28-survey.txt. Findings in notes/2026-09-28-phase-0-gap-audit.md.
- Baseline: `dotnet restore --locked-mode`, `dotnet format --no-restore --verify-no-changes`, `dotnet build --no-restore -c Release` (0 warnings), `dotnet test --no-build -c Release -f net10.0` and `-f net48` (37 of 37 each), `dotnet pack`: all exit 0. `dotnet package list --vulnerable --include-transitive`: none. nuget-latest.py: everything locked is latest except NUnit 5.0.0 and coverlet.collector 10.1.0 (both 2026-09-27, inside the cooldown).
- `check-readme-images.mjs README.md --registry nuget` on the repository README and the README inside the 3.0.1 nupkg (identical): 2 badges ok, "checked for nuget", exit 0.
- tests/Golden/ApiList: PublicApi-3.0.1.txt (31 lines; netstandard2.0 and net10.0 identical), plus 2.1.1 and 1.0.1.1 lists under tests/Golden/upgrade/.
- Golden capture tests/Golden/Capture (JsonPrettyPrinter [3.0.1] from nuget.org): `dotnet run -c Release -f net48 -p:OldVersion=3.0.1` and `-f net10.0`, twice each: 1174 cases, byte-identical per runtime; 64-bit; System.Text.Json 10.0.12 on both. assemblySha256 072fa348... (net48, lib/netstandard2.0) and e5925fa6... (net10.0) equal `sha256sum` of the nupkg's lib files. 1143 of 1174 equal across runtimes.
- The same program with `-p:OldVersion=2.1.1` (1174 cases) and `-p:OldVersion=1.0.1.1` (446 cases, JPP_V1) on both runtimes, twice each, identical. `compare.py` reports in tests/Golden/upgrade/. 1.0.0 and 1.0.1 (net48, scratch) equal 1.0.1.1 in all 446 cases; 2.0.0 (scratch copy with JPP_V1) keeps the escaped-backslash bug and prints `{}` on two lines.
- Commit 08b777a: recordings, capture, API lists, upgrade folder. From here on `git diff --exit-code 08b777a -- tests/Golden` must stay empty.
- everlast registered (mode repo, sync push); ai-docs/log/ folded into notes/ and log.md, plan renamed with frontmatter; lint clean after rewording an MSBuild element in prose (read as a placeholder). Commit 366b93a.

## [2026-09-28] add | Retrofit Phase 1: plan and decision record, stop for rulings
- plans/2026-09-28-retrofit-and-3.0.2-release.md (D1-D17, the D16 deprecation fields, one question); decisions/2026-09-28-retrofit-without-code-changes.md (proposed).
## [2026-09-28] index | rebuilt (6 entries)

## [2026-09-28] update | Phase 1 ruled: every recommendation stands
- Maintainer: "Do all the recommendations and record learnings, run all the commands you want".
- Read as: D1 to D17 as recommended; question 2 settings applied; homepage to the nuget.org page; branches modernize-net10 and release-2.1.0 deleted; tags v2.1.0 and v3.0.0 kept; a GitHub Release for v2.0.0 created (no recommendation was given; additive, run under "run all the commands you want"). Unconfirmed until read back on nuget.org: the policy edit (D7) and the deprecations (D16), which are the maintainer's.

## [2026-09-28] add | Retrofit Phase 2: golden replay, canary, templates, docs
- tests/JsonPrettyPrinterPlus.GoldenTests (NUnit 4.6.1, net10.0 and net48 win-x64, Cases.cs and Json.cs linked unchanged): first build had two analyzer errors in the new test code (CA1845, CA1865) and one comparer error on net48 (JsonElement.GetString() refuses the recorded lone surrogates; the template compares strings with GetString); after comparing strings by raw JSON text, all 1174 answers equal the recording on both runtimes, no answer differed. Commit 7cbc419.
- Canary after the commit: PrettyPrintEngine.cs empty-scope rule flipped (and to or): replay red on net10.0 (about 52 differing cases) and net48; reverted with git checkout; green on both.
- Templates adapted with a byte-safe script (ci.yml, release.yml, verify-published.yml, dependabot.yml, tests/consumers, SECURITY.md, a Directory.Build.props with the audit switches only); grep for placeholder tokens and TEMPLATE: none. ci.yml lost the publish and dorny test-report jobs.
- csproj: 3.0.2-beta.1, PackageValidationBaselineVersion 3.0.1 (validation passed in pack). README and CHANGELOG per plan D12 and D13; everwrite checker 0 strong.
- Local, CI commands as written: locked restore, format, build 0 warnings, audit restore, tests net10.0 43 and net48 43 (37 unit, 6 golden), pack. Content check extracted from ci.yml under bash -e -o pipefail: exit 0; with a wrong floor and a wrong file list: exit 1 with its error line. Consumers: packed 3.0.2-beta.1 on net10.0 and net48 green; nuget.org 3.0.1 green; a wrong expected line exit 1.
- actionlint 1.7.12 with shellcheck 0.11.0 (release zips in the scratchpad): SC2034 in the template's verify-published loop (unused variable), fixed; then clean. zizmor 1.30.1 offline: no findings. check-workflow-shell.py: clean. Every pinned SHA equals its tag, each the latest release.
- check-readme-images.mjs README.md --registry nuget: 3 images ok, exit 0. tests/Golden untouched since 08b777a.
