# MKL/OpenBLAS on AMD CPU: Support & Restriction Analysis

## Environment verified

- Host: `192.168.77.113` (Windows 11)
- CPU: `AMD Ryzen 7 7840HS` (`AuthenticAMD`)
- .NET SDK: `8.0.424`
- Packages:
  - `MathNet.Numerics` `5.0.0`
  - `MathNet.Numerics.MKL.Win-x64` `3.0.0`
  - `MathNet.Numerics.Providers.OpenBLAS` `5.0.0`
  - `MathNet.Numerics.OpenBLAS.Win` `0.3.0-beta1`
- oneMKL package installed on host: `Intel.oneMKL 2026.1.0`
- VC++ runtime installed: `Microsoft.VCRedist.2015+.x64 14.51.36247.0`

## Practical verification result on AMD CPU

- MKL provider load test: **PASS**
  - Provider: `Intel MKL (x64; revision 15; MKL 2022.0)`
- OpenBLAS provider load in benchmark app: **PASS**
  - Provider: `OpenBLAS (x64; revision 1)`

## Benchmark comparison (Managed vs MKL vs OpenBLAS)

Source: `reports/benchmark-run.txt`

- GEMM_Multiply_900
  - Managed: 83.59 ms
  - MKL: 5.55 ms (**15.05x vs Managed**)
  - OpenBLAS: 9.89 ms (**8.45x vs Managed**)
  - MKL faster than OpenBLAS by **1.78x**

- LU_Solve_700
  - Managed: 68.23 ms
  - MKL: 2.01 ms (**34.03x**)
  - OpenBLAS: 6.16 ms (**11.08x**)
  - MKL faster than OpenBLAS by **3.07x**

- QR_Solve_700
  - Managed: 142.22 ms
  - MKL: 59.97 ms (**2.37x**)
  - OpenBLAS: 90.93 ms (**1.56x**)
  - MKL faster than OpenBLAS by **1.52x**

- Cholesky_Factor_900
  - Managed: 49.69 ms
  - MKL: 3.01 ms (**16.51x**)
  - OpenBLAS: 8.30 ms (**5.99x**)
  - MKL faster than OpenBLAS by **2.76x**

Conclusion on this AMD host: both native providers accelerate over managed; MKL is consistently faster than OpenBLAS in these workloads.

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
2. Always run provider-load smoke tests (`TryUseNativeMKL`/`UseNativeMKL`) in CI.
3. Run numeric consistency checks and representative benchmarks on each target CPU family.
4. Keep OpenBLAS fallback path available if a future MKL regression appears on specific AMD SKUs.

## Sources

- Intel oneMKL System Requirements:
  - https://www.intel.com/content/www/us/en/developer/articles/system-requirements/oneapi-math-kernel-library-system-requirements.html
- Intel oneMKL 2024 Release Notes:
  - https://www.intel.com/content/www/us/en/developer/articles/release-notes/onemkl-release-notes-2024.html
- Math.NET MKL docs:
  - https://numerics.mathdotnet.com/MKL
