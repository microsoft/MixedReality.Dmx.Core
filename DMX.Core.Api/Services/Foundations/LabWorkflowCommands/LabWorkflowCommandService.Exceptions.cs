﻿// ---------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Threading.Tasks;
using DMX.Core.Api.Models.Foundations.LabWorkflowCommands;
using DMX.Core.Api.Models.Foundations.LabWorkflowCommands.Exceptions;
using DMX.Core.Api.Models.Foundations.LabWorkflows.Exceptions;
using Microsoft.Data.SqlClient;
using Xeptions;

namespace DMX.Core.Api.Services.Foundations.LabWorkflowCommands
{
    public partial class LabWorkflowCommandService
    {
        public delegate ValueTask<LabWorkflowCommand> ReturningLabWorkflowCommandFunction();

        public async ValueTask<LabWorkflowCommand> TryCatch(ReturningLabWorkflowCommandFunction returningLabWorkflowCommandFunction)
        {
            try
            {
                return await returningLabWorkflowCommandFunction();
            }
            catch (NullLabWorkflowCommandException nullLabWorkflowCommandException)
            {
                throw CreateAndLogValidationException(nullLabWorkflowCommandException);
            }
            catch (InvalidLabWorkflowCommandException invalidLabWorkflowCommandException)
            {
                throw CreateAndLogValidationException(invalidLabWorkflowCommandException);
            }
            catch (NotFoundLabWorkflowCommandException notFoundLabWorkflowCommandException)
            {
                throw CreateAndLogValidationException(notFoundLabWorkflowCommandException);
            }
            catch (SqlException sqlException)
            {
                var failedLabWorkflowCommandStorageException = new FailedLabWorkflowCommandStorageException(sqlException);

                throw CreateAndLogCriticalDepedencyException(failedLabWorkflowCommandStorageException);
            }
        }

        private LabWorkflowCommandDependencyException CreateAndLogCriticalDepedencyException(
            Xeption innerException)
        {
            var labWorkflowCommandDependencyException =
                new LabWorkflowCommandDependencyException(innerException);

            this.loggingBroker.LogCritical(labWorkflowCommandDependencyException);

            return labWorkflowCommandDependencyException;
        }

        private LabWorkflowCommandValidationException CreateAndLogValidationException(Xeption exception)
        {
            var labWorkflowCommandValidationException =
                new LabWorkflowCommandValidationException(exception);

            this.loggingBroker.LogError(labWorkflowCommandValidationException);

            return labWorkflowCommandValidationException;
        }
    }
}
