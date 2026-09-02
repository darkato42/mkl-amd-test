# MKL/OpenBLAS on AMD CPU: Support & Restriction Analysis

## Environment verified

- Host: `192.168.77.113` (Windows 11)
- CPU: `AMD Ryzen 7 7840HS` (`AuthenticAMD`)
- .NET SDK: `8.0.424`
- Packages (active benchmark config):
  - `MathNet.Numerics` `5.0.0`
  - `MathNet.Numerics.MKL.Win-x64` `3.0.0`
  - `MathNet.Numerics.Providers.OpenBLAS` `5.0.0`
  - `MathNet.Numerics.OpenBLAS.Win` `0.3.0-beta1`
- oneMKL installed on host: `Intel.oneMKL 2026.1.0`
- VC++ runtime installed: `Microsoft.VCRedist.2015+.x64 14.51.36247.0`

## OpenBLAS version discovery (NuGet)

- `MathNet.Numerics.OpenBLAS.Win`
  - Latest any: `0.3.0-beta1`
  - **Latest stable: `0.2.0`**
- `MathNet.Numerics.Providers.OpenBLAS`
  - Latest any: `6.0.0-beta2`
  - Latest stable: `5.0.0`

## Practical verification on this AMD CPU

### MKL

- `MklProvider_CanBeLoaded` test: **PASS**
- Provider: `Intel MKL (x64; revision 15; MKL 2022.0)`

### OpenBLAS (current config using `MathNet.Numerics.OpenBLAS.Win 0.3.0-beta1`)

- Provider load in benchmark app: **PASS**
- Provider: `OpenBLAS (x64; revision 1)`

## Benchmark comparison: Managed vs MKL vs OpenBLAS (0.3.0-beta1)

Source: `reports/benchmark-run.txt`

- GEMM_Multiply_900
  - Managed: 85.95 ms
  - MKL: 6.83 ms (**12.58x vs Managed**)
  - OpenBLAS: 10.14 ms (**8.47x vs Managed**)
  - MKL faster than OpenBLAS by **1.48x**

- LU_Solve_700
  - Managed: 70.40 ms
  - MKL: 2.12 ms (**33.22x**)
  - OpenBLAS: 9.49 ms (**7.42x**)
  - MKL faster than OpenBLAS by **4.48x**

- QR_Solve_700
  - Managed: 143.30 ms
  - MKL: 12.59 ms (**11.38x**)
  - OpenBLAS: 93.47 ms (**1.53x**)
  - MKL faster than OpenBLAS by **7.42x**

- Cholesky_Factor_900
  - Managed: 56.77 ms
  - MKL: 3.35 ms (**16.95x**)
  - OpenBLAS: 9.57 ms (**5.93x**)
  - MKL faster than OpenBLAS by **2.86x**

## Additional test requested: latest stable OpenBLAS library (`0.2.0`)

Source: `reports/benchmark-openblas-stable-0.2.0.txt`

- Switched benchmark project to `MathNet.Numerics.OpenBLAS.Win 0.2.0` and reran.
- Result:
  - `OpenBlasLoaded=False`
  - `OpenBlasProvider=Managed`
- Interpretation: this stable package did not activate OpenBLAS with the current .NET 8 + MathNet 5 benchmark setup.

After this test, project was returned to `MathNet.Numerics.OpenBLAS.Win 0.3.0-beta1` so OpenBLAS benchmarking remains functional.

## AMD generation restrictions — what we can and cannot assert

### Confirmed

- Intel release notes explicitly mention AMD hardware behavior in some versions.
- oneMKL 2024 release notes state a known AMD-on-Windows issue and indicate a fix starting from oneMKL 2025.0.1.
- Your environment (oneMKL 2026.1.0) is beyond that fix line and passes runtime tests.

### Not publicly documented as a strict matrix

- We did **not** find an official Intel table like “Zen1/Zen2/Zen3/Zen4/Zen5 supported or unsupported.”
- Intel system-requirement docs primarily list Intel CPU families for validated support; for AMD, practical compatibility should be validated by smoke/perf tests on target hardware.

## Recommended operational policy for AMD fleets

1. Pin oneMKL to a known-good modern version (>=2025.0.1 on Windows).
2. Keep a provider smoke test in CI (`UseNativeMKL`/`TryUseNativeMKL`).
3. Benchmark representative workloads per CPU family/SKU.
4. Keep OpenBLAS fallback path, but prefer the variant that actually loads in your stack.

## Sources

- Intel oneMKL System Requirements:
  - https://www.intel.com/content/www/us/en/developer/articles/system-requirements/oneapi-math-kernel-library-system-requirements.html
- Intel oneMKL 2024 Release Notes:
  - https://www.intel.com/content/www/us/en/developer/articles/release-notes/onemkl-release-notes-2024.html
- Math.NET MKL docs:
  - https://numerics.mathdotnet.com/MKL
