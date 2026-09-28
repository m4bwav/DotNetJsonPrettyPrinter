#!/usr/bin/env bash
# The package holds exactly what it should and declares its dependencies (package-modernize template check, moved
# here from ci.yml on 2026-09-28 so release.yml runs the same check on the nupkg it pushes). Usage: check-contents.sh DIR
set -euo pipefail
nupkg=$(ls "$1"/JsonPrettyPrinter.[0-9]*.nupkg)
# Every file the package must hold, in C-locale order as `LC_ALL=C sort` gives it (uppercase first).
expected=$'JsonPrettyPrinter.nuspec\nREADME.md\nicon.png\nlib/net10.0/JsonPrettyPrinterPlus.dll\nlib/net10.0/JsonPrettyPrinterPlus.xml\nlib/netstandard2.0/JsonPrettyPrinterPlus.dll\nlib/netstandard2.0/JsonPrettyPrinterPlus.xml'
actual=$(unzip -Z1 "$nupkg" | grep -Ev '^(_rels/|package/|\[Content_Types\])' | LC_ALL=C sort)
if [ "$actual" != "$expected" ]; then
  echo "::error::unexpected package contents"
  diff <(echo "$expected") <(echo "$actual")
  exit 1
fi
nuspec=$(unzip -p "$nupkg" JsonPrettyPrinter.nuspec | tr -d '\r\n')
for tfm in .NETStandard2.0 net10.0; do
  # grep finds nothing in an empty (self-closing) group; under -e and pipefail that would end the step without a message.
  group=$(echo "$nuspec" | { grep -o "<group targetFramework=\"$tfm\">.*" || true; } | sed 's:</group>.*::')
  deps=$(echo "$group" | { grep -o '<dependency id="[^"]*" version="[^"]*"' || true; } | LC_ALL=C sort | tr '\n' ' ')
  # The declared dependencies of each group, sorted by id, each followed by one space ('' for none).
  case "$tfm" in
    .NETStandard2.0) want='<dependency id="System.Memory" version="4.6.3" <dependency id="System.Text.Json" version="10.0.12" ' ;;
    *) want='' ;;
  esac
  if [ "$deps" != "$want" ]; then
    echo "::error::$tfm dependencies: $deps"
    exit 1
  fi
done
