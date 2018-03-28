﻿using BenchmarkDotNet.Toolchains.DotNetCli;
using System;
using System.IO;

namespace BenchmarkDotNet.Toolchains.CustomCoreClr
{
    public class CustomCoreClrToolchain : Toolchain
    {
        /// <summary>
        /// creates toolchain which uses dotnet cli publish to generated self-contained app that references given CoreCLR and CoreFX
        /// </summary>
        /// <param name="displayName">the name of the toolchain to be displayed in results</param>
        /// <param name="coreClrNuGetFeed">path to folder for local CoreCLR builds, url to MyGet feed for previews of CoreCLR. Example: "C:\coreclr\bin\Product\Windows_NT.x64.Debug\.nuget\pkg"</param>
        /// <param name="coreClrVersion">the version of Microsoft.NETCore.Runtime which should be used. Example: "2.1.0-preview2-26305-0"</param>
        /// <param name="coreFxNuGetFeed">path to folder for local CoreFX builds, url to MyGet feed for previews of CoreFX. Example: "C:\Projects\forks\corefx\bin\packages\Debug"</param>
        /// <param name="coreFxVersion">the version of Microsoft.Private.CoreFx.NETCoreApp which should be used. Example: 4.5.0-preview2-26307-0</param>
        /// <param name="targetFrameworkMoniker">TFM, netcoreapp2.1 is the default</param>
        /// <param name="runtimeIdentifier">if not provided, portable OS-arch will be used (example: "win-x64", "linux-x86")</param>
        /// <param name="customDotNetCliPath">if not provided, the default will be used</param>
        /// <returns></returns>
        public CustomCoreClrToolchain(
            string displayName,
            string coreClrNuGetFeed, string coreClrVersion, 
            string coreFxNuGetFeed, string coreFxVersion,
            string targetFrameworkMoniker = "netcoreapp2.1",
            string runtimeIdentifier = null, 
            string customDotNetCliPath = null)
            : base(displayName, 
                  new Generator(coreClrNuGetFeed, coreClrVersion, coreFxNuGetFeed, coreFxVersion, targetFrameworkMoniker, runtimeIdentifier),
                  new Publisher(customDotNetCliPath),
                  new DotNetCliExecutor(customDotNetCliPath))
        {
        }

        /// <summary>
        /// creates a toolchain which publishes self-contained app which references local CoreFx build
        /// as described here https://github.com/dotnet/corefx/blob/master/Documentation/project-docs/dogfooding.md#more-advanced-scenario---using-your-local-corefx-build
        /// </summary>
        /// <param name="pathToNuGetFolder">path to folder with CoreFX NuGet packages, Example: "C:\Projects\forks\corefx\bin\packages\Release"</param>
        /// <param name="privateCoreFxNetCoreAppVersion">the version of Microsoft.Private.CoreFx.NETCoreApp which should be used. Example: "4.5.0-preview2-26307-0"</param>
        /// <param name="targetFrameworkMoniker">TFM, netcoreapp2.1 is the default</param>
        /// <param name="runtimeIdentifier">if not provided, portable OS-arch will be used (example: "win-x64", "linux-x86")</param>
        /// <param name="customDotNetCliPath">if not provided, the default will be used</param>
        /// <param name="displayName">the name of the toolchain to be displayed in results, the default is "localCoreFX"</param>
        /// <returns></returns>
        public static IToolchain CreateForLocalCoreFxBuild(string pathToNuGetFolder, string privateCoreFxNetCoreAppVersion, 
            string targetFrameworkMoniker = "netcoreapp2.1", string displayName = "localCoreFX", string runtimeIdentifier = null, string customDotNetCliPath = null)
        {
            if (!Directory.Exists(pathToNuGetFolder))
                throw new ArgumentException($"Directory {pathToNuGetFolder} does not exist");

            runtimeIdentifier = runtimeIdentifier ?? CustomCoreClr.Generator.GetPortableRuntimeIdentifier();

            var expectedPackageFileName = $"runtime.{runtimeIdentifier}.Microsoft.Private.CoreFx.NETCoreApp.{privateCoreFxNetCoreAppVersion}.nupkg";
            if (!File.Exists(Path.Combine(pathToNuGetFolder, expectedPackageFileName)))
                throw new ArgumentException($"Expected package {expectedPackageFileName} was not found in {pathToNuGetFolder}. Please make sure you have provided the right version number");

            return new CustomCoreClrToolchain(displayName, null, null, pathToNuGetFolder, privateCoreFxNetCoreAppVersion, targetFrameworkMoniker, runtimeIdentifier, customDotNetCliPath);
        }

