﻿// ---------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DMX.Core.Api.Brokers.LabApis;
using DMX.Core.Api.Brokers.Loggings;
using DMX.Core.Api.Models.Foundations.ExternalLabs;
using DMX.Core.Api.Models.Foundations.Labs;

namespace DMX.Core.Api.Services.Foundations.ExternalLabs
{
    public partial class ExternalLabService : IExternalLabService
    {
        private readonly IExternalLabApiBroker externalLabApiBroker;
        private readonly ILoggingBroker loggingBroker;

        public ExternalLabService(
            IExternalLabApiBroker externalLabApiBroker,
            ILoggingBroker loggingBroker)
        {
            this.externalLabApiBroker = externalLabApiBroker;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<List<Lab>> RetrieveAllExternalLabsAsync() =>
        TryCatch(async () =>
        {
            List<ExternalLab> externalLabs = await RetrieveExternalLabsAsync();

            return externalLabs.Select(AsLab).ToList();
        });

        private async ValueTask<List<ExternalLab>> RetrieveExternalLabsAsync()
        {
            var externalLabServiceInformation = new ExternalLabServiceInformation
            {
                ServiceId = "Bondi-HW-Lab",
                ServiceType = "AzureIotHub"
            };

            ExternalLabCollection externalLabCollection =
                await this.externalLabApiBroker.GetAvailableLabsAsync(
                    externalLabServiceInformation);

            List<ExternalLab> externalLabs =
                externalLabCollection.ExternalLabs.ToList();

            return externalLabs;
        }

        private readonly Func<ExternalLab, Lab> AsLab = externalLab =>
        {
            return new Lab
            {
                ExternalId = externalLab.Id,
                Name = externalLab.Name,
                Status = RetrieveLabStatus(externalLab),
                Devices = RetrieveDevices(externalLab)
            };
        };

        private static LabStatus RetrieveLabStatus(ExternalLab externalLab)
        {
            return (externalLab.IsConnected, externalLab.IsReserved) switch
            {
                (false, _) => LabStatus.Offline,
                (true, false) => LabStatus.Available,
                _ => LabStatus.Reserved,
            };
        }

        private static List<LabDevice> RetrieveDevices(ExternalLab externalLab)
        {
            var devices = new List<LabDevice>();

            FindAddLabDevice(
                devices: devices,
                externalLab: externalLab,
                deviceName: "Host",
                category: LabDeviceCategory.Host,
                type: LabDeviceType.PC);

            FindAddLabDevice(
                devices: devices,
                externalLab: externalLab,
                deviceName: "Phone",
                category: LabDeviceCategory.Attachment,
                type: LabDeviceType.Phone);

            FindAddLabDevice(
                devices: devices,
                externalLab: externalLab,
                deviceName: "HMD",
                category: LabDeviceCategory.Attachment,
                type: LabDeviceType.HeadMountedDisplay);

            return devices;
        }

        private static void FindAddLabDevice(
            List<LabDevice> devices,
            ExternalLab externalLab,
            string deviceName,
            LabDeviceCategory category,
            LabDeviceType type)
        {
            bool isDeviceExist = externalLab.Properties.Any(property =>
                property.Key.Contains(@$"{deviceName}\"));

            externalLab.Properties.TryGetValue(
                key: @$"{deviceName}\isconnected",
                value: out string isConnected);

            externalLab.Properties.TryGetValue(
                key: @$"{deviceName}\name",
                value: out string name);

            externalLab.Properties.TryGetValue(
                key: @$"{deviceName}\powerlevel",
                value: out string powerLevelText);

            if (isDeviceExist)
            {
                devices.Add(new LabDevice
                {
                    Name = name,
                    Category = category,
                    Type = type,
                    PowerLevel = GetPowerLevel(powerLevelText),

                    Status = bool.Parse(isConnected)
                        ? LabDeviceStatus.Online
                        : LabDeviceStatus.Offline,
                });
            }
        }

        private static int? GetPowerLevel(string powerLevelText)
        {
            return powerLevelText switch
            {
                { } => Int32.Parse(powerLevelText),
                _ => null,
            };
        }
    }
}
