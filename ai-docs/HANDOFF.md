# Handoff

Updated 2026-09-28, evening. Read this first, then the plan and [log.md](log.md).

## Current state

- 3.0.1 is the release on nuget.org. Pull request #8 (the retrofit, no library code change) merged on 2026-09-28 as 57fbcd3; 3.0.2-beta.1 released through release.yml and verified from nuget.org on three OSes.
- Plan: [plans/2026-09-28-retrofit-and-3.0.2-release.md](plans/2026-09-28-retrofit-and-3.0.2-release.md). Rulings: "Do all the recommendations and record learnings, run all the commands you want" (every recommendation stands).
- Done and read back: golden capture (08b777a) and replay (1174 cases per runtime, canary seen red), workflows from the templates, rulesets 24143846 (master, required check `ci`) and 24143848 (tags, admins only), security settings, homepage, delete-on-merge, a GitHub Release for v2.0.0.
- Independent review done: 525,778 comparisons per runtime against 3.0.1, 0 differences; its 9 findings fixed or answered (log.md, and the comment on the pull request). CI green on 637c6c3 (run 36490775529).

## Next single action

State on 2026-09-29: pull request #10 (3.0.2) merged as a87aae5, ci green, tag v3.0.2 pushed, release run 36514934679 waiting for the maintainer's approval. Dependabot's SDK pull request #9 merged (0833ee7).

1. After the approval: `gh workflow run verify-published.yml -f version=3.0.2` (three OSes), `gh attestation verify` on the release artifact with `--format json`, `check-readme-images.mjs --registry nuget` on the README inside the nuget.org 3.0.2 package; then a pull request raising `PackageValidationBaselineVersion` to 3.0.2.
2. The wiki: the 3.0.2 edits are made but not committed in `D:\m4bwa\Claude\Projects\Ai\labs\DotNetJsonPrettyPrinter.wiki` (Versions and upgrading with the measured story, Serialisation helpers with .NET Framework numbers, Development with the new release path, version lines). Bump `notes/2026-09-28-wiki-verify.cs` to 3.0.2, run it from a scratch folder twice, diff with the saved output, save the new output, run `wikiwright.py outputs`, change Recipes' "produced with 3.0.1" line, commit and push the wiki, `wikiwright.py check` and `live`, update the wiki note.
3. Records (package-modernization, by pull request): prompts/2026-09-28-jsonprettyprinter-retrofit-kickoff.md is written (status line to finish), inventory row 5, the Wikis row.
4. The deprecations stay with the maintainer ("do everything but 2").

## Skill work

Done on 2026-09-28: package-modernize #13 merged (L-110 to L-121, templates, retrofit.md, nuget.md, the hand-over paragraph, C-20260928-7); wikiwright #1 merged (the same paragraph, C-20260928-5). wikiwright 0.3.0 (another session) carries L-103 to L-105 and the outputs fix. Evergreen upkeep: evergreen claims 0 of 4 due; ai-docs-capture shows overdue but was replaced by everlast-capture (report, do not refresh).

## Where things are

- Golden recordings and capture: `tests/Golden/` (README there); never edit. Replay: `tests/JsonPrettyPrinterPlus.GoldenTests`.
- Gap audit: [notes/2026-09-28-phase-0-gap-audit.md](notes/2026-09-28-phase-0-gap-audit.md). Survey: [notes/2026-09-28-survey.txt](notes/2026-09-28-survey.txt).
- Wiki working copy: `D:\m4bwa\Claude\Projects\Ai\labs\DotNetJsonPrettyPrinter.wiki` (master, c6b285c, unchanged so far).
