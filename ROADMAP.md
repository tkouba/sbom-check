# ROADMAP.md – sbom-check

## Vision

Create a **simple, fast, and reliable SBOM policy checker** for .NET projects based on CycloneDX.

Focus: minimalism, correctness, CI usability.

---

# 🚀 Milestones

## ✅ v0.1 – MVP (Core CLI)

### Features
- Read CycloneDX `bom.json`
- Extract components:
  - name
  - version
  - licenses
- Print license summary (grouped + counts)

### CLI
```
sbom-check bom.json
```

### Output
- License list + counts

### Done when
- Works on real BOM
- Handles missing licenses safely

---

## ✅ v0.2 – Policy Enforcement

### Features
- `--forbidden-licenses`
- detect violation
- exit code = 1

### Output
- list violating components grouped by license

### CLI
```
sbom-check bom.json --forbidden-licenses GPL-3.0
```

### Done when
- CI can fail build

---

## ✅ v0.3 – Allowed Licenses (Whitelist)

```
--allowed-licenses MIT,Apache-2.0
```

---

## ✅ v0.4 – Component Blocking

### Features
- `--forbidden-components`
- match by package name (case-insensitive)

### Output
- list of matching components

### CLI
```
sbom-check bom.json --forbidden-components log4net
```

---

## ✅ v0.5 – Component Rules with Versions (IMPORTANT)

### Features

Support **SemVer range filtering for forbidden components**:

Examples:

```
--forbidden-components log4net
--forbidden-components log4net@2.0.1
--forbidden-components log4net@(-,2.0.1]
```

### Requirements

- Support multiple component rules
- Allow mix of:
  - name-only (block all versions)
  - exact version
  - version ranges
- Same component can appear multiple times with different rules

Example:

```
--forbidden-components   log4net   log4net@2.0.0   log4net@(-,2.0.1]
```

### Behavior

- Case-insensitive matching
- Match if ANY rule applies
- Use NuGet Semantic Versioning semantics

### Output example

```
❌ Forbidden components detected:

log4net (<=2.0.1)
 - log4net@2.0.0
 - log4net@2.0.1
```

### Notes

- Use `NuGet.Versioning`
- If version cannot be parsed → treat as violation (fail-safe)

---
## ✅ v0.6 – Ignore components

- `--ignore-components `
- Override rules
- Wildcards
- SemVer ranges
- Combined and multiple

Examples:

```
--ignore-components log4net
--ignore-components log4net@2.0.1
--ignore-components log4net@(-,2.0.1]
```

Ignore components with wildcard:

```
--ignore-components MyComponent*,Microsoft*
```

Ignore multiple versions:

```
--ignore-components log4net@(-,2.0.0],log4net@2.0.5
```

Combined example:

```
--ignore-components log4net@(-,2.0.0],log4net@2.0.5,Microsoft*
```


---

## ✅ v0.7 – Multi-license & Edge Cases

### Features
- full multi-license support
- fallback chain:
  - SPDX ID
  - name
  - UNKNOWN
- deduplication (name + version)

---

## ✅ v0.8 – CLI UX Improvements

### Features
- better formatting:
  - grouping
  - indentation
  - optional colors
- consistent error formatting

---

# 🧪 v0.9 – Testing & Hardening

### Tests
- valid SBOM
- multi-license SBOM
- missing license
- invalid input
- policy violations

### Stability
- no crashes on malformed JSON
- safe null handling

---

# 📦 v1.0 – First Public Release

### Requirements
- stable CLI
- stable output format
- documented usage

### Deliverables
- README.md
- examples
- sample SBOM

### Distribution
```
dotnet tool install -g sbom-check
```

---

# 🔮 Future Enhancements

Order of post-1.0 enhancements is not planned, some of them may be cancelled or moved to pre-1.0.

## v1.x – Post-1.0 enhancements

Small enhancements with big impact

- --fail-on-unknown-license
- --summary-only (compact CI output)
  
## v1.x – Config File

```
--config sbom-policy.json
```

Example:

```
{
  "forbiddenLicenses": ["GPL-3.0"],
  "forbiddenComponents": ["log4net"]
}
```

---

## v1.x – SBOM Diff

```
--diff previous-bom.json
```

Features:
- new dependencies
- new licenses

---

## v1.x – Output Formats

- JSON output
- CI-friendly output mode

---

## v1.x – HTML Report

- simple report for audits

## v2.x - SPDX JSON file format support

- simple SPDX JSON input only
- no SPDX policy evaluation
- no boolean license logic
- keep this tool simple for dev teams

---

# ❌ Explicit Non-Roadmap (Do NOT implement)

- full ORT-like compliance engine
- custom license detection from source
- network-based enrichment
- complex DSL for policies

---

# 🧠 Development Strategy

- build MVP fast
- validate on real projects
- keep scope tight
- avoid framework complexity

---

# ✅ Definition of Done (Overall)

- tool works reliably in CI
- deterministic output
- simple onboarding for users
- minimal dependencies

---

# TL;DR

Start small, validate quickly, avoid over-engineering.
