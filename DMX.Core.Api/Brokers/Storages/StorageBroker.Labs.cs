﻿// ---------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------

using System.Linq;
using System.Threading.Tasks;
using DMX.Core.Api.Models.Foundations.Labs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DMX.Core.Api.Brokers.Storages
{
    public partial class StorageBroker
    {
        public DbSet<Lab> Labs { get; set; }

        public async ValueTask<Lab> InsertLabAsync(Lab lab)
        {
            using var broker = new StorageBroker(this.configuration);

            EntityEntry<Lab> labEntityEntry =
                await broker.Labs.AddAsync(lab);

            await broker.SaveChangesAsync();

            return labEntityEntry.Entity;
        }

        public IQueryable<Lab> SelectAllLabsWithDevices()
        {
            using var broker = new StorageBroker(this.configuration);

            return broker.Labs.Include(lab => lab.Devices);
        }
    }
}
