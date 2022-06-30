﻿// ---------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------

using System;

namespace DMX.Core.Api.Models.Foundations.Labs
{
    public class LabDevice
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int? PowerLevel { get; set; }
        public LabDeviceType Type { get; set; }
        public LabDeviceStatus Status { get; set; }
        public LabDeviceCategory Category { get; set; }
        public Guid LabId { get; set; }
        public Lab Lab { get; set; }
    }
}
