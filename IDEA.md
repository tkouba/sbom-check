# IDEA: sbom-check (Lightweight SBOM Policy Tool)

## Motivation

Modern .NET projects heavily rely on third-party dependencies. While tools like CycloneDX generate SBOMs (Software Bill of Materials), 
there is a gap between *data generation* and *policy enforcement*.

Existing solutions:
- CycloneDX → generates SBOM
- cyclonedx-cli → validation, diff
- OSS Review Toolkit → full enterprise compliance (heavyweight)

Missing:
👉 Simple, non-enterprise, lightweight dotnet CLI tool for **license and dependency policy enforcement**.

Similar tools:
1. CycloneDX/sbom-utility (https://github.com/CycloneDX/sbom-utility)
2. safedep/vet (https://github.com/safedep/vet)
3. nuget-license (https://github.com/tomchavakis/nuget-license)

---

## Goal

Create a **.NET CLI tool** that:

- Reads a CycloneDX `bom.json`
- Displays license usage
- Enforces policies (fail build when violating rules)

---

## Core Features

### ✅ 1. License Overview

Command:

```
sbom-check bom.json
```

Output:

```
Licenses used:
- MIT (12)
- Apache-2.0 (8)
- Microsoft (5)
- UNKNOWN (2)
```

---

### ✅ 2. Forbidden Licenses

Command:

```
sbom-check bom.json --forbidden-licenses GPL-3.0,AGPL-3.0
```

Behavior:

- Detect components using forbidden licenses
- Print violations
- Exit with non-zero code

Output example:

```
❌ Forbidden licenses detected:

GPL-3.0:
 - Some.Package@1.0.0
 - Legacy.Component@2.1.3
```

---

### ✅ 3. Forbidden Components

Command:

```
sbom-check bom.json --forbidden-components log4net
```

Behavior:

- Detect usage of banned packages
- Fail build

---

### ✅ 4. Multi-license Support

- Handle multiple licenses per component
- Report all relevant license IDs

---

### ✅ 5. Exit Codes

| Code | Meaning |
|------|--------|
| 0 | Success (no violations) |
| 1 | Policy violation |

---

## Optional Features (Future)

### ✅ Forbidden component version range (implemented in v0.5)

- SemVer range syntax (log4net@(-,2.0.1])
- Support multiple component rules
- Allow mix of:
  - name-only (block all versions)
  - exact version
  - version ranges
- Same component can appear multiple times with different rules

```
--forbidden-components   log4net   log4net@2.0.0   log4net@(-,2.0.1]
```
---

### ✅ Allowed Licenses Mode (implemented in v0.3)

```
--allowed-licenses MIT,Apache-2.0
```

Only these licenses are permitted.

---

### 🔸 Fail on unknown licenses

```
--fail-on-unknown-license
```

- Fail when license is UNKNOWN (`null`) or license is custom string. 
- Standard behavior UNKNOWN/custom is ok, fail only when flag.

---

### ✅ Ignore components (implemented in v0.6)

```
--ignore-components MyCompany.*,Legacy.*

```

- ignore own components
- simple whitelisting
- ignore > forbidden, example: `--forbidden-components log4net --ignore-components log4net@2.0.1` log4net@2.0.1 -> Ok


---

### 🔸 Config File

```
sbom-check bom.json --config sbom-policy.json
```

Example:

```
{
  "forbiddenLicenses": ["GPL-3.0"],
  "forbiddenComponents": ["log4net"]
}
```

---

### 🔸 Diff Mode

```
sbom-check bom.json --diff previous-bom.json
```

Detect new dependencies or licenses.

---

### 🔸 Summary only

```
sbom-check bom.json --summary-only
```

Output:
```
✅ OK (MIT=12, Apache=8)
```
or for forbidden GPL
```
❌ FAIL (MIT=12, GPL=2)
```

- CI log noise
  - CI-friendly

---

## Architecture

### Input

- CycloneDX JSON (`bom.json`)

### Processing

- Deserialize JSON
- Extract:
  - component name
  - version
  - licenses (SPDX ID or name)

### Output

- Console report
- Exit code for CI

---

## Design Principles

- ✅ Simple CLI-first tool
- ✅ No custom license resolution
- ✅ Use SBOM as source of truth
- ✅ Fast and dependency-free
- ✅ CI-friendly

---

## Non-Goals

- ❌ Full compliance engine (ORT-level)
- ❌ License detection from source code
- ❌ Complex policy DSL
- ❌ --warn-only or similar feature, simple ignore exit code

---

## Target Users

- .NET developers
- DevOps engineers
- Teams using CycloneDX SBOM

---

## Distribution

- .NET global tool

```
dotnet tool install -g sbom-check
```

---

## Summary

`sbom-check` fills the gap between SBOM generation and policy enforcement by providing a simple, fast, and developer-friendly CLI tool.

