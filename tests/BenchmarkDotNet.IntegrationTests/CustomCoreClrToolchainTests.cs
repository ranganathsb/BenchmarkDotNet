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
                    CustomCoreClrToolchain.CreateBuilder()
                        .UseCoreClrDefault()
                        .UseCoreFxNuGet("4.5.0-preview3-26328-01")
                        .ToToolchain()));

            CanExecute<CheckCoreFxVersion>(config);
        }

        public class CheckCoreFxVersion
        {
            [Benchmark]
            public void Check()
            {
                var coreFxAssemblyInfo = FileVersionInfo.GetVersionInfo(typeof(Regex).GetTypeInfo().Assembly.Location);

                if (coreFxAssemblyInfo.FileVersion != "4.6.26328.01")
                    throw new InvalidOperationException($"Wrong CoreFx version: was {coreFxAssemblyInfo.FileVersion}, should be 4.6.26328.01");
            }
        }

        [Fact]
        public void CanBenchmarkGivenCoreClrMyGetBuild()
        {
            var config = ManualConfig.CreateEmpty()
                .With(Job.Dry.With(
                    CustomCoreClrToolchain.CreateBuilder()
                        .UseCoreFxDefault()
                        .UseCoreClrNuGet("2.1.0-preview3-26329-08")
                        .ToToolchain()));

            CanExecute<CheckCoreClrVersion>(config);
        }

        public class CheckCoreClrVersion
        {
            [Benchmark]
            public void Check()
            {
                var coreClrAssemblyInfo = FileVersionInfo.GetVersionInfo(typeof(object).GetTypeInfo().Assembly.Location);

                if (coreClrAssemblyInfo.FileVersion != "4.6.26329.08")
                    throw new InvalidOperationException($"Wrong CoreClr version: was {coreClrAssemblyInfo.FileVersion}, should be 4.6.26329.08");
            }
        }

        [Fact]
        public void CanBenchmarkGivenCoreClrAndCoreFxMyGetBuilds()
        {
            var config = ManualConfig.CreateEmpty()
                .With(Job.Dry.With(
                    CustomCoreClrToolchain.CreateBuilder()
                        .UseCoreFxNuGet("4.5.0-preview3-26328-01")
                        .UseCoreClrNuGet("2.1.0-preview3-26329-08")
                        .ToToolchain()));

            CanExecute<CheckCoreClrAndCoreFxVersions>(config);
        }

        public class CheckCoreClrAndCoreFxVersions
        {
            [Benchmark]
            public void Check()
            {
                var coreFxAssemblyInfo = FileVersionInfo.GetVersionInfo(typeof(Regex).GetTypeInfo().Assembly.Location);

                if (coreFxAssemblyInfo.FileVersion != "4.6.26328.01")
                    throw new InvalidOperationException($"Wrong CoreFx version: was {coreFxAssemblyInfo.FileVersion}, should be 4.6.26328.01");

                var coreClrAssemblyInfo = FileVersionInfo.GetVersionInfo(typeof(object).GetTypeInfo().Assembly.Location);

                if (coreClrAssemblyInfo.FileVersion != "4.6.26329.08")
                    throw new InvalidOperationException($"Wrong CoreFx version: was {coreClrAssemblyInfo.FileVersion}, should be 4.6.26329.08");
            }
        }

        [Fact(Skip = "You need to setup CoreFx and CoreClr on your machine to run it and update the paths..")]
        public void CanBenchmarkGivenLocalCoreClrAndCoreFxBuilds()
        {
            var config = ManualConfig.CreateEmpty()
                .With(Job.Dry.With(
                    CustomCoreClrToolchain.CreateBuilder()
                        .UseCoreFxLocalBuild("4.5.0-preview3-26427-0", @"C:\Projects\corefx\bin\packages\Debug")
                        .UseCoreClrLocalBuild("2.1.0-preview3-26420-0", @"C:\Projects\coreclr\bin\Product\Windows_NT.x64.Debug\.nuget\pkg", @"C:\Projects\coreclr\packages\")
                        .UseNuGetClearTag(false) // for now I need it
                        .ToToolchain()));

            CanExecute<CheckLocalCoreClrAndCoreFxVersions>(config);
        }

        public class CheckLocalCoreClrAndCoreFxVersions
        {
            [Benchmark]
            public void Check()
            {
                var coreFxAssemblyInfo = FileVersionInfo.GetVersionInfo(typeof(Regex).GetTypeInfo().Assembly.Location);

                if (coreFxAssemblyInfo.FileVersion != "4.6.26427.0")
                    throw new InvalidOperationException($"Wrong CoreFx version: was {coreFxAssemblyInfo.FileVersion}, should be 4.6.26427.0");

                var coreClrAssemblyInfo = FileVersionInfo.GetVersionInfo(typeof(object).GetTypeInfo().Assembly.Location);

                if (coreClrAssemblyInfo.FileVersion != "4.6.26420.0")
                    throw new InvalidOperationException($"Wrong CoreFx version: was {coreClrAssemblyInfo.FileVersion}, should be 4.6.26420.0");
            }
        }
    }
}
#endif