```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26200.9278)
Unknown processor
.NET SDK 8.0.424
  [Host]         : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  ShortRun-Net80 : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

Job=ShortRun-Net80  Runtime=.NET 8.0  IterationCount=3  
LaunchCount=1  WarmupCount=3  

```
| Method          | Provider | Mean      | Error      | StdDev    | Min       | Max       | Gen0      | Gen1      | Gen2     | Allocated |
|---------------- |--------- |----------:|-----------:|----------:|----------:|----------:|----------:|----------:|---------:|----------:|
| **Cholesky_Factor** | **Managed**  | **21.813 ms** |  **2.6808 ms** | **0.1469 ms** | **21.715 ms** | **21.982 ms** |   **93.7500** |   **62.5000** |  **62.5000** |   **4.17 MB** |
| Cholesky_Factor | MKL      |  1.672 ms |  0.1771 ms | 0.0097 ms |  1.665 ms |  1.683 ms |  117.1875 |  117.1875 | 117.1875 |   3.74 MB |
| Cholesky_Factor | OpenBLAS |  5.403 ms |  2.2427 ms | 0.1229 ms |  5.331 ms |  5.545 ms |  109.3750 |  109.3750 | 109.3750 |   3.74 MB |
| **Gemm_Multiply**   | **Managed**  | **39.209 ms** | **23.2363 ms** | **1.2737 ms** | **37.936 ms** | **40.483 ms** | **1230.7692** | **1076.9231** |  **76.9231** |  **11.28 MB** |
| Gemm_Multiply   | MKL      |  2.982 ms |  0.5666 ms | 0.0311 ms |  2.947 ms |  3.007 ms |  117.1875 |  117.1875 | 117.1875 |   3.74 MB |
| Gemm_Multiply   | OpenBLAS |  5.028 ms |  2.4304 ms | 0.1332 ms |  4.914 ms |  5.174 ms |  109.3750 |  109.3750 | 109.3750 |   3.74 MB |
| **LU_Solve**        | **Managed**  | **33.566 ms** | **13.6388 ms** | **0.7476 ms** | **33.112 ms** | **34.429 ms** |         **-** |         **-** |        **-** |   **2.32 MB** |
| LU_Solve        | MKL      |  1.280 ms |  1.9141 ms | 0.1049 ms |  1.211 ms |  1.401 ms |   78.1250 |   78.1250 |  78.1250 |   2.32 MB |
| LU_Solve        | OpenBLAS |  5.924 ms | 11.4022 ms | 0.6250 ms |  5.490 ms |  6.640 ms |   70.3125 |   70.3125 |  70.3125 |   2.32 MB |
| **QR_Solve**        | **Managed**  | **75.078 ms** | **11.6036 ms** | **0.6360 ms** | **74.348 ms** | **75.515 ms** |  **285.7143** |         **-** |        **-** |   **9.25 MB** |
| QR_Solve        | MKL      |  6.227 ms |  7.3774 ms | 0.4044 ms |  5.767 ms |  6.528 ms |   93.7500 |   93.7500 |  93.7500 |   4.64 MB |
| QR_Solve        | OpenBLAS | 71.070 ms | 24.3435 ms | 1.3344 ms | 69.745 ms | 72.413 ms |         - |         - |        - |   4.64 MB |
