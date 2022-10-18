﻿// ---------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Threading.Tasks;
using DMX.Core.Api.Models.Foundations.LabWorkflows;
using DMX.Core.Api.Models.Foundations.LabWorkflows.Exceptions;
using EFxceptions.Models.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace DMX.Core.Api.Tests.Unit.Services.Foundations.LabWorkflows
{
    public partial class LabWorkflowTests
    {
        [Fact]
        public async Task ShouldThrowCriticalDependencyExceptionOnAddIfErrorOccursAndLogItAsync()
        {
            // given
            DateTimeOffset dateTime = GetRandomDateTimeOffset();
            LabWorkflow randomLabWorkflow = CreateRandomLabWorkflow(dateTime);
            LabWorkflow inputLabWorkflow = randomLabWorkflow;

            var sqlException = GetSqlException();

            var failedLabWorkflowStorageException =
                new FailedLabWorkflowStorageException(sqlException);

            var expectedLabWorkflowDependencyException =
                new LabWorkflowDependencyException(failedLabWorkflowStorageException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTime())
                    .Returns(dateTime);

            this.storageBrokerMock.Setup(broker =>
                broker.InsertLabWorkflowAsync(inputLabWorkflow))
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<LabWorkflow> addLabWorkflowTask =
                this.labWorkflowService.AddLabWorkflowAsync(inputLabWorkflow);

            // then
            await Assert.ThrowsAsync<LabWorkflowDependencyException>(() =>
                addLabWorkflowTask.AsTask());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCritical(It.Is(SameExceptionAs(expectedLabWorkflowDependencyException))),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTime(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertLabWorkflowAsync(inputLabWorkflow),
                    Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyValidationExceptionOnAddIfLabWorkflowAlreadyExistsAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            LabWorkflow someLabWorkflow = CreateRandomLabWorkflow(randomDateTimeOffset);
            string someMessage = GetRandomString();

            var duplicateKeyException =
                new DuplicateKeyException(someMessage);

            var alreadyExistsLabWorkflowException =
                new AlreadyExistsLabWorkflowException(duplicateKeyException);

            var expectedLabWorkflowDependencyValidationException =
                new LabWorkflowDependencyValidationException(alreadyExistsLabWorkflowException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTime())
                    .Returns(randomDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.InsertLabWorkflowAsync(It.IsAny<LabWorkflow>()))
                    .ThrowsAsync(duplicateKeyException);

            // when
            ValueTask<LabWorkflow> addLabWorkflowTask =
                this.labWorkflowService.AddLabWorkflowAsync(someLabWorkflow);

            LabWorkflowDependencyValidationException actualLabWorkflowDependencyValidationException =
                await Assert.ThrowsAsync<LabWorkflowDependencyValidationException>(
                    addLabWorkflowTask.AsTask);

            // then
            actualLabWorkflowDependencyValidationException.Should().BeEquivalentTo(
                expectedLabWorkflowDependencyValidationException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTime(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertLabWorkflowAsync(
                    It.IsAny<LabWorkflow>()),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedLabWorkflowDependencyValidationException))),
                        Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnAddIfDbUpdateErrorOccursAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            LabWorkflow someLabWorkflow = CreateRandomLabWorkflow(randomDateTimeOffset);

            var dbUpdateException =
                new DbUpdateException();

            var failedLabWorkflowStorageException =
                new FailedLabWorkflowStorageException(dbUpdateException);

            var expectedLabWorkflowDependencyException =
                new LabWorkflowDependencyException(failedLabWorkflowStorageException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTime())
                    .Returns(randomDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.InsertLabWorkflowAsync(It.IsAny<LabWorkflow>()))
                    .ThrowsAsync(dbUpdateException);

            // when
            ValueTask<LabWorkflow> addLabWorkflowTask =
                this.labWorkflowService.AddLabWorkflowAsync(someLabWorkflow);

            LabWorkflowDependencyException actualLabWorkflowDependencyException =
                await Assert.ThrowsAsync<LabWorkflowDependencyException>(
                    addLabWorkflowTask.AsTask);

            // then
            actualLabWorkflowDependencyException.Should().BeEquivalentTo(
                expectedLabWorkflowDependencyException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTime(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertLabWorkflowAsync(
                    It.IsAny<LabWorkflow>()),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedLabWorkflowDependencyException))),
                        Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnAddIfServiceErrorOccursAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            LabWorkflow someLabWorkflow = CreateRandomLabWorkflow(randomDateTimeOffset);
            var exception = new Exception();

            var failedLabWorkflowServiceException =
                new FailedLabWorkflowServiceException(exception);

            var expectedLabWorkflowServiceException =
                new LabWorkflowServiceException(failedLabWorkflowServiceException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTime())
                    .Returns(randomDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.InsertLabWorkflowAsync(It.IsAny<LabWorkflow>()))
                    .ThrowsAsync(exception);

            // when
            ValueTask<LabWorkflow> addLabWorkflowTask =
                this.labWorkflowService.AddLabWorkflowAsync(someLabWorkflow);

            LabWorkflowServiceException actualLabWorkflowServiceException =
                await Assert.ThrowsAsync<LabWorkflowServiceException>(
                    addLabWorkflowTask.AsTask);

            // then
            actualLabWorkflowServiceException.Should().BeEquivalentTo(
                expectedLabWorkflowServiceException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTime(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertLabWorkflowAsync(
                    It.IsAny<LabWorkflow>()),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedLabWorkflowServiceException))),
                        Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
