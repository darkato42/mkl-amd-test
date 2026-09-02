# mkl-amd-test

Validation project for running MathNet + Intel MKL provider on an AMD Windows host.

## What this repo contains

- `tests/MklAmdProbe.Tests`: xUnit smoke tests
  - verifies MKL provider can load
  - checks matrix multiply correctness
- `bench/MklAmdBench`: multi-operation benchmark app
  - GEMM multiply
  - LU solve
  - QR solve
  - Cholesky factorization
- `reports/`: captured outputs from the target AMD VM

## Target host used

- OS: Windows 11
- CPU: AMD Ryzen 7 7840HS (AuthenticAMD)
- Runtime: .NET 8 SDK

## Dependencies

- `MathNet.Numerics` 5.0.0
- `MathNet.Numerics.MKL.Win-x64` 3.0.0
- Visual C++ Redistributable x64 (`Microsoft.VCRedist.2015+.x64`)

## Run tests

```bash
dotnet test --runtime win-x64 -v minimal --logger "trx;LogFileName=amd-mkl-tests.trx"
```

## Run benchmark

```bash
cd bench/MklAmdBench
dotnet run -c Release -r win-x64
```

## Latest benchmark summary (from reports/benchmark-run.txt)

- GEMM_Multiply_900: Managed 82.44 ms, MKL 7.44 ms, **11.08x**
- LU_Solve_700: Managed 67.93 ms, MKL 2.30 ms, **29.56x**
- QR_Solve_700: Managed 133.57 ms, MKL 10.28 ms, **12.99x**
- Cholesky_Factor_900: Managed 49.10 ms, MKL 3.14 ms, **15.65x**

## Notes on AMD support

This repo demonstrates successful MKL loading and acceleration on an AMD CPU in practice. For details on support caveats and Intel release-note constraints, see:

- `reports/amd-support-analysis.md`
