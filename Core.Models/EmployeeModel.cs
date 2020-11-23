using System;

namespace Core.Models
{
    public class EmployeeModel
    {
        public string CompanyName { get; set; }
        public int EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SocialSecurity { get; set; }
        public DateTime? HireDate { get; set; }
        public int? ManagerEmployeeId { get; set; }
    }
}
