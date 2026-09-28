---
title: Retrofit and 3.0.2 release
kind: plan
status: active
date: 2026-09-28
verified: 2026-09-28
stale_after: never
tags: [v3, plan, nuget, retrofit, 3.0.2, github-actions, tests, release]
summary: "the living plan for the package-modernize retrofit of JsonPrettyPrinter 3.0.1: the gap audit's findings, decisions D1-D17, the one question for the maintainer, phases 0-7 with checkboxes, tests, security, the deprecation fields, verification checklist"
---

# Retrofit and 3.0.2 release plan: JsonPrettyPrinter

The package-modernize skill's retrofit path (its references/retrofit.md, second run after RandomNameGeneratorLibrary) applied to JsonPrettyPrinter 3.0.1. Evidence goes to [../log.md](../log.md); the findings are in [../notes/2026-09-28-phase-0-gap-audit.md](../notes/2026-09-28-phase-0-gap-audit.md).

## Status

Active. Phase 1 reached on 2026-09-28: waiting for the maintainer's rulings on the decisions and the one question below. Branch `v3-retrofit` holds the Phase 0 commits (golden capture 08b777a, everlast 366b93a) and this plan.

## Goal

- Every answer of the published 3.0.1 kept on both runtimes, proven by a golden replay seen red by a canary.
- Released through a gated release.yml with Trusted Publishing, verified from nuget.org on three OSes.
- The README and CHANGELOG corrected, and the measured 1.0.1.1 to 3.x upgrade story written down, because nearly every download is a 1.x version.
- Repository settings, rulesets and pinned workflows at the skill's standard; the wiki brought under wikiwright.

## Where it stands (survey 2026-09-28)

| Fact | Value | Evidence |
|---|---|---|
| Published | 3.0.1 on 2026-09-25; 309,939 downloads, 288,624 of them 1.0.1.1 and 1.0.1 | survey |
| Build | netstandard2.0 and net10.0; NUnit 4.6.1 tests on net10.0 and net48; analyzers, warnings as errors, lock files | baseline: all green, 37 of 37 per runtime |
| Dependencies | netstandard2.0: System.Memory 4.6.3, System.Text.Json 10.0.12 (both latest) | nuget-latest.py |
| Community | no issues ever; PRs #1 to #7 closed or merged; 2 forks from 2016; 0 alerts; no webhooks | survey |
| Release path | a publish job in ci.yml on tag push, environment nuget with a reviewer, Trusted Publishing policy bound to ci.yml | ci.yml, environment API |
| Settings | no rulesets; scanning, push protection, private reporting off; workflow permissions write | survey |
| Golden capture | 1174 cases per runtime, byte-identical over two runs, no bug found | gap audit |

## What the audit found, and what the retrofit does

1. No library bug. Keep the code as it is (D2).
2. The README says comments pass through; a `//` comment swallows the next token. Reword (D12).
3. The README's "try it live" sentence names a site tool that does not use this package. Remove it (D12).
4. The CHANGELOG's 2.0.0 entry names the wrong escape for quotes and lists two of about fifteen serializer changes; nothing says 1.x read dates as local time. Correct it and write the measured upgrade story (D13).
5. `ToJson` on .NET Framework writes doubles with 17 significant digits and -0.0 as 0; say so in the README (D12).
6. The release path, pins, settings and docs gaps of the gap table (D6 to D11, D14, D15).

## Decisions (recommendation first; the maintainer rules in the plan review, silence means the recommendation stands)

