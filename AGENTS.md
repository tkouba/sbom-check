# AGENTS.md – Development Guide for sbom-check

## Purpose

This document captures **engineering decisions, conventions, and design principles** for the `sbom-check` tool.

It is intended for:
- contributors
- future maintainers
- AI/code assistants

👉 This is NOT a product spec (see IDEA.md for that).

---

## Core Philosophy

- Keep the tool **simple and predictable**
- SBOM (CycloneDX) is the **single source of truth**
- Avoid over-engineering
- CLI must be **CI-friendly** and deterministic
- Prefer clarity over abstraction

---

## CLI Design Decisions

### ❌ Do NOT use System.CommandLine

Reasons:
- unstable API across versions
- frequent breaking changes
- weak documentation
- high complexity for simple CLI scenarios

---

### ✅ Preferred CLI option - Spectre.Console.Cli 

Why:
- richer CLI UX is desired
- colored output, tables, structured output

Benefits:
- stable API
- good developer experience
- built-in formatting tools (tables, colors)

---

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

---

## Policy Rules

Supported rule types:

- `--forbidden-licenses` — block components using specific SPDX license IDs
- `--allowed-licenses` — whitelist mode; any unlisted license is a violation
- `--forbidden-components` — block packages by name, exact version, or NuGet version range
- `--ignore-components` — exclude components from all checks; supports wildcards and version ranges; takes priority over all forbidden rules

### Behavior

- ANY violation → exit code 1
- NO violations → exit code 0

---

## License Handling

- Use SPDX IDs whenever available
- Fallback to license name if ID is missing
- Multiple licenses per component must be supported

Fallback order:
1. SPDX ID
2. License name
3. UNKNOWN

---

## JSON Handling (CycloneDX)

- Input assumed: CycloneDX JSON
- Do not attempt to:
  - resolve licenses from URLs
  - fetch external data

Only operate on data already present in SBOM.

---

## Performance Considerations

- Must be fast (used in CI)
- Avoid network calls
- Avoid heavy dependencies

---

## Non-Goals

- Full compliance engine (no ORT-level complexity)
- License detection from source code
- Custom SBOM generation
- Complex policy DSL

---

## Error Handling

- Clear, actionable error messages
- Do not throw raw exceptions to user
- Always print context (component + version)

---

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

---

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

---

## Distribution

- .NET global tool (`dotnet tool`)
- No runtime dependencies preferred

---

## Future Ideas

- config file support
- SBOM diff support
- HTML report export

---

## Final Notes

Keep this tool small, focused, and predictable.

If it starts looking like ORT → stop and reconsider.
