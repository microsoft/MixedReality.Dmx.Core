﻿// ---------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Data;
using DMX.Core.Api.Models.Labs;
using DMX.Core.Api.Models.Labs.Exceptions;

namespace DMX.Core.Api.Services.Foundations.Labs
{
    public partial class LabService
    {
        private void ValidateLabOnAdd(Lab lab)
        {
            ValidateLabIsNotNull(lab);

            Validate(
                (Rule: IsInvalid(lab.Id), Parameter: nameof(Lab.Id)),
                (Rule: IsInvalidId(lab.ExternalId), Parameter: nameof(Lab.ExternalId)),
                (Rule: IsInvalid(lab.Name), Parameter: nameof(Lab.Name)),
                (Rule: IsInvalid(lab.Description), Parameter: nameof(Lab.Description)));
        }

        private void ValidateLabIsNotNull(Lab lab)
        {
            if(lab is null)
            {
                throw new NullLabException();
            }
        }

        private static dynamic IsInvalid(string text) => new
        {
            Condition = String.IsNullOrWhiteSpace(text),
            Message = "Text is required"
        };

        private static dynamic IsInvalid(Guid id) => new
        {
            Condition = id == Guid.Empty,
            Message = "Id is required"
        };

        private static dynamic IsInvalidId(string id) => new
        {
            Condition = String.IsNullOrWhiteSpace(id),
            Message = "Id is required"
        };

        private static void Validate(params (dynamic Rule, string Parameter)[] validations)
        {
            var invalidLabException = new InvalidLabException();

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    invalidLabException.UpsertDataList(
                        key: parameter,
                        value: rule.Message);
                }
            }

            invalidLabException.ThrowIfContainsErrors();
        }
    }
}
