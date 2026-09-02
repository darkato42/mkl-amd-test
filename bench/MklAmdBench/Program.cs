using System;
using System.Collections.Generic;
using System.Diagnostics;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Providers.LinearAlgebra;

class Program
{
    static void Main()
    {
        var rng = new Random(12345);

        int nMul = 900;
        int nSolve = 700;
        int nQr = 700;
        int nChol = 900;

        var aMul = Matrix<double>.Build.Dense(nMul, nMul, (_, _) => rng.NextDouble());
        var bMul = Matrix<double>.Build.Dense(nMul, nMul, (_, _) => rng.NextDouble());

        var aSolveBase = Matrix<double>.Build.Dense(nSolve, nSolve, (_, _) => rng.NextDouble());
        var aSolve = aSolveBase.TransposeThisAndMultiply(aSolveBase) + Matrix<double>.Build.DenseIdentity(nSolve) * 1e-3;
        var bSolve = Vector<double>.Build.Dense(nSolve, _ => rng.NextDouble());

        var aQr = Matrix<double>.Build.Dense(nQr, nQr, (_, _) => rng.NextDouble());
        var bQr = Vector<double>.Build.Dense(nQr, _ => rng.NextDouble());

        var aCholBase = Matrix<double>.Build.Dense(nChol, nChol, (_, _) => rng.NextDouble());
        var aChol = aCholBase.TransposeThisAndMultiply(aCholBase) + Matrix<double>.Build.DenseIdentity(nChol) * 1e-3;

        var benchDefs = new (string Name, Func<double> Work, int Rounds)[]
        {
            ("GEMM_Multiply_900", () => {
                var c = aMul * bMul;
                return c[0,0] + c[nMul-1,nMul-1] + c.Row(0).Sum();
            }, 3),

            ("LU_Solve_700", () => {
                var x = aSolve.Solve(bSolve);
                return x[0] + x[nSolve-1] + x.Sum();
            }, 3),

            ("QR_Solve_700", () => {
                var x = aQr.QR().Solve(bQr);
                return x[0] + x[nQr-1] + x.Sum();
            }, 2),

            ("Cholesky_Factor_900", () => {
                var l = aChol.Cholesky().Factor;
                return l[0,0] + l[nChol-1,nChol-1] + l.Column(0).Sum();
            }, 3)
        };

        Control.UseManaged();
        var managedProvider = LinearAlgebraControl.Provider.ToString();
        var managed = RunSuite(benchDefs);

        var mklLoaded = LinearAlgebraControl.TryUseNativeMKL();
        var mklProvider = LinearAlgebraControl.Provider.ToString();
        var mkl = mklLoaded ? RunSuite(benchDefs) : new Dictionary<string, (double Ms, double Check)>();

        Control.UseManaged();
        var openBlasLoaded = LinearAlgebraControl.TryUseNativeOpenBLAS();
        var openBlasProvider = LinearAlgebraControl.Provider.ToString();
        var openBlas = openBlasLoaded ? RunSuite(benchDefs) : new Dictionary<string, (double Ms, double Check)>();

        Console.WriteLine("CPU_BENCH_START");
        Console.WriteLine($"ManagedProvider={managedProvider}");
        Console.WriteLine($"MklLoaded={mklLoaded}");
        Console.WriteLine($"MklProvider={mklProvider}");
        Console.WriteLine($"OpenBlasLoaded={openBlasLoaded}");
        Console.WriteLine($"OpenBlasProvider={openBlasProvider}");

        foreach (var (name, _, _) in benchDefs)
        {
            var m = managed[name];

            if (mklLoaded)
            {
                var k = mkl[name];
                var speed = m.Ms / k.Ms;
                var diff = Math.Abs(m.Check - k.Check);
                Console.WriteLine($"BENCH_MKL={name};ManagedMs={m.Ms:F2};MklMs={k.Ms:F2};Speedup={speed:F2};AbsCheckDiff={diff:E3}");
            }

            if (openBlasLoaded)
            {
                var o = openBlas[name];
                var speed = m.Ms / o.Ms;
                var diff = Math.Abs(m.Check - o.Check);
                Console.WriteLine($"BENCH_OPENBLAS={name};ManagedMs={m.Ms:F2};OpenBlasMs={o.Ms:F2};Speedup={speed:F2};AbsCheckDiff={diff:E3}");
            }

            if (mklLoaded && openBlasLoaded)
            {
                var k = mkl[name];
                var o = openBlas[name];
                Console.WriteLine($"BENCH_MKL_vs_OPENBLAS={name};MklMs={k.Ms:F2};OpenBlasMs={o.Ms:F2};MklOverOpenBlas={(o.Ms / k.Ms):F2}");
            }
        }

        Console.WriteLine("CPU_BENCH_END");
    }

    static Dictionary<string, (double Ms, double Check)> RunSuite((string Name, Func<double> Work, int Rounds)[] defs)
    {
        var map = new Dictionary<string, (double Ms, double Check)>();
        foreach (var d in defs)
        {
            _ = d.Work();
            var sw = Stopwatch.StartNew();
            double checksum = 0.0;
            for (int i = 0; i < d.Rounds; i++)
            {
                checksum = d.Work();
            }
            sw.Stop();
            map[d.Name] = (sw.Elapsed.TotalMilliseconds / d.Rounds, checksum);
        }
        return map;
    }
}
