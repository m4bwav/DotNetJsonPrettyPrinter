# Handoff

Updated 2026-09-28. Read this first, then [log.md](log.md) for evidence.

## Current state

- 3.0.1 is the current release on nuget.org (2.1.1 is the last 2.x; the v2.1.0 and v3.0.0 tags never published).
- The package-modernize retrofit is at its Phase 1 stop on branch `v3-retrofit` (pushed): golden capture of the published 3.0.1 committed as 08b777a, ai-docs converted to everlast (366b93a), gap audit and plan written. Nothing in the library has changed.
- Plan: [plans/2026-09-28-retrofit-and-3.0.2-release.md](plans/2026-09-28-retrofit-and-3.0.2-release.md). Findings: [notes/2026-09-28-phase-0-gap-audit.md](notes/2026-09-28-phase-0-gap-audit.md).

## Waiting for

The maintainer's rulings on D1 to D17 and the one question in the plan (GitHub settings, homepage, deletions, a v2.0.0 Release, the nuget.org policy edit and deprecations).

## Next single action

After the rulings: Phase 2 on `v3-retrofit`, golden replay project first (plan, Phases).

## Standing work

- The README and CHANGELOG corrections and the 3.0.1 baseline ride with this retrofit (plan D12, D13, D5).
- The wiki (11 pages by hand, `D:\m4bwa\Claude\Projects\Ai\labs\DotNetJsonPrettyPrinter.wiki`) gets wikiwright's Update mode after the release: run `notes/2026-09-28-wiki-verify.cs`, save its output, fix every `outputs` finding, then the 3.0.2 changes and the measured upgrade story ([notes/2026-09-28-github-wiki.md](notes/2026-09-28-github-wiki.md)).
- Dependabot pull requests arrive weekly.

## Where things are

- Golden recordings and the capture: `tests/Golden/` (its README says how to re-run). Never edit them.
- Release procedure today: AGENTS.md (the publish job in ci.yml); release.yml replaces it in this retrofit.
