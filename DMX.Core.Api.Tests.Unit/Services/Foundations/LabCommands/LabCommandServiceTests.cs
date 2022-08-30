﻿// ---------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using DMX.Core.Api.Brokers.DateTimes;
using DMX.Core.Api.Brokers.Loggings;
using DMX.Core.Api.Brokers.Storages;
using DMX.Core.Api.Models.Foundations.LabCommands;
using DMX.Core.Api.Services.Foundations.LabCommands;
using Microsoft.Data.SqlClient;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;
using Xunit;

namespace DMX.Core.Api.Tests.Unit.Services.Foundations.LabCommands
{
    public partial class LabCommandServiceTests
    {
        private readonly Mock<IStorageBroker> storageBrokerMock;
        private readonly Mock<IDateTimeBroker> dateTimeBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly ILabCommandService labCommandService;

        public LabCommandServiceTests()
        {
            this.storageBrokerMock = new Mock<IStorageBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();
            this.dateTimeBrokerMock = new Mock<IDateTimeBroker>();

            this.labCommandService = new LabCommandService(
                storageBroker: this.storageBrokerMock.Object,
                dateTimeBroker: this.dateTimeBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object);
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption exception) =>
            actualException => actualException.SameExceptionAs(exception);

        private static T GetInvalidEnum<T>()
        {
            int randomNumber = GetRandomNumber();

            while (Enum.IsDefined(typeof(T), randomNumber))
            {
                randomNumber = GetRandomNumber();
            }

            return (T)(object)randomNumber;
        }

        private static DateTimeOffset GetValidDateTimeOffset(DateTimeOffset dateTimeOffset, TimeSpan timeWindow) =>
            GetRandomDateTimeOffset(dateTimeOffset, dateTimeOffset.Add(timeWindow));

        public static TheoryData<int> MinuteBeforeAndAfter()
        {
            var randomNumber = GetRandomNumber();
            var randomNegativeNumber = GetRandomNumber() * -1;

            return new TheoryData<int>
            {
                randomNumber,
                randomNegativeNumber
            };
        }

        public static TheoryData InvalidSeconds()
        {
            int secondsInPast =
                new IntRange(
                    min: 60,
                    max: int.MaxValue).GetValue() * -1;

            int secondsInFuture =
                new IntRange(
                    min: 0,
                    max: int.MaxValue).GetValue();

            return new TheoryData<int>
            {
                secondsInPast,
                secondsInFuture
            };
        }

        private static int GetRandomNegativeNumber() =>
            GetRandomNumber() * -1;

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static string GetRandomString() =>
            new MnemonicString().GetValue();

        private static SqlException GetSqlException() =>
            (SqlException)FormatterServices.GetUninitializedObject(typeof(SqlException));

        private static LabCommand CreateRandomLabCommand() =>
            CreateLabCommandFiller(GetRandomDateTimeOffset()).Create();

        private static LabCommand CreateRandomLabCommand(DateTimeOffset date) =>
            CreateLabCommandFiller(date).Create();

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: DateTime.UnixEpoch).GetValue();

        private static DateTimeOffset GetRandomDateTimeOffset(DateTimeOffset earliestDate) =>
            new DateTimeRange(earliestDate: earliestDate.DateTime, TimeZoneInfo.Utc).GetValue();

        private static DateTimeOffset GetRandomDateTimeOffset(DateTimeOffset earliestDate, DateTimeOffset latestDate) =>
            new DateTimeRange(earliestDate: earliestDate.DateTime, latestDate: latestDate.DateTime, TimeZoneInfo.Utc).GetValue();

        private static Filler<LabCommand> CreateLabCommandFiller(DateTimeOffset dateTimeOffset)
        {
            var filler = new Filler<LabCommand>();

            filler.Setup()
                .OnType<DateTimeOffset>()
                    .Use(dateTimeOffset);

            return filler;
        }
    }
}
