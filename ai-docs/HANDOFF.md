# Handoff

Updated 2026-09-28, evening. Read this first, then the plan and [log.md](log.md).

## Current state

- 3.0.2 is the release on nuget.org (2026-09-29), released through release.yml after the verified 3.0.2-beta.1 rehearsal, verified from nuget.org on three OSes (run 36518499591), attested. No library change since 3.0.1; the golden replay guards every recorded answer.
- The package-modernize retrofit is complete: pull requests #8 (retrofit), #9 (SDK), #10 (3.0.2) and the baseline pull request from branch docs-after-3.0.2 (PackageValidationBaselineVersion 3.0.2, verify-published waits up to 60 minutes).
- The wiki describes 3.0.2 (wiki cce45ce).

## Standing work

- Owed by the maintainer: the nuget.org deprecations of 1.0.0, 1.0.1 and 2.0.0 (plan, "D16 fields"). Record them only when the maintainer says so or the registration index shows them.
- Dependabot pull requests arrive weekly (seven-day cooldown); merge when ci is green. NUnit 5 and coverlet.collector 10.1 were held by the cooldown on 2026-09-28.
- The next release: raise PackageValidationBaselineVersion after it; the README and CHANGELOG ship inside the package, so doc fixes need a version.
- .NET 8 and 9 leave support on 2026-11-10 (the targets are netstandard2.0 and net10.0, so nothing to drop); .NET 11 is expected in November 2026.

## Next single action

None owed by an agent. The next change starts from the plan's rules: the golden replay first, a named exception only with the maintainer's ruling.

## Where things are

- Golden recordings and capture: `tests/Golden/` (README there); never edit. Replay: `tests/JsonPrettyPrinterPlus.GoldenTests`.
- Gap audit: [notes/2026-09-28-phase-0-gap-audit.md](notes/2026-09-28-phase-0-gap-audit.md). Survey: [notes/2026-09-28-survey.txt](notes/2026-09-28-survey.txt).
- Wiki working copy: `D:\m4bwa\Claude\Projects\Ai\labs\DotNetJsonPrettyPrinter.wiki` (master at a8c5574, with the uncommitted 3.0.2 edits).
