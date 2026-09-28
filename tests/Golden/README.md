# Golden recordings

What the **published** JsonPrettyPrinter packages answer, recorded from nuget.org on 2026-09-28 before any code in this repository changed (package-modernize retrofit, Phase 0). The files here never change after the commit that added them: when the golden test fails, the fix goes in the library, or the difference becomes a named exception the maintainer ruled on in the plan.

| File | What it is |
|---|---|
| `3.0.1.net48-windows.json`, `3.0.1.net10.0-windows.json` | The contract: 1174 cases of the published 3.0.1 on each runtime a caller can have (net48 loads `lib/netstandard2.0`, net10.0 loads `lib/net10.0`). Each was run twice and the runs were byte-identical. The header holds the SHA-256 of the DLL that answered, the process bitness (64-bit), the System.Text.Json version and the time zone. |
| `PublicApi-3.0.1.txt` | Every public type and member of 3.0.1 with parameter names, by reflection (`ApiList/`). |
| `Capture/` | The program that made the recordings. `Cases.cs` holds every case and touches only public names, so the golden test compiles it unchanged against the new library. |
| `ApiList/` | The program that made the API lists. |
| `upgrade/` | The upgrade story: the same capture run against 2.1.1 and 1.0.1.1 (1.x allows 446 of the cases), the API lists of both, `compare.py`, and its reports against 3.0.1 on each runtime. Not replayed by any test. |

Re-running a capture (from `Capture/`; the empty `Directory.*` files keep the repository's MSBuild settings out, so the folder also runs when copied elsewhere):

```
dotnet run -c Release -f net48 -p:OldVersion=3.0.1 -- ../3.0.1.net48-windows.json
dotnet run -c Release -f net10.0 -p:OldVersion=1.0.1.1 -- ../upgrade/1.0.1.1.net10.0-windows.json
python ../upgrade/compare.py ../upgrade/1.0.1.1.net48-windows.json ../3.0.1.net48-windows.json
```

Inputs in `Cases.cs` use two placeholders so no editor or shell rewrites them: a backtick stands for a backslash and `{U+XXXX}` for one UTF-16 code unit. The recordings are ASCII: every other character is written as a six-character escape.
