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

## [2026-09-28] add | Phase 2 pushed, pull request #8, GitHub settings applied before the review stop
- Fresh clone of v3-retrofit (613cc69): locked restore, build 0 warnings, 43 tests on net10.0 and 43 on net48; run.sh executable.
- Pull request https://github.com/m4bwav/DotNetJsonPrettyPrinter/pull/8 opened with a For review list. Independent review started in the background (prompts/review-subagent.md, with a differential against the published 3.0.1 on both runtimes).
- Rulesets: 24143846 master (deletion, non-fast-forward, required check ci, admin bypass; copied from DotNetRandomNameGenerator) and 24143848 Tags only by admins (templates/rulesets). Read back: both active.
- Read back: secret scanning and push protection enabled, private vulnerability reporting enabled, Dependabot security updates enabled, default workflow permissions read with no pull-request approvals, delete branch on merge true, homepage https://www.nuget.org/packages/JsonPrettyPrinter.

## [2026-09-28] add | GitHub Release v2.0.0 created (plan question 5)
- gh release create v2.0.0 with the corrected CHANGELOG section and a note, the nupkg and snupkg downloaded from nuget.org attached, not marked latest: https://github.com/m4bwav/DotNetJsonPrettyPrinter/releases/tag/v2.0.0 (read back with gh release list: v3.0.1 still Latest).

## [2026-09-28] update | CI run 36489418546 on pull request #8: Windows green, Ubuntu red in one golden case
- tojson.options | write-indented-not-pretty: System.Text.Json WriteIndented writes Environment.NewLine; recorded CRLF on Windows, LF on Linux. Not a library change. Fixed with a second named exception keyed to that one case (off Windows, the recorded CRLF read as LF) and a test that the key exists and holds a CRLF. Local: 7 golden tests pass on net10.0 and net48, format clean. Listed on the pull request for the maintainer's review, since the ruled plan named only the message exception.

## [2026-09-28] verify | CI run 36489822416 on pull request #8: build and test green on ubuntu-24.04 and windows-latest, ci green
- Wiki Update mode, step 1 done early (it needs only 3.0.1): notes/2026-09-28-wiki-verify.cs run twice against 3.0.1 from the scratchpad, identical, 354 lines; saved LF as notes/2026-09-28-wiki-verify.out.txt. wikiwright.py outputs: 6 findings, all input blocks (wikiwright L-103). Wiki not yet changed.

## [2026-09-28] add | Independent review of pull request #8 (at 511b40d) and the fixes
- Differential of the packed 3.0.2-beta.1 against 3.0.1 from nuget.org over 25,037 generated inputs per runtime: 525,778 comparisons each on net10.0, net48 and net8.0, 0 differences; nupkg contents and nuspec dependencies identical to 3.0.1 apart from nuget.org's signature. Harness in the session scratchpad (review/diff).
- 1 (risk, fixed): release.yml pushed a nupkg nothing had checked. Now: ci must have passed on the tagged SHA (check-runs API, checks read), the build job runs the content check and consumers on the package it uploads, test-windows needs build and runs the consumers (net10.0, net48) on the downloaded release artifact; ci.yml cancels in-progress runs only for pull requests. The content check moved to tests/package/check-contents.sh (shared); verified right (exit 0) and on a package without its icon (exit 1 with its error).
- 2 (risk, fixed): ci.yml runs git diff --exit-code 08b777a -- tests/Golden.
- 3 (risk, fixed): the lenient branch also compares inner exception types and the Path part; a test warns while it is active; the weak test renamed.
- 4 (nit, answered): capture gaps (options with span and writer overloads, Equals(object), depth over 64, record operators in the API list) are covered by the differential with 0 differences; the capture stays frozen.
- 5 to 8 (nits, fixed): README comments and errors wording; CHANGELOG: two loose comparisons named, 1.0.0 and 1.0.1 measured on .NET Framework, the 2.1.1 sentence names IndentSize = int.MaxValue and the removed APIs.
- 9 (nit, fixed): release.yml comment and plan: attest also holds id-token; nuget.org accepts only the push job because the policy names the nuget environment (keep it in the D7 edit).
- Not taken: net8.0 consumers in run.sh (the differential ran net8.0 with 0 differences; .NET 8 leaves support 2026-11-10).
- Local after the fixes: build 0 warnings, 8 golden tests per runtime, format clean, actionlint with shellcheck clean, zizmor clean, tests/Golden untouched.

