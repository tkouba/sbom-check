# AGENTS.md – Development Guide for sbom-check

## Purpose

This document captures **engineering decisions, conventions, and design principles** for the `sbom-check` tool.

It is intended for:
- contributors
- future maintainers
- AI/code assistants

👉 This is NOT a product spec (see IDEA.md for that).

## Code Style

### Type names vs aliases
- **Declarations, casts, type annotations** → C# alias: `int`, `string`, `bool`, `double`
- **Static methods, static properties, constants** → BCL class name: `Int32.TryParse`, `String.IsNullOrEmpty`, `String.Empty`, `Double.NaN`

### Null and length checks
- Prefer explicit checks: `value != null && value.Length > 0` over pattern matching `value is { Length: > 0 }`

## Git Workflow for Issue Resolution

When working on an issue, a dedicated Git branch must be created before any implementation work begins.

### Requirements

1. Create a separate branch for each issue. Prefix branch fix or feature depending on issue label `bug` or `enhancement`.
2. Use a clear and descriptive branch name, preferably including the issue identifier.
3. Commit all changes to the dedicated branch.
4. Do not commit issue-related changes directly to the main, master, or release branches.
5. After the work is completed and validated, open a Pull Request (PR) targeting the appropriate base branch.
6. The issue is considered complete only after the Pull Request has been created and all required checks or reviews have been addressed.

### Example

```text
Issue: #123 Login timeout handling
Label: Bug

Branch:
fix/123-login-timeout
```

## Core Philosophy

- Keep the tool **simple and predictable**
- SBOM (CycloneDX) is the **single source of truth**
- Avoid over-engineering
- CLI must be **CI-friendly** and deterministic
- Prefer clarity over abstraction

## CLI Design Decisions

### Do NOT use System.CommandLine

Reasons:
- unstable API across versions
- frequent breaking changes
- weak documentation
- high complexity for simple CLI scenarios

### Preferred CLI option - Spectre.Console.Cli 

Why:
- richer CLI UX is desired
- colored output, tables, structured output

Benefits:
- stable API
- good developer experience
- built-in formatting tools (tables, colors)

## Output Design

Output must always be:

- human-readable
- CI-friendly (clear errors, simple parsing)
- grouped logically

### Structure

1. License summary
2. Violations (if any)
3. Additional info (optional)

### Example

```
✔ License summary
  MIT            12
  Apache-2.0      8

✘ Violations
  GPL-3.0
    - Some.Package@1.0.0
```

## Policy Rules

Supported rule types:

- `--forbidden-licenses` — block components using specific SPDX license IDs
- `--allowed-licenses` — whitelist mode; any unlisted license is a violation
- `--forbidden-components` — block packages by name, exact version, or NuGet version range
- `--ignore-components` — exclude components from all checks; supports wildcards and version ranges; takes priority over all forbidden rules

### Behavior

- ANY violation → exit code 1
- NO violations → exit code 0

## License Handling

- Use SPDX IDs whenever available
- Fallback to license name if ID is missing
- Multiple licenses per component must be supported

Fallback order:
1. SPDX ID
2. License name
3. UNKNOWN

## JSON Handling (CycloneDX)

- Input assumed: CycloneDX JSON
- Do not attempt to:
  - resolve licenses from URLs
  - fetch external data

Only operate on data already present in SBOM.

## Performance Considerations

- Must be fast (used in CI)
- Avoid network calls
- Avoid heavy dependencies

## Non-Goals

- Full compliance engine (no ORT-level complexity)
- License detection from source code
- Custom SBOM generation
- Complex policy DSL

## Error Handling

- Clear, actionable error messages
- Do not throw raw exceptions to user
- Always print context (component + version)

## Suggested Project Structure

```
/src
  SbomCheck/
    Program.cs
    Cli/           ← CheckCommand, CheckCommandSettings
    Sbom/          ← BomReader, Models (BomDocument, Component, LicenseChoice)
    Policy/        ← LicensePolicyEvaluator, ComponentPolicyEvaluator, ComponentRule, IgnoreRule
    Output/        ← LicenseSummaryRenderer
    Models/        ← LicensesResult, LicenseDetail, LicenseStatus, ViolationReason, …

/tests
  SbomCheck.Tests/ ← xUnit tests; accesses internals via InternalsVisibleTo

/samples
  bom.json        ← synthetic BOM covering edge cases
  realWorld.json  ← real-world BOM used for integration tests
```

## Testing Strategy

Test project: `tests/SbomCheck.Tests` (xUnit). Internals are exposed via `InternalsVisibleTo`.

Unit tests cover:
- `BomReader` — file not found, invalid JSON, empty/null components field
- `Component.GetLicenseIds` — SPDX ID, name fallback, UNKNOWN, deduplication
- `LicensePolicyEvaluator` — no policy, forbidden, allowed list, priority rules
- `ComponentPolicyEvaluator` — name-only, exact version, range, fail-safe
- `ComponentRule` — parsing, math-notation normalization `(-,x]` → `(,x]`, fail-safe
- `IgnoreRule` — wildcards, version ranges, inverted fail-safe

Integration tests use `samples/realWorld.json` (166-component real-world BOM).

## Distribution

- .NET global tool (`dotnet tool`)
- No runtime dependencies preferred

## Release Process

- Pushing a tag matching `vX.Y.Z` (e.g. `v1.2.0`) triggers `.github/workflows/release.yml`, which tests, packs (version taken from the tag, overriding the csproj default), publishes to NuGet.org, and creates a matching GitHub Release.
- Publishing uses NuGet.org **Trusted Publishing** (OIDC) — no long-lived API key is stored in the repo. One-time setup required on nuget.org: a Trusted Publishing policy with Repository Owner `tkouba`, Repository `sbom-check`, Workflow File `release.yml`.
- The workflow needs a `NUGET_USER` repository secret containing the nuget.org username (profile name, not email) associated with that policy.
