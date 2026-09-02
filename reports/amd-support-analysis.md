# MKL on AMD CPU: Support & Restrictions Analysis

## Environment verified

- Host: `192.168.77.113` (Windows 11)
- CPU: `AMD Ryzen 7 7840HS` (`AuthenticAMD`)
- .NET SDK: `8.0.424`
- MathNet packages:
  - `MathNet.Numerics` `5.0.0`
  - `MathNet.Numerics.MKL.Win-x64` `3.0.0`
- oneMKL package installed on host: `Intel.oneMKL 2026.1.0`
- VC++ runtime installed: `Microsoft.VCRedist.2015+.x64 14.51.36247.0`

## Practical verification result

MKL works on this AMD CPU in our test setup:

- `MklProvider_CanBeLoaded` test: **PASS**
- Provider string: `Intel MKL (x64; revision 15; MKL 2022.0)`
- Multi-operation benchmarks show substantial speedup vs managed provider.

## About AMD generation restrictions

### What we can confirm from sources and runtime

1. **Intel oneMKL system requirements pages list Intel CPU families explicitly** (Core/Xeon/Atom), and do **not** publish an AMD-generation compatibility table.
2. **Intel release notes explicitly mention AMD hardware behavior** (there was a known Windows AMD issue in oneMKL 2024.x, fixed in oneMKL 2025.0.1).
3. In practice, on this AMD Zen4 mobile CPU (`Ryzen 7 7840HS`), MKL loads and benchmarks correctly.

### Interpretation for project decisions

- There is **no public Intel matrix like “AMD Zen1/Zen2/Zen3/Zen4 supported/unsupported”** for oneMKL CPU mode.
- Operationally, treat AMD support as **runtime-validated**:
  - pin a known-good oneMKL version (>=2025.0.1 on Windows),
  - run provider-load smoke tests,
  - run numeric sanity tests + benchmark checks on target hardware.

## Required setup for AMD host (Windows + MathNet)

1. Install `.NET SDK`.
2. Add `MathNet.Numerics` + `MathNet.Numerics.MKL.Win-x64` packages.
3. Install `Microsoft.VCRedist.2015+.x64`.
4. Execute with x64 runtime (`--runtime win-x64`).

## Sources referenced

- Intel oneMKL system requirements:
  - https://www.intel.com/content/www/us/en/developer/articles/system-requirements/oneapi-math-kernel-library-system-requirements.html
- Intel oneMKL 2024 release notes (known AMD hardware issue note, fixed starting 2025.0.1):
  - https://www.intel.com/content/www/us/en/developer/articles/release-notes/onemkl-release-notes-2024.html
- Math.NET MKL provider docs:
  - https://numerics.mathdotnet.com/MKL
