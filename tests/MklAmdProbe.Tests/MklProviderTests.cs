using System;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Providers.LinearAlgebra;
using Xunit;

namespace MklAmdProbe.Tests;

public class MklProviderTests
{
    [Fact]
    public void MklProvider_CanBeLoaded()
    {
        Control.UseNativeMKL();
        var provider = LinearAlgebraControl.Provider;
        var providerName = provider.GetType().FullName ?? provider.GetType().Name;

        Assert.Contains("Mkl", providerName, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MatrixMultiply_ReturnsExpectedValues_WithMklProvider()
    {
        Control.UseNativeMKL();

        var a = Matrix<double>.Build.DenseOfArray(new double[,]
        {
            { 1, 2 },
            { 3, 4 }
        });

        var b = Matrix<double>.Build.DenseOfArray(new double[,]
        {
            { 5, 6 },
            { 7, 8 }
        });

        var c = a * b;

        Assert.InRange(c[0, 0], 19 - 1e-12, 19 + 1e-12);
        Assert.InRange(c[0, 1], 22 - 1e-12, 22 + 1e-12);
        Assert.InRange(c[1, 0], 43 - 1e-12, 43 + 1e-12);
        Assert.InRange(c[1, 1], 50 - 1e-12, 50 + 1e-12);
    }
}
