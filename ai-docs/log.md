# Log

Append-only. One line per operation: `## [YYYY-MM-DD] op | title` where op is one of add, update, supersede, verify, verify-failed, prune, handoff, index. Newest at the bottom. Never edited, only appended; this is the history the entries themselves do not carry.

## [2026-09-28] init | scaffolded

## [2026-09-28] add | GitHub wiki written and published for 3.0.1 (moved from the old log/ folder)
- Eleven wiki pages, a sidebar and a footer were written for https://github.com/m4bwav/DotNetJsonPrettyPrinter/wiki and pushed as wiki commit `c6b285c` from the sibling working copy `D:\m4bwa\Claude\Projects\Ai\labs\DotNetJsonPrettyPrinter.wiki`. Every example was run against the published 3.0.1 package first (the program is `ai-docs/notes/2026-09-28-wiki-verify.cs`); the repository's 37 tests pass on net10.0 and net48.
- Also confirmed today: 2.1.1 and 3.0.1 are live on nuget.org (published 2026-09-25), so the publish hand-off from the 2026-09-25 log is closed.
- Found and not fixed, because they ship inside the package and need a release: the README's claim that comments pass through (a `//` comment swallows the next token), the CHANGELOG 2.0.0 line about `\"` (System.Text.Json writes `\u0022`), the README's "try it live" sentence (the site uses System.Text.Json, not this package), and the csproj's pending `PackageValidationBaselineVersion` 3.0.1. Details, the verification facts and the update procedure: `ai-docs/notes/2026-09-28-github-wiki.md`.
## [2026-09-28] index | rebuilt (3 entries)
