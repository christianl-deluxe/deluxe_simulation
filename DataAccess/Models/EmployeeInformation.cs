using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.HumanResources.Models
{
    public partial class EmployeeInformation
    {
        [Key]
        public long Id { get; set; }

        public int EmployeeId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string SocialSecurity { get; set; }

        public DateTime? HireDate { get; set; }

        public long CompanyId { get; set; }
        
        public int? ManagerEmployeeId { get; set; }

        public bool Deleted { get; set; }
        
        public DateTimeOffset DateLastModified { get; set; }

        [InverseProperty("EmployeeInformation")]
        public virtual CompanyInformation Company { get; set; }
    }
}
