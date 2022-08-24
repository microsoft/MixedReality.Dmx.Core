﻿// ---------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.AppService.Fluent.Models;
using Microsoft.Azure.Management.AppService.Fluent.WebApp.Definition;
using Microsoft.Azure.Management.ResourceManager.Fluent;

namespace DMX.Core.Api.Infrastructure.Provision.Brokers.Clouds
{
    public partial class CloudBroker
    {
        public async ValueTask<IWebApp> CreateWebAppAsync(
            string webAppName,
            string connectionString,
            IAppServicePlan appServicePlan,
            IResourceGroup resourceGroup)
        {
            var webAppSettings = new Dictionary<string, string>
                {
                    { "ASPNETCORE_ENVIRONMENT", DmxEnvironment },
                    { "ApiConfigurations:Url", this.configurationExternalLabApiUrl },
                    { "ApiConfigurations:AccessKey", this.configurationExternalLabApiAccessKey },
                    { "AzureAd:TenantId", this.tenantId },
                    { "AzureAd:Instance", this.dmxCoreInstance },
                    { "AzureAd:Domain", this.dmxCoreDomain },
                    { "AzureAd:ClientId", this.dmxCoreClientId },
                    { "AzureAd:CallbackPath", this.dmxCoreCallbackPath },
                    { "AzureAd:Scopes:GetAllLabs", this.dmxCoreScopesGetAllLabs },
                    { "AzureAd:Scopes:PostLab", this.dmxCoreScopesPostLab },
                    { "AzureAd:Scopes:DeleteLab", this.dmxCoreScopesDeleteLab },
                    { "AzureAd:Scopes:PostLabCommand", this.dmxCoreScopesPostLabCommand },
                    { "AzureAd:Scopes:GetLabCommand", this.dmxCoreScopesGetLabCommand},
                    { "AzureAd:Scopes:PutLabCommand", this.dmxCoreScopesPutLabCommand},
                };

            IWithWindowsRuntimeStack webAppWithPlanAndResouceGroup =
                this.azure.AppServices.WebApps
                    .Define(webAppName)
                        .WithExistingWindowsPlan(appServicePlan)
                            .WithExistingResourceGroup(resourceGroup);

            return await webAppWithPlanAndResouceGroup
                .WithNetFrameworkVersion(NetFrameworkVersion.Parse("v6.0"))
                    .WithConnectionString(
                        name: "DefaultConnection",
                        value: connectionString,
                        type: ConnectionStringType.SQLAzure)
                            .WithAppSettings(settings: webAppSettings)
                                .CreateAsync();
        }
    }
}
