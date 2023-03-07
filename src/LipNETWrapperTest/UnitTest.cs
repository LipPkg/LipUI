using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using LipNETWrapper;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using NUnit.Framework.Internal;

namespace LipNETWrapperTest
{
    public class Tests
    {
        private void Sep()
        {
            Console.WriteLine("---");
        }
        private void OutPut(object? obj, [CallerArgumentExpression(nameof(obj))] string? expression = null)
        {
            if (obj is null)
            {
                Console.WriteLine(expression + " = [null]");
            }
            else
            {
                Console.WriteLine(expression + " = " + obj);

            }
        }
        [SetUp]
        public void Setup()
        {
            var (success, path) = Utils.TryGetLipFromPath();
            //if (!success)
            //{
            //    path=
            //}
            foreach (var workingDir in new[]
                     {
                         "A:\\Documents\\GitHub\\BDS\\Latest\\",
                         //put your path here
                     })
            {
                if (Directory.Exists(workingDir))
                {
                    _loader = new LipNETWrapper.LipConsoleWrapper(success ? path! :
                        Path.Combine(workingDir, "lip.exe"), workingDir);
                    break;
                }
            }
            if (_loader == null)
                Assert.Fail("please put your lip.exe path");
        }
        LipNETWrapper.LipConsoleWrapper Loader => _loader!;
        LipNETWrapper.LipConsoleWrapper? _loader;
        [Test]
        public async Task TestLipVersion()
        {
            var result = await Loader.GetLipVersion();
            Assert.Pass(result);
        }
        [Test]
        public async Task TestGetAllPackages()
        {
            var (packages, message) = await Loader.GetAllPackagesAsync();
            OutPut(packages.Length.ToString());
            foreach (var item in packages)
            {
                OutPut("Tooth = " + item.Tooth);
                OutPut("Version = " + item.Version);
                OutPut("-----");
            }
            OutPut(message);
        }

        [Test]
        public async Task TestGetPackageInfo()
        {
            //github.com/tooth-hub/liteloaderbds
            var (success, package, message) = await Loader.GetPackageInfoAsync("github.com/tooth-hub/liteloaderbds");
            OutPut(success.ToString());
            if (success)
            {
                if (package!.Versions is not null)
                {
                    foreach (var v in package!.Versions)
                    {
                        OutPut(v);
                    }
                }
            }
            OutPut("----------");
            OutPut(message);
        }

        [Test]
        public async Task TestGetLocalPackageInfo()
        {
            //github.com/tooth-hub/liteloaderbds
            var (success, package, message) = await Loader.GetLocalPackageInfoAsync("github.com/tooth-hub/liteloaderbds");
            OutPut(success.ToString());
            OutPut(package?.Name);
            OutPut(package?.Version);
            OutPut("----------");
            OutPut(message);
        }
        [Test]
        public async Task TestInstallPackage()
        {
            var result = await Loader.InstallPackageAsync("github.com/tooth-hub/liteloaderbds", onOutput: (s, input) =>
            {
                OutPut(s);
            });
            OutPut("exit code : " + result);
        }

        [Test]
        public async Task TestGetLipRegistry()
        {
            var registry = await Loader.GetLipRegistryAsync("https://registry.litebds.com/index.json");
            //output all info
            OutPut(registry.FormatVersion);
            foreach (var x in registry.Index)
            {
                Sep();
                OutPut(x.Key);
                OutPut(x.Value.Tooth);
            }
        }
        [Test]
        public void TestLipPath()
        {
            //OutPut(Environment.GetEnvironmentVariable("PATH"));
            OutPut(Utils.TryGetLipFromPath());
        }
        [Test]
        public void TestLipDownload()
        {
            //OutPut(Environment.GetEnvironmentVariable("PATH"));
            OutPut(Utils.TryGetLipFromPath());
        }
    }
}