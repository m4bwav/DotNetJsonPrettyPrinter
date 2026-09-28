---
title: GitHub wiki written and published for 3.0.1
kind: note
date: 2026-09-28
verified: 2026-09-28
stale_after: 2027-03-28
tags: [wiki, docs, 3.0.1, github]
summary: "the eleven wiki pages, where their git working copy is, how every example was verified against the published 3.0.1 package, the facts found on the way (three README/CHANGELOG inaccuracies, the file-based app AOT trap) and how to update the wiki; read before touching the wiki, the README's 'not a validator' or 'try it live' sentences, or the CHANGELOG's 2.0.0 entry"
---

# GitHub wiki for 3.0.1

## Summary

Mark asked for the repository wiki (https://github.com/m4bwav/DotNetJsonPrettyPrinter/wiki) to be filled the way the DotNetRandomNameGenerator wiki was on the same day. Eleven pages plus sidebar and footer were written from the 3.0.1 source, README, CHANGELOG, AGENTS.md, the modernization plan and log, the CI workflow, the benchmark results and nuget.org. Every example output on the wiki was produced by running it against the published `JsonPrettyPrinter` 3.0.1 package (see "How the examples were verified"). Published on 2026-09-28 as wiki commit `c6b285c`; every page answers 200 and the sidebar and footer render. Everwrite checker: 0 strong, 9 weak (all judged fine: quoted error messages, "rather than" with both halves informative, a product name in a heading).

Pages: Home, Getting started, API reference, Output format, Not a validator, Serialisation helpers, Recipes, Performance and threading, Versions and upgrading, FAQ, Development.

## Where the pages are

`D:\m4bwa\Claude\Projects\Ai\labs\DotNetJsonPrettyPrinter.wiki` (a sibling of this clone, outside this repository), branch `master`, remote `origin` = `https://github.com/m4bwav/DotNetJsonPrettyPrinter.wiki.git`. Files: `Home.md`, `Getting-Started.md`, `API-Reference.md`, `Output-Format.md`, `Not-a-Validator.md`, `Serialisation-Helpers.md`, `Recipes.md`, `Performance-and-Threading.md`, `Versions-and-Upgrading.md`, `FAQ.md`, `Development.md`, `_Sidebar.md`, `_Footer.md`. Plain markdown links between pages (`[Recipes](Recipes)`), no wikilinks. LF line endings.

## How it was published

Unlike the RandomNameGenerator wiki, the `.wiki.git` repository already existed (Mark had saved GitHub's placeholder Home page, commit `3542d9a`), so `git ls-remote` answered at once, a normal clone worked and the pages went up with a plain `git push`. The rule stands for any other repository: run `git ls-remote https://github.com/<owner>/<repo>.wiki.git` first; if it fails, the first page has to be saved in the web UI (no API), so ask for that click early.

## Updating the wiki later

1. Edit the markdown in the working copy above. Page names are the file names with hyphens; links between pages are `[Text](Page-Name)`.
2. Re-verify the examples: bump the version in the `#:package` line of `2026-09-28-wiki-verify.cs` (next to this note), run `dotnet run 2026-09-28-wiki-verify.cs` from a directory outside the repository (copy it to a temp folder; file-based apps should not live in a project cone) and compare the output with the pages. The file carries `#:property PublishAot=false` because the serialisation half needs reflection.
3. Run the everwrite checker: `python C:\Users\m4bwa\.claude\skills\everwrite\scripts\tells.py *.md`.
4. Commit and `git push`. When a release changes the version number, the pages to touch are Home, Getting started (the `PackageReference` and `#:package` lines), Versions and upgrading, the footer, and the benchmark table on Performance and threading if the engine changed.

Writing-tool note: the harness's Write tool decodes `\u` escapes, so the pages were written with a `<BS>` placeholder for every backslash and converted with Python afterwards. Check with `grep -c '<BS>' *.md` after any edit made that way.

## How the examples were verified

A .NET 10 file-based app with `#:package JsonPrettyPrinter@3.0.1` (the copy in `2026-09-28-wiki-verify.cs`, plus two smaller runs for the serialiser escaping, the comment-stripping recipe, `JsonNode.DeepEquals` and the stdin filter), `dotnet fsi` for the F# snippet, and PowerShell 7.6.6 on .NET 10.0.12 for the `Add-Type` snippet. The repository's tests were also run: 37 pass on net10.0 and 37 on net48.

## Facts verified while writing (not in the README)

- A `//` line comment swallows the token after it: `{"a":1, // a note about a` + newline + `"b":2}` prints `//anoteabouta"b": 2`, because the spaces inside the comment and the newline that ends it are whitespace and are dropped. A block comment survives as `1/*note*/,`. The wiki documents this and the fix (`JsonNode.Parse` with `JsonCommentHandling.Skip` and `AllowTrailingCommas`, then `ToJsonString()`, then pretty print).
- Error messages: `Unexpected '}' at index 7: there is no open object or array to close.`, `Unexpected '}' at index 2: expected ']'.`; null gives `Value cannot be null. (Parameter 'inputString')` even through the extension method (parameter name comes from the printer); `IndentSize = -1` gives `IndentSize cannot be negative. (Parameter 'value')` with `Actual value was -1.` on a second line.
- `NewLine = " "` with `IndentSize = 0` gives `{ "a": 1, "b": [ 1, 2 ], "c": {} }`; `NewLine = ""` gives `{"a": 1,"b": [1,2]}`.
- Duplicate keys, key order, numbers (`1.0`, `1e3`, `-0`, `007`), top-level scalars, a leading BOM and non-ASCII text all pass through unchanged. Two documents in one string print as `}{`. A thousand-deep nesting works (1,999 lines). 20,000 `Parallel.For` calls of `PrettyPrintJson()` gave identical output.
- `JsonPrettyPrintOptions` is a record: `==` works, `with` works, `ToString()` prints the three values. F# sets init properties with `JsonPrettyPrintOptions(IndentSize = 2)`; PowerShell 7.6 can assign `$o.IndentSize = 2` after `::new()` despite the `init` accessor.
- System.Text.Json default escaping in `ToJson()`: `<b>&'é"</b>` becomes `\u003Cb\u003E\u0026\u0027\u00E9\u0022\u003C/b\u003E`. `JavaScriptEncoder.UnsafeRelaxedJsonEscaping` gives `<b>&'é\"</b>`. `DeserializeFromJson<object>()` returns a `JsonElement`. `DateTimeOffset` serialises with its offset, enums as numbers.
- `JsonNode.DeepEquals` ignores key order as well as whitespace.
- .NET 10 file-based apps enable native AOT by default (learn.microsoft.com/dotnet/core/sdk/file-based-apps, "Native AOT publishing enabled by default"), which makes the reflection-based `ToJson`/`DeserializeFromJson` throw `InvalidOperationException: Reflection-based serialization has been disabled for this application` even under `dotnet run`. `#:property PublishAot=false` fixes it (also `#:property JsonSerializerIsReflectionEnabledByDefault=true`, but the IL2026/IL3050 warnings then remain). The printer itself is unaffected.
- `JavaScriptSerializer` (1.x) matched property names case-insensitively: `ObjectConverter.AssignToPropertyOrField` uses `BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public` (microsoft/referencesource, `System.Web.Extensions/Script/Serialization/ObjectConverter.cs`). System.Text.Json is case-sensitive unless `PropertyNameCaseInsensitive` is set.
- The benchmark's 1 MB document has 5,954 items; pretty printed it is 2,715,804 characters over 107,176 lines, 2.59 times the input.
- Package 3.0.1 on nuget.org: 37,214 bytes, published 2026-09-25, `lib/net10.0` and `lib/netstandard2.0` DLLs 12,800 bytes each with XML docs, README and icon inside. Dependencies: none on net10.0; `System.Memory` 4.6.3 and `System.Text.Json` 10.0.12 on netstandard2.0 (transitive: Microsoft.Bcl.AsyncInterfaces, System.Buffers, System.IO.Pipelines, System.Numerics.Vectors, System.Runtime.CompilerServices.Unsafe, System.Text.Encodings.Web, System.Threading.Tasks.Extensions). Listed versions: 1.0.0, 1.0.1, 1.0.1.1 (2014), 2.0.0, 2.1.1, 3.0.1 (2026-09-25). Downloads on 2026-09-28: 309,939 total, 200,592 of them 1.0.1.1. `2.1.1` and `3.0.1` are live, so the publish hand-off in the 2026-09-25 log is resolved.
- The repository has no issues, open or closed; pull requests are the 2016 Gitter badge, the 2.0.0 branch and five closed dependabot bumps.
- `https://www.markdavidrogers.com` refused a TLS handshake from this machine (curl/schannel and PowerShell both), so the article and tools links were not re-checked; the wiki links them as the README does.

## Inaccuracies found in the shipped docs (not fixed; they ship in the package)

1. README, "Not a validator": says comments "come out indented rather than rejected". A `//` comment breaks the document (see above). Reword at the next README change; the wiki already says it.
2. CHANGELOG, 2.0.0 entry: says quotes inside strings are escaped as `\"`. System.Text.Json's default encoder writes `\u0022` (the test file `jsonLintBeautifyExample.json` shows the same). Fix the entry.
3. README, first paragraph: "Try it live at https://www.markdavidrogers.com/tools or through the site's MCP server tool `prettify_json`". The site formats with System.Text.Json (`markdavidrogers-web/src/MarkDavidRogers.Web/Tools/JsonPrettifier.cs`, `WriteIndented = true`), not with this package, so the output differs (two spaces, validated input). Either switch the site back to the package (tracked in the site's plans) or reword the README.
4. `JsonPrettyPrinterPlus.csproj`: the comment says to set `PackageValidationBaselineVersion` to 3.0.1 once it is on nuget.org. It is. Add `<PackageValidationBaselineVersion>3.0.1</PackageValidationBaselineVersion>` with the next change.

## Gotchas

- GitHub wiki repositories use branch `master`; the clone came that way.
- `curl` of the nuget.org `registration5-gz-semver2` index needs `--compressed`.
- Concurrent `dotnet run` of the same file-based app contends for its build output; separate folders or `dotnet build` then `--no-build`.

Related: [../log/2026-09-28-github-wiki.md](../log/2026-09-28-github-wiki.md), [../HANDOFF.md](../HANDOFF.md), [../log/2026-09-25-modernization-2.1.0-and-3.0.0.md](../log/2026-09-25-modernization-2.1.0-and-3.0.0.md), the sibling procedure in `DotNetRandomNameGenerator/ai-docs/notes/2026-09-28-github-wiki.md`.