| # | Question | Recommendation | Why | Alternative |
|---|---|---|---|---|
| D1 | Compatibility promise | Every recorded 3.0.1 answer stays the same on its runtime: `tests/Golden/3.0.1.net48-windows.json` on net48 (64-bit, like the capture), `3.0.1.net10.0-windows.json` on net10.0 (Windows, Linux, macOS). One named exception class: an exception marked as worded by System.Text.Json is compared by type and parameter only when the running System.Text.Json is not 10.0.12 (a later .NET 10 patch on a runner). `PublicApi-3.0.1.txt` lines all present; package validation against 3.0.1 | The recording is the contract; runtime patches move System.Text.Json's messages, not this library's behaviour | Exact everywhere (CI breaks on the first .NET 10 patch that rewords a message) |
| D2 | Library code changes | None. The csproj gains only the version, the baseline and release notes | The capture found no bug | Fix the `//` comment behaviour: an output change, so a new option or 4.0, not a retrofit |
| D3 | Edge behaviours | Keep all as recorded and document them (wiki and README): comments, blank input judged by `char.IsWhiteSpace` while only four whitespace characters are dropped, a leading BOM copied, partial writer output before a FormatException | A formatter, not a validator; callers may rely on each | Change any of them under a new name |
| D4 | Release | 3.0.2 after a 3.0.2-beta.1 rehearsal: README and CHANGELOG corrections, baseline 3.0.1, and the first run of release.yml and of the moved publishing policy | nuget.org shows the README from the package; release.yml must be proven before a real change depends on it | No release: the fixes stay on GitHub and the wiki, and nuget.org keeps the wrong README |
| D5 | Package validation baseline | 3.0.1 | The last published stable | None |
| D6 | Workflows | ci.yml, release.yml and verify-published.yml from the templates, actions pinned to SHAs, `persist-credentials: false`, the publish job removed from ci.yml, a final `ci` job, the NuGet audit step, the package content and dependency check, consumers of the packed package; the dorny/test-reporter job dropped (a third-party action with checks set to write; the trx files stay as run artifacts) | The skill's CI-proven set, second retrofit | Keep the test-report job, SHA-pinned |
| D7 | Trusted Publishing policy | You edit the existing policy on nuget.org after the merge and before the beta tag: Workflow File `ci.yml` becomes `release.yml`; owner m4bwav, repository DotNetJsonPrettyPrinter, environment `nuget`, package JsonPrettyPrinter unchanged | The policy names one workflow file | A second policy for release.yml, then delete the old one |
| D8 | Tests added | `tests/JsonPrettyPrinterPlus.GoldenTests` (NUnit, like the unit tests and the template; the capture's Cases.cs and Json.cs by link; net10.0 and net48 win-x64); a PublicApi test; `tests/consumers` from the template (net10.0 and net48, source-mapped). The upgrade recordings are evidence only, never replayed | The replay and API list are the contract | xunit.v3 for the new project (two frameworks in one repository) |
| D9 | Layout | Keep `JsonPrettyPrinterPlus/`, `JsonPrettyPrinterPlusTests/` and `benchmarks/` where they are; new test projects under `tests/` | Moving them breaks history and links for no caller benefit | src/ and tests/ like the template |
| D10 | Build settings | A root Directory.Build.props with the template's audit-pipeline switches and what the projects do not already set; the library's and tests' analyzer settings stay as they are | New analyzer rules could demand code edits (D2) | The full template props, moving settings out of the csproj files |
| D11 | Dependabot | The template: nuget, github-actions and dotnet-sdk weekly, grouped, seven-day cooldown on every ecosystem; the library's declared floors (System.Memory, System.Text.Json) ignored so no bot raises a caller's floor | Current file has no cooldown, no SDK updates | Keep the current file |
| D12 | README | Add the downloads badge (three live badges); reword "Not a validator" to say a `//` comment swallows what follows on its line; remove the "try it live" sentence and keep the article link; a line under Serialising about doubles on .NET Framework; a short "Upgrading from 1.x" with the three biggest changes and a link to the wiki page; a link to the wiki | Every claim true on nuget.org's page | Only the two known corrections |
| D13 | CHANGELOG | A 3.0.2 section: no library change; the measured upgrade story from 1.0.1.1 (printing, ToJson, DeserializeFromJson, .NET Core) and from 2.1.1 (line endings only); the 2.0.0 entry corrected in place with a dated note | The history most callers need was never written | A separate UPGRADING.md |
| D14 | SECURITY.md and AGENTS.md | SECURITY.md from the template (private reporting; supported: 3.x); AGENTS.md release section rewritten for release.yml plus the template's rules | Missing or out of date | None |
| D15 | GitHub settings | In the one question below, applied before the pull request goes up | L-077 | |
| D16 | Old versions on nuget.org | You deprecate 1.0.0, 1.0.1 and 2.0.0 as Legacy (fields below); 1.0.1.1 stays undeprecated as the only build for .NET Framework 3.5 to 4.6.1, as RandomNameGeneratorLibrary 1.2.2 was ruled; 2.1.1 and 3.x stay | 1.0.0 and 1.0.1 answer every case as 1.0.1.1 does; 2.0.0 carries two bugs 2.1.1 fixed | Deprecate 1.0.1.1 too (message below), or deprecate nothing |
| D17 | Dependents | The markdavidrogers.com site formats JSON with System.Text.Json, not this package, so nothing depends on it that we know of | | |

### D16 fields (nuget.org, Manage package, Deprecation; one entry per group)

- Versions 1.0.0 and 1.0.1. Reason: "This package is legacy and is no longer maintained". Alternate package: none. Message: "Superseded by 1.0.1.1, which behaves the same, and by 3.x. For .NET Framework 4.6.2 or later, .NET Core and .NET 5 or later, use JsonPrettyPrinter 3.0.2 or later. Upgrade notes: https://github.com/m4bwav/DotNetJsonPrettyPrinter/wiki/Versions-and-Upgrading"
- Version 2.0.0. Reason: Legacy. Alternate package: none. Message: "An escaped backslash before a closing quote stops the formatting of the rest of the document, and empty objects print on two lines; 2.1.1 fixed both. Use JsonPrettyPrinter 3.0.2 or later (lines end with LF; 2.1.1 keeps Environment.NewLine). Upgrade notes: https://github.com/m4bwav/DotNetJsonPrettyPrinter/wiki/Versions-and-Upgrading"
- Only if you choose the alternative for 1.0.1.1. Reason: Legacy. Message: "Targets .NET Framework 3.5. Its ToJSON and DeserializeFromJson need System.Web.Extensions and fail on .NET Core and .NET 5 or later, and an escaped backslash before a closing quote stops the formatting of the rest of the document. On .NET Framework 4.6.2 or later and every modern .NET, use JsonPrettyPrinter 3.0.2 or later. Upgrade notes: https://github.com/m4bwav/DotNetJsonPrettyPrinter/wiki/Versions-and-Upgrading"

## The one question for the maintainer (GitHub writes and nuget.org actions)

1. Rulings on D1 to D17 (silence keeps the recommendations).
2. GitHub settings to apply through `gh` before the pull request goes up: master ruleset (deletion and non-fast-forward blocked, required check `ci`, admin bypass); tag ruleset (only admins create, update or delete tags); secret scanning, push protection, private vulnerability reporting and Dependabot security updates on; default workflow permissions read and no pull-request approvals by Actions; delete branches on merge.
3. Repository homepage: from the 2014 article on markdavidrogers.com to https://www.nuget.org/packages/JsonPrettyPrinter (the overlay's default)?
4. Deletions on GitHub: branches `modernize-net10` (merged in PR #2) and `release-2.1.0` (its commit stays reachable through the v2.1.1 tag); the tags `v2.1.0` and `v3.0.0`, which point at commits that never published (recommended: keep them, the CHANGELOG explains them; deleting a tag others may have fetched gains little).
5. A GitHub Release for v2.0.0 (the only published version without one), notes from the CHANGELOG and the nupkg from nuget.org attached?
6. On nuget.org, by you: the policy edit of D7 (after the merge, before the beta tag) and the deprecations of D16 (any time).
7. The wiki: its update (Phase 7) comes after 3.0.2 is verified; nothing needed from you there, since the wiki repository exists.

## Build and package specifics

Unchanged: targets, metadata, icon, README packing, SourceLink and symbols. Changed: the version (3.0.2-beta.1, then 3.0.2), the `PackageValidationBaselineVersion` property (3.0.1), release notes. New: Directory.Build.props (D10), `tests/JsonPrettyPrinterPlus.GoldenTests`, `tests/consumers`, release.yml, verify-published.yml, SECURITY.md. The golden folder `tests/Golden` stays byte-identical to commit 08b777a.

## Phases

### Phase 0: survey, baseline, golden capture (2026-09-28, no library code changed)
- [x] Survey into [../notes/2026-09-28-survey.txt](../notes/2026-09-28-survey.txt); every version's files read
- [x] Baseline: locked restore, format, build (0 warnings), 37 of 37 tests per runtime, pack
- [x] Golden capture of the published 3.0.1 on net48 and net10.0, twice each, identical; public API list; commit 08b777a
- [x] Upgrade recordings of 2.1.1 and 1.0.1.1 on both runtimes, compare reports; 1.0.0, 1.0.1 and 2.0.0 run in scratch
- [x] everlast (mode repo, sync push), AGENTS.md block, lint clean; commit 366b93a
- [x] Gap audit [../notes/2026-09-28-phase-0-gap-audit.md](../notes/2026-09-28-phase-0-gap-audit.md)
### Phase 1: plan
- [x] This plan and the decision record [../decisions/2026-09-28-retrofit-without-code-changes.md](../decisions/2026-09-28-retrofit-without-code-changes.md). **Stop** for the rulings and the one question.
### Phase 2: retrofit on branch v3-retrofit
- [ ] Golden replay project first, green on the first build; canary after committing (a planted line in the engine turns it red, reverted, green; both logged); `git diff --exit-code 08b777a -- tests/Golden` empty
- [ ] Templates adapted (then grep for `{{[A-Z_]+}}` and `TEMPLATE`); workflows, Dependabot, Directory.Build.props, consumers, SECURITY.md, AGENTS.md, README, CHANGELOG
- [ ] actionlint and zizmor clean; each new CI check run locally with `bash -e -o pipefail` on the real nupkg, with a right and a wrong expectation (L-103); CI's exact test and pack commands locally (L-101); a fresh clone
- [ ] `check-readme-images.mjs README.md --registry nuget` exit 0
- [ ] Pushed; pull request with a "For review" list
### Phase 3: review
- [ ] Independent review (prompts/review-subagent.md) with a differential of the new build against the published 3.0.1 over generated JSON inputs on both runtimes; findings fixed or answered
- [ ] Rulesets and security settings applied (per the answer to question 2). **Stop** for the pull request review.
### Phase 4: CI, merge, cleanup
- [ ] CI green; merge read back (method and SHA); cleanup per question 4
### Phase 5: rehearsal
- [ ] Policy moved to release.yml (you); 3.0.2-beta.1 tagged after master is green; approval (you); verify-published on three OSes
### Phase 6: release
- [ ] CHANGELOG dated; 3.0.2 tagged after green; approval (you); verify-published; GitHub Release; README check on the nuget.org copy; baseline bump to 3.0.2
### Phase 7: wrap-up
- [ ] Wiki with wikiwright Update mode (saved output first, then 3.0.2 and the upgrade story), live check
- [ ] HANDOFF, inventory row 5, the kickoff's corrections, lessons into both skills

## Test strategy

| Layer | What it proves | How | Runs where |
|---|---|---|---|
| Golden replay | Every 3.0.1 answer kept | Cases.cs compiled unchanged against the project reference, compared as parsed JSON with the recording of the same runtime | net10.0 on Ubuntu and Windows, net48 (win-x64) on Windows |
| Public API | No name or parameter lost | Every line of PublicApi-3.0.1.txt present in a reflection listing of the built assembly | net10.0 and net48 |
| Package validation | Binary compatibility with 3.0.1 | `dotnet pack` with the baseline | pack step |
| Unit tests | The existing 37 | as today | both |
| Package shape | lib per target, README, icon, dependencies per group | the template's content check on the nupkg | Ubuntu |
| Consumers | The packed package restores and answers | tests/consumers/run.sh, net10.0 and net48 | Ubuntu and Windows |
| Registry | The published package | verify-published.yml | three OSes |

## Security

No leaked credentials (no secrets outside the environment's NUGET_USER). Workflows: contents read by default; id-token set to write only in the push job, which checks nothing out and runs first-party actions only; the GitHub Release in its own job; SHA pins; zizmor. Publishing: Trusted Publishing bound to release.yml and environment nuget with your approval; no API key stored. The library reads no files, makes no requests, uses no reflection outside the serializer helpers.

## Badges and images: disposition

| Image or badge | What it shows now | Decision | New URL or reason |
|---|---|---|---|
| NuGet version (shields.io) | 3.0.1, live | keep | |
| CI (GitHub Actions ci.yml) | passing, live | keep | |
| Downloads | missing | add | https://img.shields.io/nuget/dt/JsonPrettyPrinter.svg |

## Verification checklist

| Claim | Command or place | Expected |
|---|---|---|
| Golden files untouched | `git diff --exit-code 08b777a -- tests/Golden` | empty |
| Old behaviour kept | the golden replay in `dotnet test` | every case, both runtimes |
| API kept | PublicApi test; package validation | pass; no CP errors |
| Restores clean, formatted, built | `dotnet restore --locked-mode`, `dotnet format --verify-no-changes`, `dotnet build -c Release` | exit 0, no warnings |
| Workflows | actionlint, `uvx zizmor --offline .` | clean |
| README images | `node scripts/check-readme-images.mjs README.md --registry nuget` | "checked for nuget", exit 0 |
| On nuget.org | verify-published run | green on three OSes |
| Release | `gh release view v3.0.2` | notes and nupkg |

## Risks and open points

- A .NET 10 patch on a runner may reword a System.Text.Json message before the replay's version check is proven; D1 handles it.
- The first release.yml run may fail on the policy (still bound to ci.yml) if the edit is missed; the push job's login shows it.

## Next single action

Wait for the maintainer's rulings on D1 to D17 and the answers to the one question.
