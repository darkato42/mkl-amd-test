using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Providers.LinearAlgebra;

namespace MklAmdBench;

public enum ProviderKind
{
    Managed,
    MKL,
    OpenBLAS
}

public class ProviderBenchConfig : ManualConfig
{
    public ProviderBenchConfig()
    {
        AddJob(Job.ShortRun
            .WithRuntime(BenchmarkDotNet.Environments.CoreRuntime.Core80)
            .WithId("ShortRun-Net80"));

        AddColumn(TargetMethodColumn.Method, StatisticColumn.Mean, StatisticColumn.Min, StatisticColumn.Max, StatisticColumn.StdDev);
        AddExporter(MarkdownExporter.GitHub);
        Orderer = new DefaultOrderer(SummaryOrderPolicy.Method);
    }
}

[Config(typeof(ProviderBenchConfig))]
[MemoryDiagnoser]
public class LinearAlgebraProviderBenchmarks
{
    [Params(ProviderKind.Managed, ProviderKind.MKL, ProviderKind.OpenBLAS)]
    public ProviderKind Provider { get; set; }

    private Matrix<double> _aMul = null!;
    private Matrix<double> _bMul = null!;

    private Matrix<double> _aSolve = null!;
    private Vector<double> _bSolve = null!;

    private Matrix<double> _aQr = null!;
    private Vector<double> _bQr = null!;

    private Matrix<double> _aChol = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var rng = new Random(12345);

        const int nMul = 700;
        const int nSolve = 550;
        const int nQr = 550;
        const int nChol = 700;

        _aMul = Matrix<double>.Build.Dense(nMul, nMul, (_, _) => rng.NextDouble());
        _bMul = Matrix<double>.Build.Dense(nMul, nMul, (_, _) => rng.NextDouble());

        var aSolveBase = Matrix<double>.Build.Dense(nSolve, nSolve, (_, _) => rng.NextDouble());
        _aSolve = aSolveBase.TransposeThisAndMultiply(aSolveBase) + Matrix<double>.Build.DenseIdentity(nSolve) * 1e-3;
        _bSolve = Vector<double>.Build.Dense(nSolve, _ => rng.NextDouble());

        _aQr = Matrix<double>.Build.Dense(nQr, nQr, (_, _) => rng.NextDouble());
        _bQr = Vector<double>.Build.Dense(nQr, _ => rng.NextDouble());

        var aCholBase = Matrix<double>.Build.Dense(nChol, nChol, (_, _) => rng.NextDouble());
        _aChol = aCholBase.TransposeThisAndMultiply(aCholBase) + Matrix<double>.Build.DenseIdentity(nChol) * 1e-3;

        SetProvider(Provider);
    }

    [Benchmark]
    public double Gemm_Multiply()
    {
        var c = _aMul * _bMul;
        return c[0, 0] + c[_aMul.RowCount - 1, _bMul.ColumnCount - 1] + c.Row(0).Sum();
    }

    [Benchmark]
    public double LU_Solve()
    {
        var x = _aSolve.Solve(_bSolve);
        return x[0] + x[x.Count - 1] + x.Sum();
    }

    [Benchmark]
    public double QR_Solve()
    {
        var x = _aQr.QR().Solve(_bQr);
        return x[0] + x[x.Count - 1] + x.Sum();
    }

    [Benchmark]
    public double Cholesky_Factor()
    {
        var l = _aChol.Cholesky().Factor;
        return l[0, 0] + l[l.RowCount - 1, l.ColumnCount - 1] + l.Column(0).Sum();
    }

    private static void SetProvider(ProviderKind provider)
    {
        Control.UseManaged();

        switch (provider)
        {
            case ProviderKind.Managed:
                Control.UseManaged();
                break;
            case ProviderKind.MKL:
                Control.UseNativeMKL();
                break;
            case ProviderKind.OpenBLAS:
                Control.UseNativeOpenBLAS();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(provider), provider, null);
        }

        var providerName = LinearAlgebraControl.Provider.ToString();
        if (provider == ProviderKind.Managed && !providerName.Contains("Managed", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Expected Managed provider, got: {providerName}");

        if (provider == ProviderKind.MKL && !providerName.Contains("MKL", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Expected MKL provider, got: {providerName}");

        if (provider == ProviderKind.OpenBLAS && !providerName.Contains("OpenBLAS", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Expected OpenBLAS provider, got: {providerName}");
    }
}

public static class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<LinearAlgebraProviderBenchmarks>();
    }
}
