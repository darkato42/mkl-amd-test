# MKL/OpenBLAS on AMD CPU: Support & Restriction Analysis

## Environment verified

- Host: `192.168.77.113` (Windows 11)
- CPU: `AMD Ryzen 7 7840HS` (`AuthenticAMD`)
- .NET SDK: `8.0.424`
- Benchmark harness: `BenchmarkDotNet 0.14.0` (ShortRun profile)
- Packages (active benchmark config):
  - `MathNet.Numerics` `5.0.0`
  - `MathNet.Numerics.MKL.Win-x64` `3.0.0`
  - `MathNet.Numerics.Providers.OpenBLAS` `5.0.0`
  - `MathNet.Numerics.OpenBLAS.Win` `0.3.0-beta1`
- oneMKL installed on host: `Intel.oneMKL 2026.1.0`
- VC++ runtime installed: `Microsoft.VCRedist.2015+.x64 14.51.36247.0`

## BenchmarkDotNet results (Managed vs MKL vs OpenBLAS)

Source artifact:
- `reports/benchmarkdotnet/MklAmdBench.LinearAlgebraProviderBenchmarks-report-github.md`

ShortRun mean timings:

- Gemm_Multiply
  - Managed: 39.209 ms
  - MKL: 2.982 ms (**13.15x** speedup vs Managed)
  - OpenBLAS: 5.028 ms (**7.80x** speedup vs Managed)
  - MKL faster than OpenBLAS by **1.69x**

- LU_Solve
  - Managed: 33.566 ms
  - MKL: 1.280 ms (**26.22x**)
  - OpenBLAS: 5.924 ms (**5.67x**)
  - MKL faster than OpenBLAS by **4.63x**

- QR_Solve
  - Managed: 75.078 ms
  - MKL: 6.227 ms (**12.06x**)
  - OpenBLAS: 71.070 ms (**1.06x**)
  - MKL faster than OpenBLAS by **11.41x**

- Cholesky_Factor
  - Managed: 21.813 ms
  - MKL: 1.672 ms (**13.05x**)
  - OpenBLAS: 5.403 ms (**4.04x**)
  - MKL faster than OpenBLAS by **3.23x**

Conclusion on this AMD host: both native providers can accelerate over managed, and MKL is consistently faster than OpenBLAS in this benchmark set.

## OpenBLAS version probe and stable test

NuGet discovery:
- `MathNet.Numerics.OpenBLAS.Win`
  - Latest any: `0.3.0-beta1`
  - Latest stable: `0.2.0`

Stable-package test:
- Switched benchmark project to `MathNet.Numerics.OpenBLAS.Win 0.2.0`
- Observed: `OpenBlasLoaded=False` (provider fell back to Managed)
- Evidence: `reports/benchmark-openblas-stable-0.2.0.txt`

## AMD generation restrictions — what is known

### Confirmed

- Intel release notes explicitly mention AMD hardware behavior in certain versions.
- oneMKL 2024 release notes document an AMD-on-Windows issue and note fix availability starting from oneMKL 2025.0.1.
- This environment uses oneMKL 2026.1.0 and passes runtime validation.

### Not provided as a strict public matrix

- No official Intel table was found mapping AMD Zen generations (Zen1/2/3/4/5) to supported/unsupported status for oneMKL CPU mode.
- Practical policy is version pinning + runtime validation on each target CPU family.

## Recommended operational policy for AMD fleets

1. Pin oneMKL to a known-good version (>=2025.0.1 on Windows).
2. Keep provider-load smoke tests in CI for MKL and fallback providers.
3. Run BenchmarkDotNet baselines per hardware family before production rollout.
4. Keep OpenBLAS fallback path available, but use a package/version combination that actually loads in your target stack.

## Sources

- Intel oneMKL System Requirements:
  - https://www.intel.com/content/www/us/en/developer/articles/system-requirements/oneapi-math-kernel-library-system-requirements.html
- Intel oneMKL 2024 Release Notes:
  - https://www.intel.com/content/www/us/en/developer/articles/release-notes/onemkl-release-notes-2024.html
- Math.NET MKL docs:
  - https://numerics.mathdotnet.com/MKL
