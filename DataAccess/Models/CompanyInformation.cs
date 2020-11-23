using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Data.HumanResources.Models
{
    public partial class CompanyInformation
    {
        [Key]
        public long Id { get; set; }

        public string CompanyName { get; set; }
    }
}