## [2026-09-28] update | Wiki brought under wikiwright's saved-output rule for 3.0.1
- notes/2026-09-28-wiki-verify.cs run against 3.0.1: 354 lines, identical to the saved output. wikiwright 0.3.0's `outputs` (fixed for input and output pairs, code followed by output and values in comments) then found 23 page outputs and 11 missing: 9 never printed by the program, all of them matching the page once printed, and 2 commands in untagged fences.
- The program now prints the 9 and runs the pages' PowerShell and F# snippets as written (pwsh 7.6.6, dotnet fsi); output 446 lines, saved LF. The two command fences are tagged `sh` and `bat` on the wiki. `outputs`: 21 checked, 0 missing; `check`: 0 errors. The note's update procedure now diffs against the saved output and runs `outputs`.

## [2026-09-28] update | Maintainer on the nuget.org policy
- Maintainer: "ok, i updated nuget" (after asking what to put for the glob pattern; the answer given was JsonPrettyPrinter, workflow release.yml, environment nuget). Unconfirmed until the 3.0.2-beta.1 push job signs in through NuGet/login.

## [2026-09-28] add | Pull request #8 merged; 3.0.2-beta.1 tagged; release run waiting at the approval
- Merged 2026-09-28T22:30:37Z as 57fbcd34 (a merge commit, two parents, not a squash). ci on master 57fbcd3: run 36492819192 success.
- Deleted (ruled): branches modernize-net10 and release-2.1.0; branches now: master and a Dependabot dotnet-sdk branch. Tags v2.1.0 and v3.0.0 kept.
- Tag v3.0.2-beta.1 on 57fbcd3 pushed and read back with ls-remote.
- release run 36512903672: build (tag, master, ci on the SHA, content check, consumers) success; test-windows with consumers on the package to push success; attest success; push job waiting for the nuget environment approval.
- A wait loop in this session polled a null run id for ten minutes (gh run list -b master returned a run without an id); stopped. Wait loops exit on an empty id or a gh error.

## [2026-09-29] verify | 3.0.2-beta.1 approved, pushed and verified from nuget.org
- Maintainer: "i approved".
- release run 36512903672: all five jobs success; the push job signed in through NuGet/login and pushed, which confirms the maintainer's policy edit to release.yml. GitHub Release v3.0.2-beta.1 (prerelease) with the nupkg and snupkg.
- gh attestation verify on the release artifact (--format json): workflow .github/workflows/release.yml, ref refs/tags/v3.0.2-beta.1, exit 0.
- verify-published run 36513697039 (version 3.0.2-beta.1): ubuntu-24.04, windows-latest, macos-latest all success (both indexes, repository signature, consumers from nuget.org).

## [2026-09-29] update | Maintainer: "do everything but 2"
- Read as: release 3.0.2, the wiki update, merge the Dependabot SDK pull request, the two skill pull requests, the records and the evergreen upkeep; not the nuget.org deprecations (item 2), which stay with the maintainer.

## [2026-09-29] add | 3.0.2 released; the first verify-published timed out on nuget.org's registration index
- Pull request #9 (Dependabot, global.json SDK 10.0.401) merged as 0833ee7; pull request #10 (3.0.2: version, dated CHANGELOG) merged as a87aae5 after ci; ci on master a87aae5 success; tag v3.0.2 pushed and read back.
- Maintainer: "I approved do the rest". release run 36514934679: all five jobs success, push through Trusted Publishing, GitHub Release v3.0.2. gh attestation verify (--format json): .github/workflows/release.yml at refs/tags/v3.0.2, exit 0.
- verify-published run 36516856747 failed on all three OSes at the index wait: the flat container listed 3.0.2, the registration index listed 3.0.2 only at 03:44Z, about 23 minutes after the push, just past the 20-minute wait (skill L-122; an earlier message in the session said over 40 minutes, which was wrong). The wait is now 60 minutes (verify-published.yml in this repository and the template); rerun after the index lists it.
- Baseline raised to 3.0.2 on branch docs-after-3.0.2 (pack needs the registration index too).

## [2026-09-29] verify | 3.0.2 verified from nuget.org; wiki updated for 3.0.2
- The registration index listed 3.0.2 at 03:44Z. verify-published run 36518499591: ubuntu-24.04, macos-latest, windows-latest success.
- check-readme-images.mjs --registry nuget on the README inside the nuget.org 3.0.2 package: 3 images ok; the file equals the repository's README.
- Wiki: program bumped to 3.0.2, two identical runs equal to the saved 3.0.1 output (446 lines); outputs 21 checked, 0 missing; check 0 errors; pushed as wiki cce45ce; live 11 pages, 0 failures. Details in notes/2026-09-28-github-wiki.md.
