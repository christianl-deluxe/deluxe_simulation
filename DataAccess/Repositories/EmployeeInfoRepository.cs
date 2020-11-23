using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data.HumanResources.DataAccess;
using Data.HumanResources.Models;
using Microsoft.EntityFrameworkCore;

namespace Data.HumanResources.Repositories
{
    public interface IEmployeeInfoRepository
    {
        Task<IList<EmployeeInformation>> GetEmployees(long companyId, CancellationToken token);
        Task<IList<EmployeeInformation>> GetEmployeesByLastName(long companyId, string lastName, CancellationToken token);
        Task<EmployeeInformation> GetEmployeeById(long id, CancellationToken token);
        Task<EmployeeInformation> GetEmployeeByEmployeeId(long companyId, int employeeId, CancellationToken token);
        Task<IList<EmployeeInformation>> GetEmployeeByManagerId(long companyId, int managerEmployeeId, CancellationToken token);

        Task<int> GetLastEmployeeIdForCompany(long companyId, CancellationToken token);

        Task<EmployeeInformation> CreateEmployee(EmployeeInformation employee, CancellationToken token);

        Task UpdateEmployee(EmployeeInformation employee, CancellationToken token);

        Task RemoveEmployee(long id, CancellationToken token);
    }

    public class EmployeeInfoRepository : IEmployeeInfoRepository
    {
        private readonly DbContextOptions<HumanResourcesDataContext> Options;

        public EmployeeInfoRepository(DbContextOptions<HumanResourcesDataContext> options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<IList<EmployeeInformation>> GetEmployees(long companyId, CancellationToken token)
        {
            await using var db = new HumanResourcesDataContext(Options);
            var employees = db.Employees.Where(x => x.CompanyId == companyId && !x.Deleted);

            return await employees.ToListAsync(token).ConfigureAwait(false);
        }

        public async Task<IList<EmployeeInformation>> GetEmployeesByLastName(long companyId, string lastName, CancellationToken token)
        {
            await using var db = new HumanResourcesDataContext(Options);
            var employees = db.Employees.Where(x => x.CompanyId == companyId && string.Equals(x.LastName, lastName) && !x.Deleted);

            return await employees.ToListAsync(token).ConfigureAwait(false);
        }

        public async Task<EmployeeInformation> GetEmployeeById(long id, CancellationToken token)
        {
            await using var db = new HumanResourcesDataContext(Options);
            return await db.Employees.SingleOrDefaultAsync(x => x.Id == id && !x.Deleted, token).ConfigureAwait(false);
        }

        public async Task<EmployeeInformation> GetEmployeeByEmployeeId(long companyId, int employeeId, CancellationToken token)
        {
            await using var db = new HumanResourcesDataContext(Options);
            return await db.Employees.SingleOrDefaultAsync(x => x.CompanyId == companyId && x.EmployeeId == employeeId && !x.Deleted, token).ConfigureAwait(false);
        }

        public async Task<IList<EmployeeInformation>> GetEmployeeByManagerId(long companyId, int managerEmployeeId, CancellationToken token)
        {
            await using var db = new HumanResourcesDataContext(Options);
            var employees = db.Employees.Where(x => x.CompanyId == companyId && x.ManagerEmployeeId == managerEmployeeId && !x.Deleted);

            return await employees.ToListAsync(token).ConfigureAwait(false);
        }

        public async Task<int> GetLastEmployeeIdForCompany(long companyId, CancellationToken token)
        {
            await using var db = new HumanResourcesDataContext(Options);
            // we want deleted employees here, employee IDs should never be reused
            var employee = await db.Employees.Where(x => x.CompanyId == companyId)
                .OrderByDescending(y => y.EmployeeId)
                .FirstOrDefaultAsync(token).ConfigureAwait(false);

            return employee?.EmployeeId ?? 0;
        }

        public async Task<EmployeeInformation> CreateEmployee(EmployeeInformation employee, CancellationToken token)
        {
            if (employee == null)
            {
                throw new ArgumentNullException(nameof(employee));
            }

            employee.DateLastModified = DateTimeOffset.UtcNow;

            await using var db = new HumanResourcesDataContext(Options);
            var newEmployee = db.Employees.Add(employee);
            await db.SaveChangesAsync(token).ConfigureAwait(false);

            return newEmployee.Entity;
        }

        public async Task UpdateEmployee(EmployeeInformation employee, CancellationToken token)
        {
            if (employee == null)
            {
                throw new ArgumentNullException(nameof(employee));
            }

            await using var db = new HumanResourcesDataContext(Options);

            var e = await db.Employees.SingleOrDefaultAsync(x => x.Id == employee.Id && !x.Deleted, token).ConfigureAwait(false);
            if (e == null)
            {
                // throw some kind of 'NotFound' exception here
                throw new Exception();
            }

            e.Update(employee);
            db.Employees.Update(e);
            await db.SaveChangesAsync(token).ConfigureAwait(false);
        }

        public async Task RemoveEmployee(long id, CancellationToken token)
        {
            await using var db = new HumanResourcesDataContext(Options);
            var e = await db.Employees.SingleOrDefaultAsync(x => x.Id == id && !x.Deleted, token).ConfigureAwait(false);
            if (e == null)
            {
                // throw some kind of 'NotFound' exception here
                throw new Exception();
            }

            e.Deleted = true;
            e.DateLastModified = DateTimeOffset.UtcNow;

            db.Employees.Update(e);
            await db.SaveChangesAsync(token).ConfigureAwait(false);
        }
    }
}
