"""Compare two golden recordings case by case (package-modernize retrofit, 2026-09-28).

Usage: python compare.py OLD.json NEW.json [--markdown]

Prints every case whose result differs, every case only one side has, and a summary. A difference that disappears when
CRLF is read as LF in both results is reported as "line endings only", because 1.x and 2.x wrote Environment.NewLine
and 3.x writes LF. Used for the upgrade story in CHANGELOG.md and the wiki's Versions and upgrading page: the 3.0.1
recording against the 1.0.1.1 and 2.1.1 recordings made by the same capture program (tests/Golden/Capture).
Standard library only.
"""

import json
import sys

CR = chr(13)
LF = chr(10)


def load(path):
    with open(path, encoding="utf-8") as f:
        data = json.load(f)
    cases = {}
    for c in data["cases"]:
        cases[(c["group"], c["name"])] = c["result"]
    return data, cases


def normalise(value):
    text = json.dumps(value, ensure_ascii=True, sort_keys=False)
    # In the JSON text a CR is the six characters of its escape; drop it before an LF escape.
    esc = chr(92)
    return text.replace(esc + "r" + esc + "n", esc + "n")


def short(value, limit=160):
    text = json.dumps(value, ensure_ascii=True)
    return text if len(text) <= limit else text[:limit] + "..."


def main(argv):
    if len(argv) < 3:
        print(__doc__)
        return 2
    old_meta, old = load(argv[1])
    new_meta, new = load(argv[2])
    md = "--markdown" in argv
    same = eol = 0
    differ = []
    for key in old:
        if key not in new:
            continue
        a, b = old[key], new[key]
        if json.dumps(a) == json.dumps(b):
            same += 1
        elif normalise(a) == normalise(b):
            eol += 1
        else:
            differ.append((key, a, b))
    only_old = [k for k in old if k not in new]
    only_new = [k for k in new if k not in old]
    print("old: " + old_meta["package"] + " on " + old_meta["runtime"] + " (" + str(len(old)) + " cases)")
    print("new: " + new_meta["package"] + " on " + new_meta["runtime"] + " (" + str(len(new)) + " cases)")
    print("shared cases: " + str(same + eol + len(differ)) + "; identical " + str(same) + "; line endings only " + str(eol) + "; different " + str(len(differ)))
    print("only in old: " + str(len(only_old)) + "; only in new: " + str(len(only_new)))
    groups = {}
    for (g, n), a, b in differ:
        groups.setdefault(g, []).append((n, a, b))
    for g in groups:
        print("")
        print(("### " if md else "== ") + g + " (" + str(len(groups[g])) + ")")
        for n, a, b in groups[g]:
            print(("- " if md else "  ") + n)
            print("    old: " + short(a))
            print("    new: " + short(b))
    if only_old:
        print("")
        print("only in old: " + ", ".join(g + "/" + n for g, n in only_old))
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
