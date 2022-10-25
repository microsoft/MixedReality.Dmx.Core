// ---------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Threading.Tasks;
using DMX.Core.Api.Models.Foundations.LabWorkflows;
using DMX.Core.Api.Models.Foundations.LabWorkflows.Exceptions;
using DMX.Core.Api.Models.Orchestrations.LabWorkflows.Exceptions;
using DMX.Core.Api.Services.Foundations.LabWorkflows;
using DMX.Core.Api.Services.Orchestrations.LabWorkflows;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RESTFulSense.Controllers;

#if RELEASE
using Microsoft.Identity.Web.Resource;
#endif

namespace DMX.Core.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LabWorkflowsController : RESTFulController
    {
        private readonly ILabWorkflowService labWorkflowService;
        private readonly ILabWorkflowOrchestrationService labWorkflowOrchestrationService;

        public LabWorkflowsController(
            ILabWorkflowService labWorkflowService,
            ILabWorkflowOrchestrationService labWorkflowOrchestrationService)
        {
            this.labWorkflowService = labWorkflowService;
            this.labWorkflowOrchestrationService = labWorkflowOrchestrationService;
        }

        [HttpPost]
#if RELEASE
        [RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes:PostLabWorkflow")]
#endif
        public async ValueTask<ActionResult<LabWorkflow>> PostLabWorkflowAsync(LabWorkflow labWorkflow)
        {
            try
            {
                LabWorkflow addedLabWorkflow =
                    await this.labWorkflowService.AddLabWorkflowAsync(labWorkflow);

                return Created(addedLabWorkflow);
            }
            catch (LabWorkflowValidationException labWorkflowValidationException)
            {
                return BadRequest(labWorkflowValidationException.InnerException);
            }
            catch (LabWorkflowDependencyException labWorkflowDependencyException)
            {
                return InternalServerError(labWorkflowDependencyException);
            }
            catch (LabWorkflowDependencyValidationException labWorkflowDependencyValidationException)
                when (labWorkflowDependencyValidationException.InnerException is AlreadyExistsLabWorkflowException)
            {
                return Conflict(labWorkflowDependencyValidationException.InnerException);
            }
            catch (LabWorkflowDependencyValidationException labWorkflowDependencyValidationException)
            {
                return BadRequest(labWorkflowDependencyValidationException.InnerException);
            }
            catch (LabWorkflowServiceException labWorkflowServiceException)
            {
                return InternalServerError(labWorkflowServiceException);
            }
        }

        [HttpGet("{labWorkflowId}")]
#if RELEASE
        [RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes:GetLabWorkflow")]
#endif
        public async ValueTask<ActionResult<LabWorkflow>> GetLabWorkflowByIdAsync(Guid labWorkflowId)
        {
            try
            {
                LabWorkflow labWorkflow =
                    await this.labWorkflowOrchestrationService.RetrieveLabWorkflowByIdAsync(labWorkflowId);

                return Ok(labWorkflow);
            }
            catch (LabWorkflowOrchestrationDependencyValidationException labWorkflowOrchestrationDependencyValidationException)
                when (labWorkflowOrchestrationDependencyValidationException.InnerException is NotFoundLabWorkflowException)
            {
                return NotFound(labWorkflowOrchestrationDependencyValidationException.InnerException);
            }
            catch (LabWorkflowOrchestrationValidationException labWorkflowOrchestrationValidationException)
            {
                return BadRequest(labWorkflowOrchestrationValidationException.InnerException);
            }
            catch (LabWorkflowOrchestrationDependencyValidationException labWorkflowOrchestrationDependencyValidationException)
            {
                return BadRequest(labWorkflowOrchestrationDependencyValidationException.InnerException);
            }
            catch (LabWorkflowOrchestrationDependencyException labWorkflowOrchestrationDependencyException)
            {
                return InternalServerError(labWorkflowOrchestrationDependencyException);
            }
            catch (LabWorkflowOrchestrationServiceException labWorkflowOrchestrationServiceException)
            {
                return InternalServerError(labWorkflowOrchestrationServiceException);
            }
        }
    }
}