        /// <summary>
        /// creates a toolchain which publishes self-contained app which references local CoreClr build
        /// as described here https://github.com/dotnet/coreclr/blob/master/Documentation/workflow/UsingDotNetCli.md
        /// </summary>
        /// <param name="coreClrVersion">the version of Microsoft.NETCore.Runtime which should be used. Example: "2.1.0-preview2-26305-0"</param>
        /// <param name="privateCoreFxNetCoreAppVersion">the version of Microsoft.Private.CoreFx.NETCoreApp which should be used. Example: "4.5.0-preview2-26307-0"</param>
        /// <param name="coreClrProductPackagesPath">path to folder with CoreClr NuGet packages. Example: "C:\coreclr\bin\Product\Windows_NT.x64.Release\.nuget\pkg"</param>
        /// <param name="packagesPath">path to folder with NuGet packages. Example: "C:\Projects\coreclr\packages"</param>
        /// <param name="targetFrameworkMoniker">TFM, netcoreapp2.1 is the default</param>
        /// <param name="runtimeIdentifier">if not provided, portable OS-arch will be used (example: "win-x64", "linux-x86")</param>
        /// <param name="customDotNetCliPath">if not provided, the default will be used</param>
        /// <param name="displayName">the name of the toolchain to be displayed in results, the default is "localCoreFX"</param>
        /// <returns></returns>
        public static IToolchain CreateForLocalCoreClrBuild(string coreClrVersion, string privateCoreFxNetCoreAppVersion, string coreClrProductPackagesPath, string packagesPath,
            string targetFrameworkMoniker = "netcoreapp2.1", string displayName = "localCoreClr", string runtimeIdentifier = null, string customDotNetCliPath = null)
        {
            if (string.IsNullOrEmpty(coreClrVersion)) throw new ArgumentNullException(nameof(coreClrVersion));
            if (string.IsNullOrEmpty(privateCoreFxNetCoreAppVersion)) throw new ArgumentNullException(nameof(privateCoreFxNetCoreAppVersion));
            if (!Directory.Exists(coreClrProductPackagesPath)) throw new ArgumentException($"Directory {coreClrProductPackagesPath} does not exist");
            if (!Directory.Exists(packagesPath)) throw new ArgumentException($"Directory {packagesPath} does not exist");

            runtimeIdentifier = runtimeIdentifier ?? CustomCoreClr.Generator.GetPortableRuntimeIdentifier();

            VerifyPackageExists("Microsoft.NETCore.Runtime.CoreCLR", coreClrVersion, coreClrProductPackagesPath, runtimeIdentifier);
            VerifyPackageExists("Microsoft.NETCore.Jit", coreClrVersion, coreClrProductPackagesPath, runtimeIdentifier);

            return new CustomCoreClrToolchain(displayName, coreClrProductPackagesPath, coreClrVersion, packagesPath, privateCoreFxNetCoreAppVersion, targetFrameworkMoniker, runtimeIdentifier, customDotNetCliPath);
        }

        /// <summary>
        /// creates a toolchain which publishes self-contained app which references nightly CoreFx build
        /// as described here https://github.com/dotnet/corefx/blob/master/Documentation/project-docs/dogfooding.md#advanced-scenario---using-a-nightly-build-of-microsoftnetcoreapp
        /// </summary>
        /// <param name="privateCoreFxNetCoreAppVersion">the version of Microsoft.Private.CoreFx.NETCoreApp which should be used. Example: "4.5.0-preview2-26307-0"</param>
        /// <param name="nugetFeedUrl">ulr to NuGet CoreFX feed, The default is: "https://dotnet.myget.org/F/dotnet-core/api/v3/index.json"</param>
        /// <param name="targetFrameworkMoniker">TFM, netcoreapp2.1 is the default</param>
        /// <param name="runtimeIdentifier">if not provided, portable OS-arch will be used (example: "win-x64", "linux-x86")</param>
        /// <param name="customDotNetCliPath">if not provided, the default will be used</param>
        /// <param name="displayName">the name of the toolchain to be displayed in results, the default is "nightlyCoreFX"</param>
        /// <returns></returns>
        public static IToolchain CreateForNightlyCoreFxBuild(string privateCoreFxNetCoreAppVersion,
            string nugetFeedUrl = "https://dotnet.myget.org/F/dotnet-core/api/v3/index.json",
            string targetFrameworkMoniker = "netcoreapp2.1", string displayName = "nightlyCoreFX", string runtimeIdentifier = null, string customDotNetCliPath = null)
        {
            if (string.IsNullOrEmpty(privateCoreFxNetCoreAppVersion)) throw new ArgumentNullException(nameof(privateCoreFxNetCoreAppVersion));
            if (string.IsNullOrEmpty(nugetFeedUrl)) throw new ArgumentNullException(nameof(nugetFeedUrl));

            runtimeIdentifier = runtimeIdentifier ?? CustomCoreClr.Generator.GetPortableRuntimeIdentifier();

            return new CustomCoreClrToolchain(displayName, null, null, nugetFeedUrl, privateCoreFxNetCoreAppVersion, targetFrameworkMoniker, runtimeIdentifier, customDotNetCliPath);
        }

        private static void VerifyPackageExists(string packageName, string coreClrVersion, string coreClrProductPackagesPath, string runtimeIdentifier)
        {
            var expectedPackageFileName = $"runtime.{runtimeIdentifier}.{packageName}.{coreClrVersion}.nupkg";
            if (!File.Exists(Path.Combine(coreClrProductPackagesPath, expectedPackageFileName)))
                throw new ArgumentException(
                    $"Expected package {expectedPackageFileName} was not found in {coreClrProductPackagesPath}. Please make sure you have provided the right version number");
        }
    }
}
