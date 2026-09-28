# Handoff

## Current state

3.0.1 is the current release, live on nuget.org since 2026-09-25 (2.1.1 is the last 2.x; the `v2.1.0` and `v3.0.0` tags never published). The modernization plan (`plans/modernization-plan.md`) is complete. The GitHub wiki was written and published on 2026-09-28 (wiki commit `c6b285c`, working copy `D:\m4bwa\Claude\Projects\Ai\labs\DotNetJsonPrettyPrinter.wiki`); `notes/2026-09-28-github-wiki.md` says how to update it and re-verify its examples.

## Standing work

- With the next README change: reword "comments ... come out indented" (a `//` comment breaks the document) and the "try it live" sentence (the site's formatter is System.Text.Json, not this package). With the next CHANGELOG change: 2.0.0 escapes quotes as `\u0022`, not `\"`.
- With the next csproj change: `<PackageValidationBaselineVersion>3.0.1</PackageValidationBaselineVersion>`.
- The package-modernize retrofit of this repository (golden capture, release workflow with attestation, security settings) is still owed per the package-modernization inventory; copy the RandomNameGeneratorLibrary kickoff prompt.
- Dependabot pull requests arrive weekly.

## Next single action

Nothing is owed by the agent for the wiki. The next planned work is the package-modernize retrofit; the README and CHANGELOG fixes above ride along with it.

## Where things are

- Logs: `log/` (one file per session). Notes: `notes/`. Plan: `plans/modernization-plan.md`.
- Release procedure: `AGENTS.md` and the README's "Building and releasing".
