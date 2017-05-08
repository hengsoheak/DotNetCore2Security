// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.Extensions.Logging.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Owin.Security.Interop.FunctionalTest
{
    public class CookieInteropTests : LoggedTest, IDisposable
    {
        private readonly Lazy<Task<DeploymentResult>> _deployCoreOnce;
        private readonly Lazy<Task<DeploymentResult>> _deployKatanaOnce;

        public CookieInteropTests(ITestOutputHelper output) : base(output)
        {
            LogScope = StartLog(out var loggerFactory);

            var coreDeploymentParameters = new DeploymentParameters(
                Helpers.GetApplicationPath(ApplicationType.Portable, "Interop.Microsoft.AspNetCore.TestApp"),
                ServerType.Kestrel, RuntimeFlavor.CoreClr, RuntimeArchitecture.x64)
            {
                ApplicationType = ApplicationType.Portable
            };

            CoreDeployer = ApplicationDeployerFactory.Create(coreDeploymentParameters, loggerFactory);
            _deployCoreOnce = new Lazy<Task<DeploymentResult>>(() => CoreDeployer.DeployAsync());

            var katanaDeploymentParameters = new DeploymentParameters(
                Helpers.GetApplicationPath(ApplicationType.Standalone, "Interop.Microsoft.Owin.TestApp"),
                ServerType.WebListener, RuntimeFlavor.Clr, RuntimeArchitecture.x64)
            {
                ApplicationType = ApplicationType.Standalone
            };

            KatanaDeployer = ApplicationDeployerFactory.Create(katanaDeploymentParameters, loggerFactory);
            _deployKatanaOnce = new Lazy<Task<DeploymentResult>>(() => KatanaDeployer.DeployAsync());
        }

        public void Dispose()
        {
            CoreDeployer.Dispose();
            KatanaDeployer.Dispose();
            LogScope.Dispose();
        }

        public IDisposable LogScope { get; }

        public IApplicationDeployer CoreDeployer { get; }
        public IApplicationDeployer KatanaDeployer { get; }

        public Task<DeploymentResult> DeployCoreOnceAsync()
        {
            return _deployCoreOnce.Value;
        }

        public Task<DeploymentResult> DeployKatanaOnceAsync()
        {
            return _deployKatanaOnce.Value;
        }

        [Fact]
        public async Task CoreEchoEmptyUser()
        {
            var result = await DeployCoreOnceAsync();
            var noCookieResponse = await result.HttpClient.GetAsync("/echo");

            Assert.Equal("No User", await noCookieResponse.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task KatanaEchoEmptyUser()
        {
            var result = await DeployKatanaOnceAsync();
            var noCookieResponse = await result.HttpClient.GetAsync("/echo");

            Assert.Equal("No User", await noCookieResponse.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task CookieFromCoreCanBeReadByKatana()
        {
            var result = await DeployCoreOnceAsync();
            var noCookieResponse = await result.HttpClient.GetAsync("/echo");

            Assert.Equal("No User", await noCookieResponse.Content.ReadAsStringAsync());
        }
    }
}
