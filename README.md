# mkl-amd-test

Validation project for MathNet native-provider performance on an AMD Windows host.

## What this repo contains

- `tests/MklAmdProbe.Tests`: xUnit smoke tests
  - verifies MKL provider can load
  - checks matrix multiply correctness
- `bench/MklAmdBench`: **BenchmarkDotNet** benchmark app
  - compares Managed vs MKL vs OpenBLAS
  - workloads: GEMM multiply, LU solve, QR solve, Cholesky factorization
- `reports/`: captured outputs from the target AMD VM

## Target host used

- OS: Windows 11
- CPU: AMD Ryzen 7 7840HS (AuthenticAMD)
- Runtime: .NET 8 SDK

## Dependencies

- `BenchmarkDotNet` 0.14.0
- `MathNet.Numerics` 5.0.0
- `MathNet.Numerics.MKL.Win-x64` 3.0.0
- `MathNet.Numerics.Providers.OpenBLAS` 5.0.0
- `MathNet.Numerics.OpenBLAS.Win` 0.3.0-beta1 (active benchmark config)
- Visual C++ Redistributable x64 (`Microsoft.VCRedist.2015+.x64`)

## Run tests

```bash
dotnet test --runtime win-x64 -v minimal --logger "trx;LogFileName=amd-mkl-tests.trx"
```

## Run benchmarks (BenchmarkDotNet)

```bash
cd bench/MklAmdBench
dotnet run -c Release -r win-x64
```

BenchmarkDotNet artifacts are generated under:
- `bench/MklAmdBench/BenchmarkDotNet.Artifacts/results/`

## Latest BenchmarkDotNet summary (ShortRun)

Source: `reports/benchmarkdotnet/MklAmdBench.LinearAlgebraProviderBenchmarks-report-github.md`

- Gemm_Multiply
  - Managed: 39.209 ms
  - MKL: 2.982 ms (**13.15x speedup vs Managed**)
  - OpenBLAS: 5.028 ms (**7.80x speedup vs Managed**)
- LU_Solve
  - Managed: 33.566 ms
  - MKL: 1.280 ms (**26.22x**)
  - OpenBLAS: 5.924 ms (**5.67x**)
- QR_Solve
  - Managed: 75.078 ms
  - MKL: 6.227 ms (**12.06x**)
  - OpenBLAS: 71.070 ms (**1.06x**)
- Cholesky_Factor
  - Managed: 21.813 ms
  - MKL: 1.672 ms (**13.05x**)
  - OpenBLAS: 5.403 ms (**4.04x**)

## Stable OpenBLAS 0.2.0 compatibility check (brief)

- Latest stable `MathNet.Numerics.OpenBLAS.Win` found: **0.2.0**
- Trial result on this setup (`.NET 8` + `MathNet 5`): `OpenBlasLoaded=False` (provider fell back to Managed)
- Evidence file: `reports/benchmark-openblas-stable-0.2.0.txt`
- Therefore this repo keeps `0.3.0-beta1` for working OpenBLAS benchmarks.

## AMD support notes

Runtime verification on this host confirms MKL works on AMD CPU in practice. For caveats and version policy, see:

- `reports/amd-support-analysis.md`
