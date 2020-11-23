using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Models;
using Data.HumanResources.Models;
using Data.HumanResources.Repositories;
using Microsoft.Extensions.Logging;

namespace BusinessManagers
{
    public interface IEmployeeInfoManager
    {
        // ideally, none of these would be based on company name, but a company's ID (which would *not* be the company's DB Primary Key)
        Task<IEnumerable<EmployeeModel>> GetEmployeesByCompanyName(string companyName, CancellationToken token);
        Task<IEnumerable<EmployeeModel>> GetEmployeesByLastName(string companyName, string lastName, CancellationToken token);
        Task<EmployeeModel> GetEmployeeByEmployeeId(string companyName, int employeeId, CancellationToken token);

        Task<EmployeeModel> CreateEmployee(EmployeeModel employee, CancellationToken token);

        Task UpdateEmployee(EmployeeModel employee, CancellationToken token);

        Task RemoveEmployee(string companyName, int employeeId, CancellationToken token);
    }

    public class EmployeeInfoManager : IEmployeeInfoManager
    {
        private readonly ILogger<EmployeeInfoManager> Logger;
        private readonly ICompanyInfoRepository CompanyRepository;
        private readonly IEmployeeInfoRepository EmployeeRepository;

        public EmployeeInfoManager(ILogger<EmployeeInfoManager> logger,
            ICompanyInfoRepository companyRepository,
            IEmployeeInfoRepository employeeRepository)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            CompanyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
            EmployeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        }

        public async Task<IEnumerable<EmployeeModel>> GetEmployeesByCompanyName(string companyName, CancellationToken token)
        {
            Logger.LogInformation("Requested employees for company: {Company}", companyName);

            var company = await CompanyRepository.GetCompanyByName(companyName, token).ConfigureAwait(false);
            if (company == null)
            {
                // throw some kind of 'NotFound' exception to controller
                Logger.LogInformation("Company not found: {Company}", companyName);
                throw new Exception();
            }

            var employees = await EmployeeRepository.GetEmployees(company.Id, token).ConfigureAwait(false);

            Logger.LogDebug("Found {Count} employees for company: {Company}", employees.Count, companyName);

            return employees.OrderBy(x => x.EmployeeId).Select(y => new EmployeeModel
            {
                EmployeeId = y.EmployeeId,
                LastName = y.LastName,
                FirstName = y.FirstName,
                SocialSecurity = y.SocialSecurity,
                HireDate = y.HireDate,
                ManagerEmployeeId = y.ManagerEmployeeId,
            });
        }

        public async Task<IEnumerable<EmployeeModel>> GetEmployeesByLastName(string companyName, string lastName, CancellationToken token)
        {
            Logger.LogInformation("Search for employee last name '{LastName}' for company: {Company}", lastName, companyName);

            var company = await CompanyRepository.GetCompanyByName(companyName, token).ConfigureAwait(false);
            if (company == null)
            {
                // throw some kind of 'NotFound' exception to controller
                Logger.LogInformation("Company not found: {Company}", companyName);
                throw new Exception();
            }

            var employees = await EmployeeRepository.GetEmployeesByLastName(company.Id, lastName, token).ConfigureAwait(false);

            Logger.LogDebug("Found {Count} employees with '{LastName}' for company: {Company}", employees.Count, lastName, companyName);

            return employees.OrderBy(x => x.EmployeeId).Select(y => new EmployeeModel
            {
                EmployeeId = y.EmployeeId,
                LastName = y.LastName,
                FirstName = y.FirstName,
                SocialSecurity = y.SocialSecurity,
                HireDate = y.HireDate,
                ManagerEmployeeId = y.ManagerEmployeeId,
            });
        }

        public async Task<EmployeeModel> GetEmployeeByEmployeeId(string companyName, int employeeId, CancellationToken token)
        {
            Logger.LogInformation("Request for employee ID '{EmployeeID}' for company: {Company}", employeeId, companyName);

            var company = await CompanyRepository.GetCompanyByName(companyName, token).ConfigureAwait(false);
            if (company == null)
            {
                // throw some kind of 'NotFound' exception to controller
                Logger.LogInformation("Company not found: {Company}", companyName);
                throw new Exception();
            }

            var employee = await EmployeeRepository.GetEmployeeByEmployeeId(company.Id, employeeId, token).ConfigureAwait(false);
            if (employee == null)
            {
                // throw some kind of 'NotFound' exception to controller
                Logger.LogInformation("Company '{Company}' employee ID not found: {EmployeeID}", companyName, employeeId);
                throw new Exception();
            }

            return new EmployeeModel
            {
                EmployeeId = employee.EmployeeId,
                LastName = employee.LastName,
                FirstName = employee.FirstName,
                SocialSecurity = employee.SocialSecurity,
                HireDate = employee.HireDate,
                ManagerEmployeeId = employee.ManagerEmployeeId,
            };
        }

        public async Task<EmployeeModel> CreateEmployee(EmployeeModel newEmployee, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(newEmployee?.CompanyName))
            {
                throw new ArgumentException("Employee company name is required", nameof(newEmployee));
            }

            Logger.LogInformation("Create new employee for company: {Company}", newEmployee.CompanyName);

            var company = await CompanyRepository.GetCompanyByName(newEmployee.CompanyName, token).ConfigureAwait(false);
            if (company == null)
            {
                // throw some kind of 'NotFound' exception to controller
                Logger.LogInformation("Company not found: {Company}", newEmployee.CompanyName);
                throw new Exception();
            }

