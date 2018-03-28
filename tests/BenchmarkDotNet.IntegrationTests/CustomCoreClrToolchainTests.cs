#if NETCOREAPP2_1
using System;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.CustomCoreClr;
using Xunit;
using Xunit.Abstractions;

namespace BenchmarkDotNet.IntegrationTests
{
    public class CustomCoreClrToolchainTests : BenchmarkTestExecutor
    {
        public CustomCoreClrToolchainTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void CanBenchmarkGivenCoreFxMyGetBuild()
        {
            var config = ManualConfig.CreateEmpty()
                .With(Job.Dry.With(
                    CustomCoreClrToolchain.CreateForNightlyCoreFxBuild("4.5.0-preview3-26328-01")));

            CanExecute<CheckCoreFxVersion>(config);
        }
    }

    public class CheckCoreFxVersion
    {
        [Benchmark]
        public void Check()
        {
            var corefxAssemblyInfo = FileVersionInfo.GetVersionInfo(typeof(Regex).GetTypeInfo().Assembly.Location);

            if (corefxAssemblyInfo.FileVersion != "4.6.26328.01")
                throw new InvalidOperationException("Wrong version");
        }
    }
}
#endif