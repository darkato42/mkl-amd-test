# mkl-amd-test

Validation project for MathNet native-provider performance on an AMD Windows host.

## What this repo contains

- `tests/MklAmdProbe.Tests`: xUnit smoke tests
  - verifies MKL provider can load
  - checks matrix multiply correctness
- `bench/MklAmdBench`: multi-operation benchmark app
  - compares Managed vs MKL vs OpenBLAS
  - workloads: GEMM multiply, LU solve, QR solve, Cholesky factorization
- `reports/`: captured outputs from the target AMD VM

## Target host used

- OS: Windows 11
- CPU: AMD Ryzen 7 7840HS (AuthenticAMD)
- Runtime: .NET 8 SDK

## Dependencies

- `MathNet.Numerics` 5.0.0
- `MathNet.Numerics.MKL.Win-x64` 3.0.0
- `MathNet.Numerics.Providers.OpenBLAS` 5.0.0
- `MathNet.Numerics.OpenBLAS.Win` 0.3.0-beta1 (active benchmark config)
- Visual C++ Redistributable x64 (`Microsoft.VCRedist.2015+.x64`)

## Stable OpenBLAS 0.2.0 compatibility check (brief)

- Latest stable `MathNet.Numerics.OpenBLAS.Win` found: **0.2.0**
- Trial result on this setup (`.NET 8` + `MathNet 5`): `OpenBlasLoaded=False` (provider fell back to Managed)
- Evidence file: `reports/benchmark-openblas-stable-0.2.0.txt`
- Therefore this repo keeps `0.3.0-beta1` for working OpenBLAS benchmarks.

## Run tests

```bash
dotnet test --runtime win-x64 -v minimal --logger "trx;LogFileName=amd-mkl-tests.trx"
```

## Run benchmark

```bash
cd bench/MklAmdBench
dotnet run -c Release -r win-x64
```

## Latest benchmark summary (from `reports/benchmark-run.txt`)

- GEMM_Multiply_900
  - Managed 83.59 ms
  - MKL 5.55 ms (**15.05x speedup**)
  - OpenBLAS 9.89 ms (**8.45x speedup**)
- LU_Solve_700
  - Managed 68.23 ms
  - MKL 2.01 ms (**34.03x**)
  - OpenBLAS 6.16 ms (**11.08x**)
- QR_Solve_700
  - Managed 142.22 ms
  - MKL 59.97 ms (**2.37x**)
  - OpenBLAS 90.93 ms (**1.56x**)
- Cholesky_Factor_900
  - Managed 49.69 ms
  - MKL 3.01 ms (**16.51x**)
  - OpenBLAS 8.30 ms (**5.99x**)

## AMD support notes

Runtime verification on this host confirms MKL works on AMD CPU in practice. For caveats and version policy, see:

- `reports/amd-support-analysis.md`
