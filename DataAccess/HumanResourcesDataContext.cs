using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Data.HumanResources.Models;

namespace Data.HumanResources.DataAccess
{
    public partial class HumanResourcesDataContext : DbContext
    {
        /// <remarks>
        /// Normally, the DB Context class, the OnModelCreating method which sets up all the FK relationships, index names, and other metadata
        /// needed by EF and EF Core to properly construct expressions and track state would be auto-generated using EF Core's package manager commands.
        /// However, as I don't have a readily-available DB schema and that seemed a bit beyond the simulation scope, I'll simply stub out the necessary DAO models
        /// </remarks>
        public HumanResourcesDataContext()
        {
        }

        public HumanResourcesDataContext(DbContextOptions<HumanResourcesDataContext> options)
            : base(options)
        {
        }

        public virtual DbSet<EmployeeInformation> Employees { get; set; }
        public virtual DbSet<CompanyInformation> Companies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // FKs, indexes, and PKs would be specified here
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
