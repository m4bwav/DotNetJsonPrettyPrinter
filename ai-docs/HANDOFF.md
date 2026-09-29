# Handoff

Updated 2026-09-28, evening. Read this first, then the plan and [log.md](log.md).

## Current state

- 3.0.1 is the release on nuget.org. Pull request #8 (the retrofit, no library code change) merged on 2026-09-28 as 57fbcd3; 3.0.2-beta.1 released through release.yml and verified from nuget.org on three OSes.
- Plan: [plans/2026-09-28-retrofit-and-3.0.2-release.md](plans/2026-09-28-retrofit-and-3.0.2-release.md). Rulings: "Do all the recommendations and record learnings, run all the commands you want" (every recommendation stands).
- Done and read back: golden capture (08b777a) and replay (1174 cases per runtime, canary seen red), workflows from the templates, rulesets 24143846 (master, required check `ci`) and 24143848 (tags, admins only), security settings, homepage, delete-on-merge, a GitHub Release for v2.0.0.
- Independent review done: 525,778 comparisons per runtime against 3.0.1, 0 differences; its 9 findings fixed or answered (log.md, and the comment on the pull request). CI green on 637c6c3 (run 36490775529).

## Next single action

3.0.2-beta.1 is verified (log.md, 2026-09-29). Continue on branch `release-3.0.2` (pushed, holds only these docs):

1. Move the 3.0.2-beta.1 CHANGELOG content under `## [3.0.2] - YYYY-MM-DD` (the beta section then says it rehearsed 3.0.2), set `<Version>3.0.2</Version>`, fix the compare links; pull request, merge, `ci` green on master, tag `v3.0.2` on that commit, the maintainer approves, `gh workflow run verify-published.yml -f version=3.0.2`, `gh attestation verify --format json`, `check-readme-images.mjs --registry nuget` on the README inside the nuget.org package; then a pull request raising `PackageValidationBaselineVersion` to 3.0.2.
2. Deprecations on nuget.org (the maintainer; fields in the plan under "D16 fields"): 1.0.0, 1.0.1 and 2.0.0 as Legacy. Log only what the maintainer says.
3. The wiki with wikiwright Update mode ([notes/2026-09-28-github-wiki.md](notes/2026-09-28-github-wiki.md)): step 1 is done (the saved `notes/2026-09-28-wiki-verify.out.txt`); mark the input and output pairs (wikiwright L-103), fold the two unsaved runs into the program (L-104), bump to 3.0.2, put the measured upgrade story on Versions and upgrading and the .NET Framework doubles on Serialisation helpers, push, `wikiwright.py check` and `live`.
4. A Dependabot pull request for the .NET SDK bump (dotnet-sdk 10.0.401) is open with ci green: merge it.

## Skill work in flight (other repositories)

- package-modernize, branch `jpp-retrofit-lessons` (pushed, no pull request yet): L-110 to L-119, template fixes, references/retrofit.md corrections (second run) and dated references/nuget.md traps are committed. Still to do there: make the Phase 7 and wikiwright hand-over wording identical in both skills (retrofit or not, release or not, the golden capture feeding Versions and upgrading), a CHANGELOG entry, then the pull request.
- wikiwright, branch `jpp-wiki-lessons` (pushed): L-103 to L-105. Still to do: the `outputs` helper fix with unit tests, a C# note in references/nuget.md, CHANGELOG and a patch release (tag and GitHub Release); HANDOFF open item 1 (retarget the eval action cases to is-an-image-url, re-run, record in TESTS.md).
- package-modernization records (private repository): the kickoff prompt for this run (status line and "What the run found wrong"), inventory row 5, the Wikis table row (now wikiwright-maintained), by pull request.
- Evergreen upkeep the SessionStart hook asked for: `evergreen` has one verify-at-use claim due (`evergreen.py claims evergreen --due`, then `--stamp due`).

## Where things are

- Golden recordings and capture: `tests/Golden/` (README there); never edit. Replay: `tests/JsonPrettyPrinterPlus.GoldenTests`.
- Gap audit: [notes/2026-09-28-phase-0-gap-audit.md](notes/2026-09-28-phase-0-gap-audit.md). Survey: [notes/2026-09-28-survey.txt](notes/2026-09-28-survey.txt).
- Wiki working copy: `D:\m4bwa\Claude\Projects\Ai\labs\DotNetJsonPrettyPrinter.wiki` (master, c6b285c, unchanged so far).
