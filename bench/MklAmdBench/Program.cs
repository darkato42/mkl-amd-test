using System;
using System.Collections.Generic;
using System.Diagnostics;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Providers.LinearAlgebra;

class Program
{
    record BenchResult(string Name, double ManagedMs, double MklMs, double Speedup, double ManagedCheck, double MklCheck, double AbsDiff);

    static void Main()
    {
        var rng = new Random(12345);

        // Shared deterministic datasets
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

        Dictionary<string, (double Ms, double Check)> mkl = new();
        if (mklLoaded)
        {
            mkl = RunSuite(benchDefs);
        }

        Console.WriteLine($"CPU_BENCH_START");
        Console.WriteLine($"ManagedProvider={managedProvider}");
        Console.WriteLine($"MklLoaded={mklLoaded}");
        Console.WriteLine($"MklProvider={mklProvider}");

        if (!mklLoaded)
        {
            Console.WriteLine("MKL not loaded; no comparison possible.");
            return;
        }

        var results = new List<BenchResult>();
        foreach (var (name, _, _) in benchDefs)
        {
            var m1 = managed[name];
            var m2 = mkl[name];
            var speedup = m1.Ms / m2.Ms;
            var absDiff = Math.Abs(m1.Check - m2.Check);
            results.Add(new BenchResult(name, m1.Ms, m2.Ms, speedup, m1.Check, m2.Check, absDiff));
        }

        foreach (var r in results)
        {
            Console.WriteLine($"BENCH={r.Name};ManagedMs={r.ManagedMs:F2};MklMs={r.MklMs:F2};Speedup={r.Speedup:F2};AbsCheckDiff={r.AbsDiff:E3}");
        }

        Console.WriteLine("CPU_BENCH_END");
    }

    static Dictionary<string, (double Ms, double Check)> RunSuite((string Name, Func<double> Work, int Rounds)[] defs)
    {
        var map = new Dictionary<string, (double Ms, double Check)>();
        foreach (var d in defs)
        {
            // warmup
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
