# sbom-check

Lightweight .NET CLI tool to **analyze and enforce license and component policies on [CycloneDX](https://cyclonedx.org/) SBOMs**.

Designed for CI pipelines — exits with code `1` on any violation.

---

## Installation

```sh
dotnet tool install -g sbom-check
```

Requires [.NET 8 or later](https://dotnet.microsoft.com/download). (.NET 8 and .NET 9 support will be dropped after November 2026.)

---

## Quick start

```sh
sbom-check bom.json
```

```
── Info: License summary ───────────────────────────────────────────────────────

  MIT                  7
  Apache-2.0           6
  UNKNOWN              2
  Proprietary License  1
  Custom-1.0           1

Total components found: 16
```

No policy configured → always exits `0`.

---

## Options

| Option | Description |
|--------|-------------|
| `bom.json` | Path to CycloneDX JSON file **(required)** |
| `--forbidden-licenses <licenses>` | Comma-separated SPDX IDs to forbid. Repeatable. |
| `--allowed-licenses <licenses>` | Whitelist mode — any unlisted license is a violation. Repeatable. |
| `--forbidden-components <components>` | Component rules: name, `name@version`, or `name@range`. Repeatable. |
| `--ignore-components <components>` | Exclude from all checks. Supports wildcards and version ranges. Repeatable. |
| `--plain` | Plain ASCII output — no colors, no box-drawing characters. Recommended for CI log files. |

All `<licenses>` and `<components>` values accept comma-separated lists and can be provided multiple times.

---

## Features

### License overview

Displays a license summary grouped by SPDX ID. No policy required.

```sh
sbom-check bom.json
```

- Licenses without a SPDX ID fall back to the license `name` field
- Components with no license data are listed as `UNKNOWN`
- Duplicate components (same name + version) are deduplicated automatically

---

### Forbidden licenses

Fail when a component uses a forbidden license:

```sh
sbom-check bom.json --forbidden-licenses GPL-3.0,AGPL-3.0
```

```
── Invalid: License summary ────────────────────────────────────────────────────

  MIT                  7
  Apache-2.0           6
  UNKNOWN              2
  Proprietary License  1
  Custom-1.0           1

Total components found: 16

── Forbidden licenses detected ─────────────────────────────────────────────────

  GPL-3.0
    - Some.Package@1.0.0
    - Legacy.Component@2.1.3
```

Exit code: `1`.

---

### Allowed licenses (whitelist)

Only permit specific licenses — anything not on the list is a violation:

```sh
sbom-check bom.json --allowed-licenses MIT,Apache-2.0
```

```
── Invalid: License summary ────────────────────────────────────────────────────

  MIT                  7
  Apache-2.0           6
  UNKNOWN              2
  Proprietary License  1
  Custom-1.0           1

Total components found: 16

── Licenses not in allowed list ────────────────────────────────────────────────

  Custom-1.0
    - EmptyIdLib@1.0.0
  Proprietary License
    - SomeProprietaryLib@1.0.0
  UNKNOWN
    - LegacyComponent@2.0.0
    - EmptyAllLib@1.0.0
```

`--forbidden-licenses` and `--allowed-licenses` can be combined. Forbidden always takes priority.

---

### Forbidden components

Block specific packages by name (case-insensitive):

```sh
sbom-check bom.json --forbidden-components log4net
```

```
── Invalid: License summary ────────────────────────────────────────────────────
  ...

── Forbidden components detected ───────────────────────────────────────────────

  log4net
    - log4net@1.2.10
    - log4net@2.0.1
    - log4net@2.0.15
```

**Version rules** — target an exact version or a [NuGet version range](https://learn.microsoft.com/en-us/nuget/concepts/package-versioning#version-ranges):

```sh
# Block all versions
--forbidden-components log4net

# Block an exact version only
--forbidden-components log4net@2.0.1

# Block a range: all versions up to and including 2.0.1
--forbidden-components "log4net@(-,2.0.1]"

# Multiple rules (comma-separated or repeated flag)
--forbidden-components "log4net@(-,2.0.1]" --forbidden-components "log4net@2.0.15"
```

> **Fail-safe:** if a component's version is missing or cannot be parsed, the rule matches.

---

### Ignore components (escape hatch)

Exclude specific components from all policy checks. Takes priority over every forbidden rule:

```sh
sbom-check bom.json \
  --allowed-licenses MIT,Apache-2.0 \
  --ignore-components "SomeProprietaryLib,LegacyComponent,EmptyIdLib,EmptyAllLib"
```

```
── Valid: License summary ──────────────────────────────────────────────────────

  MIT         7
  Apache-2.0  6

Total components found: 16

── Ignored components (4) ──────────────────────────────────────────────────────

  EmptyAllLib@1.0.0         UNKNOWN
  EmptyIdLib@1.0.0          Custom-1.0
  LegacyComponent@2.0.0     UNKNOWN
  SomeProprietaryLib@1.0.0  Proprietary License
```

Exit code: `0`.

Supports wildcards and version ranges:

```sh
# Wildcard — ignore all packages with this name prefix
--ignore-components "Microsoft*"

# Exact version
--ignore-components "log4net@2.0.1"

# Range — ignore versions up to and including 2.0.1
--ignore-components "log4net@(-,2.0.1]"

# Combined
--ignore-components "Microsoft*,log4net@(-,2.0.1]"
```

> **Note:** for versioned ignore rules, a missing or unparseable component version is **not** ignored — it surfaces as a potential violation.

---

### Plain output

No colors and no box-drawing characters — safe for any log file or terminal:

```sh
sbom-check bom.json --plain --forbidden-licenses GPL-3.0
```

```
Invalid: License summary

  GPL-3.0  2
  MIT      7

Total components found: 9

Forbidden licenses detected:

  GPL-3.0
    - Some.Package@1.0.0
    - Legacy.Component@2.1.3
```

Recommended for CI environments where ANSI output is not rendered.

---

## Exit codes

| Code | Meaning |
|------|---------|
| `0` | No violations (or no policy configured) |
| `1` | Policy violation **or** input error (file not found, invalid JSON) |

---

## Generating a CycloneDX SBOM

Install the CycloneDX .NET tool:

```sh
dotnet tool install -g CycloneDX
```

Generate from a solution or project:

```sh
dotnet cyclonedx MySolution.sln -o ./
```

This produces `bom.json` ready for `sbom-check`.

---

## CI example (GitHub Actions)

```yaml
- name: Generate SBOM
  run: dotnet cyclonedx MySolution.sln -o ./

- name: Check SBOM policies
  run: |
    dotnet tool install -g sbom-check
    sbom-check bom.json \
      --forbidden-licenses GPL-3.0,AGPL-3.0,LGPL-3.0 \
      --allowed-licenses MIT,Apache-2.0,BSD-2-Clause \
      --ignore-components "MyCompany.*"
```

---

## Design principles

- **No network calls** — works fully offline
- **Deterministic output** — same input always produces the same result
- **Fail-safe** — ambiguous cases (missing versions, unknown licenses) surface as violations rather than being silently skipped
- **Minimal dependencies** — [NuGet.Versioning](https://www.nuget.org/packages/NuGet.Versioning) and [Spectre.Console](https://spectreconsole.net/) only

---

## License

MIT
