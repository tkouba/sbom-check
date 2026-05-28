# sbom-check

Lightweight CLI tool to **analyze and enforce policies on CycloneDX SBOMs**.

---

## 🚀 Why sbom-check?

Modern .NET projects rely heavily on third-party dependencies. While tools like CycloneDX generate SBOMs, there is a missing piece:

👉 **Simple, fast policy enforcement for CI pipelines**

sbom-check focuses on:

- ✅ License visibility
- ✅ Blocking unsafe licenses (e.g., GPL)
- ✅ Blocking specific packages (e.g., log4net)
- ✅ Zero over-engineering

---

## 📦 Features

### ✅ License Overview

Display all licenses used in the project:

```
sbom-check bom.json
```

Output:

```
License summary

  MIT                  5
  Apache-2.0           2
  GPL                  1
  Proprietary License  1
  UNKNOWN              1

Total components found: 9
```

---

### 🚫 Forbidden Licenses

Fail when forbidden licenses are detected:

```
sbom-check bom.json --forbidden-licenses GPL-3.0,AGPL-3.0
```

Output:

```
❌ Forbidden licenses detected:

GPL-3.0:
 - Some.Package@1.0.0
 - Legacy.Component@2.1.3
```

Exit code:

- `0` = OK
- `1` = violation

---

### 🚫 Forbidden Components

Block specific packages:

```
sbom-check bom.json --forbidden-components log4net
```

---

## 📥 Installation

```
dotnet tool install -g sbom-check
```

---

## ⚙️ Usage

Basic:

```
sbom-check <bom.json>
```

With policies:

```
sbom-check <bom.json>   --forbidden-licenses GPL-3.0   --forbidden-components log4net
```

---

## 📁 Input

- CycloneDX JSON (`bom.json`)

Example generation:

```
dotnet cyclonedx MySolution.sln -o bom.json
```

---

## ✅ Exit Codes

| Code | Meaning |
|------|--------|
| 0 | Success |
| 1 | Policy violation |

---

## 🧠 Design Principles

- minimal dependencies
- no external API calls
- SBOM is source of truth
- deterministic output (CI-friendly)

---

## 🚧 Roadmap

See [ROADMAP.md]

---

## 🤝 Contributing

- Keep it simple
- Avoid unnecessary abstractions
- Don’t introduce heavy frameworks

---

## 📄 License

MIT

---

## TL;DR

**sbom-check = simple SBOM → policy enforcement → CI fail**
