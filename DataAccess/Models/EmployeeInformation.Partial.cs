using System;
using System.Collections.Generic;
using System.Text;

namespace Data.HumanResources.Models
{
    public partial class EmployeeInformation
    {
        public void Update(EmployeeInformation newData)
        {
            if (newData == null)
            {
                throw new ArgumentNullException(nameof(newData));
            }

            EmployeeId = newData.EmployeeId;
            FirstName = newData.FirstName;
            LastName = newData.LastName;
            HireDate = newData.HireDate;
            ManagerEmployeeId = newData.ManagerEmployeeId;

            // DB Primary Key
            // Id
            
            // immutable once record is created
            // EmployeeId, CompanyId

            // immutable by Update operation
            // Deleted

            DateLastModified = DateTimeOffset.UtcNow;
        }
    }
}
