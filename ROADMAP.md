# ROADMAP.md – sbom-check

## TL;DR

Start small, validate quickly, avoid over-engineering.

## Vision

Create a **simple, fast, and reliable SBOM policy checker** for .NET projects based on CycloneDX.

Focus: minimalism, correctness, CI usability.

---

# 🔮 Future Enhancements

Order of post-1.0 enhancements is not planned, some of them may be cancelled or moved to pre-1.0.

## Small enhancements

Small enhancements with big impact

- `--fail-on-unknown-license`
- `--message` - custom message

  
## Config File

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

## SBOM Diff

```
--diff previous-bom.json
```

- new dependencies
- new licenses
- idea, but use-case missing

---

## Output Formats

- JSON output
- CI-friendly output mode

---

## HTML Report

- simple report for audits
- idea, but breaks rules **simple** and **CI-friendly**

## SPDX JSON file format support

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
