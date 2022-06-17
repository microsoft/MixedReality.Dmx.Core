﻿// ---------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using DMX.Core.Api.Brokers.LabApis;
using DMX.Core.Api.Brokers.Loggings;
using DMX.Core.Api.Brokers.Storages;
using DMX.Core.Api.Models.Externals.ExternalLabs;
using DMX.Core.Api.Models.Labs;
using DMX.Core.Api.Services.Foundations.Labs;
using KellermanSoftware.CompareNetObjects;
using Microsoft.AspNetCore.Http;
using Moq;
using RESTFulSense.Exceptions;
using Tynamix.ObjectFiller;
using Xeptions;
using Xunit;

namespace DMX.Core.Api.Tests.Unit.Services.Foundations
{
    public partial class LabServiceTests
    {
        private readonly Mock<ILabApiBroker> labApiBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly Mock<IStorageBroker> storageBrokerMock;
        private readonly ICompareLogic compareLogic;
        private readonly ILabService labService;

        public LabServiceTests()
        {
            this.labApiBrokerMock = new Mock<ILabApiBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();
            this.storageBrokerMock = new Mock<IStorageBroker>();
            this.compareLogic = new CompareLogic();

            this.labService = new LabService(
                labApiBroker: this.labApiBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object,
                storageBroker: this.storageBrokerMock.Object);
        }

        public static TheoryData CriticalDependencyException()
        {
            string someMessage = GetRandomString();
            var someResponseMessage = new HttpResponseMessage();

            return new TheoryData<Xeption>()
            {
                new HttpResponseUrlNotFoundException(someResponseMessage, someMessage),
                new HttpResponseUnauthorizedException(someResponseMessage, someMessage),
                new HttpResponseForbiddenException(someResponseMessage, someMessage)
            };
        }

        private Expression<Func<ExternalLabServiceInformation, bool>> SameInformationAs(
            ExternalLabServiceInformation expectedExternalLabServiceInformation)
        {
            return actualExternalLabServiceInformation =>
                this.compareLogic.Compare(
                    expectedExternalLabServiceInformation,
                    actualExternalLabServiceInformation)
                        .AreEqual;
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        private static List<dynamic> CreateRandomLabsProperties()
        {
            int randomCount = GetRandomNumber();

            (IDictionary<string, string> randomProperties, List<LabDevice> randomDevices) =
                GetRandomLabProperties();

            var allCases = new List<dynamic>
            {
                new
                {
                    Id = GetRandomString(),
                    Name = GetRandomString(),
                    IsConnected = true,
                    IsReserved = false,
                    LabStatus = LabStatus.Available,
                    Properties = randomProperties,
                    Devices = randomDevices
                },

                new
                {
                    Id = GetRandomString(),
                    Name = GetRandomString(),
                    IsConnected = true,
                    IsReserved = true,
                    LabStatus = LabStatus.Reserved,
                    Properties = randomProperties,
                    Devices = randomDevices
                },

                new
                {
                    Id = GetRandomString(),
                    Name = GetRandomString(),
                    IsConnected = false,
                    IsReserved = GetRandomBoolean(),
                    LabStatus = LabStatus.Offline,
                    Properties = randomProperties,
                    Devices = randomDevices
                }
            };

            return Enumerable.Range(start: 0, count: randomCount)
                .Select(iterator => allCases)
                    .SelectMany(@case => @case)
                        .ToList();
        }

        private static bool GetRandomBoolean() => new Random().Next(2) == 1;

        private static string GetRandomString() =>
            new MnemonicString().GetValue();

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static int GetRandomPowerLevel() =>
            new IntRange(min: 0, max: 101).GetValue();

        private static ExternalLabCollection CreateRandomLabCollection() =>
            CreateExternalLabCollectionFiller().Create();

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: new DateTime()).GetValue();

        private static Filler<ExternalLabCollection> CreateExternalLabCollectionFiller()
        {
            var filler = new Filler<ExternalLabCollection>();

            filler.Setup()
                .OnType<DateTimeOffset?>().Use(GetRandomDateTimeOffset());

            return filler;
        }

        private static (IDictionary<string, string>, List<LabDevice>) GetRandomLabProperties()
        {
            string randomPhoneName = GetRandomString();
            string randomHMDName = GetRandomString();
            bool randomHostConnectionStatus = GetRandomBoolean();
            bool randomPhoneConnectionStatus = GetRandomBoolean();
            bool randomHMDConnectionStatus = GetRandomBoolean();
            int randomPhonePowerLevel = GetRandomPowerLevel();
            int randomHMDPowerLevel = GetRandomPowerLevel();

            var externalDeviceProperties = new Dictionary<string, string>
            {
                { @"Host\isconnected", $"{randomHostConnectionStatus}" },
                { @"Phone\name", randomPhoneName },
                { @"Phone\isconnected", $"{randomPhoneConnectionStatus}" },
                { @"Phone\powerlevel", $"{randomPhonePowerLevel}" },
                { @"HMD\name", randomHMDName },
                { @"HMD\isconnected", $"{randomHMDConnectionStatus}" },
                { @"HMD\powerlevel", $"{randomHMDPowerLevel}" },
            };

            var labDevices = new List<LabDevice>
            {
                new LabDevice
                {
                    Name = null,
                    Type = LabDeviceType.PC,
                    Category = LabDeviceCategory.Host,
                    PowerLevel = null,

                    Status = randomHostConnectionStatus
                        ? LabDeviceStatus.Online
                        : LabDeviceStatus.Offline,
                },

                new LabDevice
                {
                    Name = randomPhoneName,
                    Type = LabDeviceType.Phone,
                    Category = LabDeviceCategory.Attachment,
                    PowerLevel = randomPhonePowerLevel,

                    Status = randomPhoneConnectionStatus
                        ? LabDeviceStatus.Online
                        : LabDeviceStatus.Offline,
                },

                new LabDevice
                {
                    Name = randomHMDName,
                    Type = LabDeviceType.HeadMountedDisplay,
                    Category = LabDeviceCategory.Attachment,
                    PowerLevel = randomHMDPowerLevel,

                    Status = randomHMDConnectionStatus
                        ? LabDeviceStatus.Online
                        : LabDeviceStatus.Offline,
                },
            };

            return (externalDeviceProperties, labDevices);
        }

        private static Lab CreateRandomLab() =>
            CreateLabFiller().Create();

        private static Filler<Lab> CreateLabFiller() =>
            new Filler<Lab>();
    }
}
