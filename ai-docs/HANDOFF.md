# Handoff

Updated 2026-09-28, evening. Read this first, then the plan and [log.md](log.md).

## Current state

- 3.0.1 is the release on nuget.org. The package-modernize retrofit is at the **pull-request stop**: https://github.com/m4bwav/DotNetJsonPrettyPrinter/pull/8 (branch `v3-retrofit`, version 3.0.2-beta.1, no library code change).
- Plan: [plans/2026-09-28-retrofit-and-3.0.2-release.md](plans/2026-09-28-retrofit-and-3.0.2-release.md). Rulings: "Do all the recommendations and record learnings, run all the commands you want" (every recommendation stands).
- Done and read back: golden capture (08b777a) and replay (1174 cases per runtime, canary seen red), workflows from the templates, rulesets 24143846 (master, required check `ci`) and 24143848 (tags, admins only), security settings, homepage, delete-on-merge, a GitHub Release for v2.0.0.
- The independent review ran on the pull request; its findings and answers are in log.md (the last entries).

## Next single action

The maintainer reviews and merges pull request #8. Then, in order (plan, Phases 4 to 7):

1. Read the merge back (`gh pr view 8 --json mergeCommit,mergedAt`); delete branches `modernize-net10` and `release-2.1.0` (ruled; `release-2.1.0` is the v2.1.1 tag's commit, which the tag keeps). Keep tags v2.1.0 and v3.0.0.
2. The maintainer edits the nuget.org Trusted Publishing policy: Workflow File `ci.yml` becomes `release.yml` (owner m4bwav, repository DotNetJsonPrettyPrinter, environment `nuget`). Log only what the maintainer says, quoted; the beta's push job proves it.
3. After `ci` is green on master: tag `v3.0.2-beta.1` on that commit, push the tag, wait at the `nuget` approval (the maintainer clicks Review deployments), then `gh workflow run verify-published.yml -f version=3.0.2-beta.1` (three OSes), `gh attestation verify` with `--format json`.
4. Release 3.0.2: a small pull request that moves the 3.0.2-beta.1 CHANGELOG content under a dated `## [3.0.2] - YYYY-MM-DD` heading (the beta section then says it rehearsed 3.0.2), sets `<Version>3.0.2</Version>` and the compare links; merge, green, tag `v3.0.2`, approval, verify-published, `check-readme-images.mjs --registry nuget` on the README inside the nuget.org package, then a pull request raising `PackageValidationBaselineVersion` to 3.0.2.
5. Deprecations on nuget.org (the maintainer, any time; exact fields in the plan under "D16 fields"): 1.0.0, 1.0.1 and 2.0.0 as Legacy; 1.0.1.1 stays.
6. The wiki with wikiwright's Update mode ([notes/2026-09-28-github-wiki.md](notes/2026-09-28-github-wiki.md)). Already done: `notes/2026-09-28-wiki-verify.cs` run twice against 3.0.1 (identical), output saved as `notes/2026-09-28-wiki-verify.out.txt`. `wikiwright.py outputs` gave 6 findings, all input blocks on Home and Not-a-Validator; the helper misses the output block that follows an input block (wikiwright L-103), so mark those pairs with `outputs: skip` and `outputs: check` comments (or fix the helper first), fold the two unsaved runs into the program (wikiwright L-104), then bump to 3.0.2 and put the measured upgrade story (`tests/Golden/upgrade/diff-*.txt`, CHANGELOG 3.0.2-beta.1) on Versions and upgrading, and the .NET Framework double formatting on Serialisation helpers. Push, `wikiwright.py check` and `live`.

## Skill work in flight (other repositories)

- package-modernize, branch `jpp-retrofit-lessons` (pushed, no pull request yet): L-110 to L-119 and template fixes. Still to do there: references/retrofit.md corrections (second run: the capture of old versions with one program, the ai-docs conversion recipe, homepage and never-published tags in the gap table, the dorny job, a props file limited to the audit switches when the csproj files carry the settings, the OS-newline check), references/nuget.md traps with the date, the Phase 7 and wikiwright hand-over wording made identical in both skills, CHANGELOG entry, then the pull request.
- wikiwright, branch `jpp-wiki-lessons` (pushed): L-103 to L-105. Still to do: the `outputs` helper fix with unit tests, a C# note in references/nuget.md, CHANGELOG and a patch release (tag and GitHub Release); HANDOFF open item 1 (retarget the eval action cases to is-an-image-url, re-run, record in TESTS.md).
- package-modernization records (private repository): the kickoff prompt for this run (status line and "What the run found wrong"), inventory row 5, the Wikis table row (now wikiwright-maintained), by pull request.
- Evergreen upkeep the SessionStart hook asked for: `evergreen` has one verify-at-use claim due (`evergreen.py claims evergreen --due`, then `--stamp due`).

## Where things are

- Golden recordings and capture: `tests/Golden/` (README there); never edit. Replay: `tests/JsonPrettyPrinterPlus.GoldenTests`.
- Gap audit: [notes/2026-09-28-phase-0-gap-audit.md](notes/2026-09-28-phase-0-gap-audit.md). Survey: [notes/2026-09-28-survey.txt](notes/2026-09-28-survey.txt).
- Wiki working copy: `D:\m4bwa\Claude\Projects\Ai\labs\DotNetJsonPrettyPrinter.wiki` (master, c6b285c, unchanged so far).
