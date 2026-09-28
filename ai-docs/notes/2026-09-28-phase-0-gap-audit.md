---
title: Retrofit Phase 0 gap audit (3.0.1)
kind: note
date: 2026-09-28
verified: 2026-09-28
stale_after: 2027-03-28
tags: [retrofit, phase-0, survey, golden, nuget, upgrade, deprecation]
summary: "what JsonPrettyPrinter 3.0.1 lacks against the package-modernize standard (gap table), the golden capture on net48 and net10.0, the upgrade story from 1.0.1.1 and 2.1.1 measured by the same capture, the old versions and the deprecation recommendation, and the shipped README and CHANGELOG errors; read before the retrofit plan or any release"
---

# Retrofit Phase 0 gap audit: JsonPrettyPrinter 3.0.1

## Summary

JsonPrettyPrinter was modernized by hand on 2026-09-24 and 25 (2.0.0, then 2.1.1 and 3.0.1; the v2.1.0 and v3.0.0 tags never published). This audit, made with the package-modernize skill's retrofit path on 2026-09-28, measures it against the skill's standard. The library code needs no change: the golden capture of the published 3.0.1 (1174 cases per runtime) found no bug in the printer. The gaps are in the release path, the repository settings and the docs. The capture also measured the drift that matters to users: about 289,000 of the 310,000 downloads are 1.0.1 and 1.0.1.1, and the move from 1.x to 3.x changes far more of the serializer's output than the CHANGELOG says. Evidence: [2026-09-28-survey.txt](2026-09-28-survey.txt) and `tests/Golden/`.

## Registry and repository

