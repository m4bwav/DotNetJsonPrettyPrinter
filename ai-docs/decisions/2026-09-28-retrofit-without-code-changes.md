---
title: Retrofit 3.0.1 without library code changes, release as 3.0.2
kind: decision
status: proposed
date: 2026-09-28
verified: 2026-09-28
stale_after: never
tags: [retrofit, 3.0.2, compatibility, golden]
summary: "why the package-modernize retrofit changes no library code and releases 3.0.2 for the docs and the new release path; read before changing any output of the printer or the serializer helpers"
---

# Retrofit 3.0.1 without library code changes

## Context

The golden capture of the published 3.0.1 (1174 cases per runtime) found no bug in the printer or the helpers. What is wrong is in the shipped README and CHANGELOG, the release path and the repository settings. The popular versions (1.0.1 and 1.0.1.1) differ a great deal from 3.x, but those differences are already shipped in 2.0.0 to 3.0.1; the retrofit documents them instead of changing anything.

## Decision (proposed; the maintainer rules in the plan review)

- No library code changes. Every recorded 3.0.1 answer stays the same on its runtime; the only named exception is a System.Text.Json message compared by type when the runtime's System.Text.Json version differs from the recording's.
- Release 3.0.2 after a 3.0.2-beta.1 rehearsal, for the corrected README (nuget.org renders it from the package) and to prove release.yml and the moved publishing policy.

## Reasons

- The recording is the contract from now on; a retrofit that changes answers would need a named exception for each, and nothing asks for one.
- The README ships inside the package, so its corrections need a version; a patch is the honest size.

## Rejected options

- Fixing the `//` comment behaviour now: it changes output for input that is not JSON, so it belongs behind a new option or in 4.0.
- No release: nuget.org would keep a README that says comments pass through and points at a tool that does not use the package.

Related: builds on [../plans/2026-09-28-retrofit-and-3.0.2-release.md](../plans/2026-09-28-retrofit-and-3.0.2-release.md); see also [../notes/2026-09-28-phase-0-gap-audit.md](../notes/2026-09-28-phase-0-gap-audit.md).