            // if we had any logic here around preventing duplicates, like maybe two employees
            // can't have same SSN or same first or last name, we'd validate that here
            //var existing = await EmployeeRepository.GetEmployeesBySomeField(company.Id, field, token).ConfigureAwait(false);

            if (newEmployee.ManagerEmployeeId.HasValue)
            {
                // manager may not have been assigned yet, or maybe this is the CEO
                var manager = await EmployeeRepository
                    .GetEmployeeByManagerId(company.Id, newEmployee.ManagerEmployeeId.Value, token)
                    .ConfigureAwait(false);

                if (manager == null)
                {
                    // throw some kind of 'NotFound' exception to controller
                    Logger.LogInformation("Invalid manager employee ID: {ManagerEmployeeID}", newEmployee.ManagerEmployeeId.Value);
                    throw new Exception();
                }
            }

            int newEmployeeId = await GetNewEmployeeIdForCompany(company.Id, token).ConfigureAwait(false);
            var dbEmployee = new EmployeeInformation
            {
                CompanyId = company.Id,
                EmployeeId = newEmployeeId,
                FirstName = newEmployee.FirstName,
                LastName = newEmployee.LastName,
                SocialSecurity = newEmployee.SocialSecurity,
                HireDate = newEmployee.HireDate,
                ManagerEmployeeId = newEmployee.ManagerEmployeeId
            };

            await EmployeeRepository.CreateEmployee(dbEmployee, token).ConfigureAwait(false);

            Logger.LogInformation("New employee created for '{Company}'. ID: {EmployeeID}", company.CompanyName, newEmployee.EmployeeId);

            newEmployee.EmployeeId = newEmployeeId;
            return newEmployee;
        }

        public async Task UpdateEmployee(EmployeeModel employee, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(employee?.CompanyName))
            {
                throw new ArgumentException("Employee company name is required", nameof(employee));
            }
            if (employee.EmployeeId < 1)
            {
                throw new ArgumentException($"Invalid employee ID: {employee.EmployeeId}", nameof(employee));
            }

            Logger.LogInformation("Update employee ID '{EmployeeID}' for company: {Company}", employee.EmployeeId, employee.CompanyName);

            var company = await CompanyRepository.GetCompanyByName(employee.CompanyName, token).ConfigureAwait(false);
            if (company == null)
            {
                // throw some kind of 'NotFound' exception to controller
                Logger.LogInformation("Company not found: {Company}", employee.CompanyName);
                throw new Exception();
            }

            var dbEmployee = await EmployeeRepository.GetEmployeeByEmployeeId(company.Id, employee.EmployeeId, token)
                .ConfigureAwait(false);
            if (dbEmployee == null)
            {
                // throw some kind of 'NotFound' exception to controller
                Logger.LogInformation("Company not found: {Company}", employee.CompanyName);
                throw new Exception();
            }

            if (employee.ManagerEmployeeId.HasValue)
            {
                // manager may not have been assigned yet, or maybe this is the CEO
                var manager = await EmployeeRepository
                    .GetEmployeeByManagerId(company.Id, employee.ManagerEmployeeId.Value, token)
                    .ConfigureAwait(false);

                if (manager == null)
                {
                    // throw some kind of 'NotFound' exception to controller
                    Logger.LogInformation("Invalid manager employee ID: {ManagerEmployeeID}", employee.ManagerEmployeeId.Value);
                    throw new Exception();
                }
            }

            dbEmployee.FirstName = employee.FirstName;
            dbEmployee.LastName = employee.LastName;
            dbEmployee.SocialSecurity = employee.SocialSecurity;
            dbEmployee.ManagerEmployeeId = employee.ManagerEmployeeId;
            dbEmployee.HireDate = employee.HireDate;
            // Id, EmployeeId, and CompanyId are immutable after creation

            await EmployeeRepository.UpdateEmployee(dbEmployee, token).ConfigureAwait(false);

            Logger.LogInformation("Updated employee ID '{EmployeeID}' for company: {Company}", employee.EmployeeId, company.CompanyName);
        }

        public async Task RemoveEmployee(string companyName, int employeeId, CancellationToken token)
        {
            Logger.LogInformation("Remove employee ID '{EmployeeID}' for company: {Company}", employeeId, companyName);

            var company = await CompanyRepository.GetCompanyByName(companyName, token).ConfigureAwait(false);
            if (company == null)
            {
                // throw some kind of 'NotFound' exception to controller
                Logger.LogInformation("Company not found: {Company}", companyName);
                throw new Exception();
            }

            var employee = await EmployeeRepository.GetEmployeeByEmployeeId(company.Id, employeeId, token).ConfigureAwait(false);
            if (employee == null)
            {
                // throw some kind of 'NotFound' exception to controller
                Logger.LogInformation("Company '{Company}' employee ID not found: {EmployeeID}", companyName, employeeId);
                throw new Exception();
            }

            await EmployeeRepository.RemoveEmployee(employee.Id, token).ConfigureAwait(false);
        }

        private async Task<int> GetNewEmployeeIdForCompany(long companyId, CancellationToken token)
        {
            // this should really be an SP that will figure out a new employee ID for a company
            // but, here's the business logic in code form
            int lastId = await EmployeeRepository.GetLastEmployeeIdForCompany(companyId, token).ConfigureAwait(false);
            return ++lastId;
        }
    }
}