| Fact | Value |
|---|---|
| Versions (all listed) | 1.0.0 and 1.0.1 (2014-03-31), 1.0.1.1 (2014-04-03), 2.0.0, 2.1.1, 3.0.1 (2026-09-25) |
| Downloads (2026-09-28) | 309,939 in all: 1.0.1.1 200,592; 1.0.1 88,032; 1.0.0 21,141; 2.0.0 65; 2.1.1 42; 3.0.1 67 |
| Owner, prefix | rogersm0; no verified prefix; no deprecation, no vulnerability |
| 3.0.1 targets and dependencies | net10.0 (none); netstandard2.0: System.Memory 4.6.3, System.Text.Json 10.0.12 |
| Package files | every version has its DLL under a framework folder (lib/net35 for 1.x); 3.0.1 has README, icon, XML docs, repository commit 1610145 |
| GitHub | 9 stars, 2 forks (2016, a Gitter badge), no issues ever, PRs #1 to #7 all closed or merged, no webhooks, no secrets outside the environment, 0 Dependabot alerts |
| Branches | master; modernize-net10 (merged, PR #2); release-2.1.0 (its tip is the v2.1.1 tag's commit c6632e1, which the tag keeps) |
| Tags | 1.0, v2.0.0, v2.1.0 (cc4f0ca, failing CI, never published), v2.1.1, v3.0.0 (d431ed6, never published), v3.0.1 |
| Releases | v3.0.1, v2.1.1, "Original Version" (tag 1.0); none for v2.0.0 |
| Environment nuget | required reviewer m4bwav, self-review allowed, deployment rules: branch master and tag v*; secret NUGET_USER |
| Baseline (master 08836a7) | locked restore, format, build (0 warnings), 37 of 37 tests on net10.0 and on net48, pack: all exit 0; no vulnerable package; newer than locked: NUnit 5.0.0 and coverlet.collector 10.1.0, both published 2026-09-27 and inside the three-day cooldown |
| README images | 2 badges (NuGet version, CI), both live, identical README in the repository and the package; `check-readme-images.mjs --registry nuget` exit 0 on both |

## Gap table

| Item | Current state | Standard (SKILL.md, references/nuget.md, retrofit.md) |
|---|---|---|
| Golden capture | none before today | done: `tests/Golden/3.0.1.net48-windows.json` and `3.0.1.net10.0-windows.json`, commit 08b777a; needs the replay project |
| Public API list | none; package validation had no baseline since 3.0.0 | done: `tests/Golden/PublicApi-3.0.1.txt` (31 lines, parameter names, init accessors); a test that every line exists |
| Package validation baseline | unset (csproj comment says to set 3.0.1) | the `PackageValidationBaselineVersion` property set to 3.0.1 |
| Release workflow | a `publish` job inside ci.yml, triggered by the tag push, with contents set to write in that job and softprops/action-gh-release | release.yml from the template: tag equals csproj version and sits on master, build and test on Linux and Windows (net48), pack, attest, a push job gated by environment nuget with no checkout, the GitHub Release in its own job |
| Verify from the registry | by hand (2026-09-25 log) | verify-published.yml: both nuget.org indexes, repository signature, consumers on three OSes |
| Action pins | major tags (checkout v7, setup-dotnet v6, upload-artifact v7, download-artifact v8, dorny/test-reporter v3, NuGet/login v1, softprops v3), all node24 | commit SHAs, actionlint and zizmor clean, persist-credentials false |
| CI content | restore, format, build, test (net10.0 both OSes, net48 Windows), coverage, trx with dorny/test-reporter, pack on Linux | template ci.yml: NuGet audit step, package content and dependency check, consumers of the packed package, a final `ci` job for the ruleset |
| Consumers | none | tests/consumers/run.sh from the template: fresh net10.0 and net48 projects against the packed package, source-mapped (L-104) |
| Dependabot | github-actions and nuget weekly, test group, no cooldown, no dotnet-sdk | template: seven-day cooldown on every ecosystem, dotnet-sdk, the library's declared floors (System.Memory, System.Text.Json) ignored |
| Rulesets | none; no classic protection | master: deletion and non-fast-forward blocked, required check `ci`, admin bypass; tags: admins only |
| Security settings | secret scanning, push protection, Dependabot security updates, private vulnerability reporting all off; default workflow permissions write, Actions may approve pull requests | all on; workflow permissions read; Actions may not approve |
| SECURITY.md | missing | template |
| Directory.Build.props, global.json | no props (settings repeated in each csproj); global.json 10.0.100 latestFeature | template props for the audit pipeline switches; global.json kept |
| README | two badges; the comment sentence and the "try it live" sentence are wrong (below); no 1.x upgrade section | three live badges (add downloads); corrections; a short "Upgrading from 1.x" pointing at the wiki |
| CHANGELOG | 2.0.0 entry wrong about escaping; the 1.x to 2.0 serializer drift mostly unstated | corrected entry and the measured upgrade story in the next release's section |
| Icon | present (the blue style) | nothing to do |
| AGENTS.md, CLAUDE.md, Copilot pointer | present; CLAUDE.md imports AGENTS.md; AGENTS.md describes the old publish job | release section rewritten for release.yml; template rules added |
| ai-docs | old layout (log/ folder, undated plan) | converted to everlast, mode repo, sync push today (commit 366b93a) |
| GitHub wiki | 11 pages by hand for 3.0.1 (wiki commit c6b285c), verification program `ai-docs/notes/2026-09-28-wiki-verify.cs` but no saved output | wikiwright Update mode: run the program, save its output, fix every `outputs` finding, then this run's changes; Versions and upgrading takes the upgrade story below |
| Repository homepage | the 2014 article on markdavidrogers.com | the nuget.org page (overlay) |
| Stale branches and tags | two branches, two never-published tags | branches deleted with the maintainer's OK; the two tags kept as history (documented) or deleted, the maintainer's call |
| Data provenance (L-093) | no embedded data files | nothing to rebuild |

## The golden capture

`tests/Golden/Capture` is split like RandomNameGeneratorLibrary's (Cases.cs, Json.cs, Program.cs; empty MSBuild files beside it). It references the published package from nuget.org by exact version and records: every corpus input (110 documents: well-formed, whitespace, strings and escapes, scalars, text outside ASCII and lone surrogates, eleven kinds of blank input, 31 malformed ones) through `PrettyPrintJson()` and the five other entry points, idempotence, 16 option sets over 10 inputs, option errors, the options record's behaviour, every null argument, printer reuse after malformed input, writers (partial output before an exception), big and deep input (1 MB and 4 MB documents, a 1,000,000-character string, 1,000,000 array items, 1,000-deep and 20,000-deep nesting), eight threads at once, three cultures, `ToJson` over 36 values and 14 option and type-info cases, and `DeserializeFromJson` over 47 inputs.

- 1174 cases per runtime, each run twice and byte-identical; 64-bit process on both; System.Text.Json 10.0.12 on both (the package on net48, the framework's on .NET 10.0.12). The header SHA-256 of each loaded DLL equals the file in the nuget.org package (072fa348 for lib/netstandard2.0, e5925fa6 for lib/net10.0).
- 1143 of 1174 answers are equal across the runtimes. The 31 others: 22 exception messages (.NET adds " (Parameter 'x')"), 2 OutOfMemoryException wordings, and 7 serializer answers: `ToJson` on .NET Framework writes 0.1 as `0.10000000000000001`, 1e300 as `1.0000000000000001E+300`, 0.1f as `0.100000001` and -0.0 as `0` (System.Text.Json's netstandard2.0 build), and `DeserializeFromJson<double>("1e400")` throws there but returns Infinity on .NET 10. None of this is in the README.
- Exceptions raised inside System.Text.Json carry a `$from` marker, so the replay can compare type and path only if a later runtime's System.Text.Json words a message differently.

Behaviour the README does not state, all from the recording: blank input is judged by `char.IsWhiteSpace` (so U+2028 or U+3000 alone print as an empty string) but only space, tab, CR and LF are dropped between tokens (a no-break space or U+200B stays in the output); a leading BOM is copied through; a writer keeps what was written before a `FormatException`; `PrettyPrint(" ", null)` throws for the writer even though the input is blank; `IndentSize = int.MaxValue` fails with OutOfMemoryException in the constructor; the options record's `ToString()` prints the raw newline.

## The upgrade story, measured

The same Cases.cs was compiled against 2.1.1 (unchanged) and 1.0.1.1 (with `JPP_V1` defined: 446 cases its API allows) and run on both runtimes; `compare.py` reports are in `tests/Golden/upgrade/`. 1.0.0 and 1.0.1 were run too (not committed): all 446 answers equal 1.0.1.1's.

**2.1.1 to 3.0.1:** 983 cases identical, 179 differ only in line endings (CRLF to LF on Windows), and the rest are the summarised big outputs (length differs by the line count), the options record comparing a CRLF option with the default, and `IndentSize = int.MaxValue` failing in the constructor instead of at the first call. The CHANGELOG's 3.0.0 entry is accurate.

**1.0.1.1 to 3.0.1** (net48; 273 identical, 60 line endings only, 110 different):

- Printing: empty objects and arrays print as `{}` and `[]` (1.x: an opening line, a blank indented line, a closing line); an escaped backslash before a closing quote no longer stops the formatting of the rest of the document; a stray or extra closing bracket throws FormatException with the index (1.x: InvalidOperationException "Stack empty."); a wrong kind of closing bracket throws (1.x printed it); unclosed documents and trailing commas no longer leave indentation-only lines; null throws ArgumentNullException (1.x: NullReferenceException); a reused printer no longer carries string or depth state into the next document; lines end with LF (1.x: Environment.NewLine). Everything else prints the same, including comments, single quotes and bare words.
- `ToJSON` became `ToJson` over System.Text.Json: dates as ISO 8601 (1.x: `/Date(ms)/` with escaped slashes, and a DateTime of unspecified kind taken as local time), DateTimeOffset keeps its offset, public fields are no longer written, TimeSpan and Version as strings, byte arrays as base64, `Dictionary<int,...>` now works, NaN and infinities throw (1.x wrote invalid JSON), HTML-sensitive characters escaped upper case, non-ASCII and emoji escaped, a lone surrogate becomes U+FFFD, a cycle throws JsonException, and on .NET Framework doubles print with 17 significant digits.
- `DeserializeFromJson` is strict and case-sensitive: 1.x accepted lower-case property names, single quotes, unquoted keys, numbers as strings, strings as numbers, `1e3` for an int and enum names, and turned ISO dates into local time; 3.x throws JsonException for each of those, except that a lower-case key is silently ignored. `object` targets give a JsonElement (1.x: dictionaries and arrays). The `/Date(ms)/` form 1.x wrote cannot be read back.
- On .NET Core and .NET 5 or later, 1.x's `ToJSON` and `DeserializeFromJson` throw FileNotFoundException for System.Web.Extensions 3.5 (every call, both recorded); only its printer works there.

## Old versions

Package files: all fine (no broken layout as in RandomNameGeneratorLibrary 1.1.x). Loading (L-108): 1.0.1.1 and 2.1.1, the latest of each old major, were installed from nuget.org and run on net48 and net10.0 by the capture. 1.x loads on .NET 10 through the .NET Framework fallback (NU1701), but its serializer half fails there (above). 1.0.1's assembly reports version 1.0.0.0.

Deprecation recommendation for the maintainer (nuget.org, Manage package, Deprecation; no deletion exists): 1.0.0, 1.0.1, 1.0.1.1 and 2.0.0 as Legacy, 2.1.1 and 3.0.x left alone. The exact fields are in the retrofit plan.

## Shipped docs that are wrong

1. README, "Not a validator": comments "come out indented rather than rejected"; a `//` comment swallows the next token (`print/line-comment` in the recording). Known since the wiki run.
2. README, first paragraph: "Try it live" names a site tool that formats with System.Text.Json, not this package. Known since the wiki run.
3. CHANGELOG, 2.0.0: "quotes inside strings are escaped as `\"`"; System.Text.Json writes the six-character escape for U+0022 (a backslash, u, 0022; see `tojson/string-html` in the recording). Known since the wiki run.
4. CHANGELOG, 2.0.0: describes the serializer change as dates and quotes only; the measured list above is much longer, and nothing says 1.x read dates as local time.
5. README: no word that `ToJson` formats doubles differently on .NET Framework.

## What the earlier pass changed without guarding it

2.0.0 replaced JavaScriptSerializer with System.Text.Json and rewrote the test expectation file to the new output (references/nuget.md records this), so the serializer drift above was never measured until today. The printer changes of 2.1.0 and 3.0.0 are all in the CHANGELOG and match the recording.

Related: builds on [2026-09-28-survey.txt](2026-09-28-survey.txt); see also [2026-09-28-github-wiki.md](2026-09-28-github-wiki.md), [2026-09-25-modernization-2.1.0-and-3.0.0.md](2026-09-25-modernization-2.1.0-and-3.0.0.md).
