using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusinessManagers;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HumanResourcesDataService.Controllers
{
    [Authorize]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly ILogger<EmployeeController> Logger;
        private readonly IEmployeeInfoManager Manager;

        public EmployeeController(ILogger<EmployeeController> logger, IEmployeeInfoManager storeManager)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Manager = storeManager ?? throw new ArgumentNullException(nameof(storeManager));
        }


        [HttpGet]
        [Route("/api/v{version}/company/{companyName}/employee")]
        public async Task<IActionResult> GetEmployees([FromRoute][Required] int version, [FromRoute][Required] string companyName, CancellationToken token)
        {
            // TODO: performance improvement via pagination using query string
            // TODO: query string search by employee name, manager ID, or hire date; but not employee ID, which is 'unique key' (within a company's 'namespace')
            
            // ideally, we'd have some type of Role-Based Access Control here that would look at user and both authenticate and authorize
            // user since we wouldn't want HR employees from company A to be able to view/create/update employees of company B

            // version is also a premature feature to account for future API versions that may have different models or behavior

            // as indicated in the Manager, company name should actually be some unique ID (but *never* the company's DB record PK from the DB)

            Logger.LogInformation("Request: Action = {Action}, Company Name = {CompanyName}, User = {User}", 
                nameof(GetEmployees), companyName, User);

            if (string.IsNullOrWhiteSpace(companyName))
            {
                return BadRequest("Company name is required");
            }

            try
            {
                var employees = await Manager.GetEmployeesByCompanyName(companyName, token).ConfigureAwait(false);
                return Ok(employees);
            }
            catch (Exception e)
            {
                // ideally, we'd have some custom exceptions that our business
                // managers would throw that would let us return specific
                // exceptions, especially around create/update requests where
                // required fields may not be present which should be a different
                // error from 404 (400)
                Logger.LogError("Ruht roh!. Exception: {Message}", e.Message);
                return NotFound(e.Message);
            }
        }

        [HttpGet]
        [Route("/api/v{version}/company/{companyName}/employee/{id}")]
        public async Task<IActionResult> GetEmployee([FromRoute][Required] int version, [FromRoute][Required] string companyName, int employeeId, CancellationToken token)
        {
            Logger.LogInformation("Request: Action = {Action}, Company Name = {CompanyName}, Employee ID = {EmployeeID}, User = {User}",
                nameof(GetEmployee), companyName, employeeId, User);

            if (string.IsNullOrWhiteSpace(companyName))
            {
                return BadRequest("Company name is required");
            }

            try
            {
                var employee = await Manager.GetEmployeeByEmployeeId(companyName, employeeId, token).ConfigureAwait(false);
                return Ok(employee);
            }
            catch (Exception e)
            {
                Logger.LogError("Ruht roh!. Exception: {Message}", e.Message);
                return NotFound(e.Message);
            }
        }

        [HttpPost]
        [Route("/api/v{version}/company/{companyName}/employee")]
        public async Task<IActionResult> CreateEmployee([FromRoute][Required] int version, [FromRoute][Required] string companyName, [FromBody] EmployeeModel employee, CancellationToken token)
        {
            Logger.LogInformation("Request: Action = {Action}, Company Name = {CompanyName}, Employee ID = {EmployeeID}, User = {User}",
                nameof(CreateEmployee), companyName, employee?.EmployeeId, User);

            if (string.IsNullOrWhiteSpace(companyName))
            {
                return BadRequest("Company name is required");
            }
            if (employee == null)
            {
                return BadRequest("Employee is required");
            }

            try
            {
                var newEmployee = await Manager.CreateEmployee(employee, token).ConfigureAwait(false);
                return Created($"/api/v{version}/company/{companyName}/employee/{newEmployee.EmployeeId}", newEmployee);
            }
            catch (Exception e)
            {
                // here's where it'd be real nice to differentiate
                // between NotFound 'cause bogus company reference vs.
                // bad employee model (invalid hire date or SSN too many or too few digits)
                Logger.LogError("Ruht roh!. Exception: {Message}", e.Message);
                return BadRequest(e.Message);
            }
        }

        [HttpPut]
        [Route("/api/v{version}/company/{companyName}/employee/{id}")]
        public async Task<IActionResult> UpdateEmployee([FromRoute][Required] int version, [FromRoute][Required] string companyName, [FromBody] EmployeeModel employee, CancellationToken token)
        {
            Logger.LogInformation("Request: Action = {Action}, Company Name = {CompanyName}, Employee ID = {EmployeeID}, User = {User}",
                nameof(UpdateEmployee), companyName, employee?.EmployeeId, User);

            if (string.IsNullOrWhiteSpace(companyName))
            {
                return BadRequest("Company name is required");
            }
            if (employee == null)
            {
                return BadRequest("Employee is required");
            }

            try
            {
                await Manager.UpdateEmployee(employee, token).ConfigureAwait(false);
                return Ok();
            }
            catch (Exception e)
            {
                // here's where it'd be real nice to differentiate
                // between NotFound 'cause bogus company reference vs.
                // bad employee model (invalid hire date or SSN too many or too few digits)
                Logger.LogError("Ruht roh!. Exception: {Message}", e.Message);
                return BadRequest(e.Message);
            }
        }

        [HttpDelete]
        [Route("/api/v{version}/company/{companyName}/employee/{id}")]
        public async Task<IActionResult> RemoveEmployee([FromRoute][Required] int version, [FromRoute][Required] string companyName, [FromRoute] int employeeId, CancellationToken token)
        {
            Logger.LogInformation("Request: Action = {Action}, Company Name = {CompanyName}, Employee ID = {EmployeeID}, User = {User}",
                nameof(RemoveEmployee), companyName, employeeId, User);

            if (string.IsNullOrWhiteSpace(companyName))
            {
                return BadRequest("Company name is required");
            }

            try
            {
                await Manager.RemoveEmployee(companyName, employeeId, token).ConfigureAwait(false);
                return Ok();
            }
            catch (Exception e)
            {
                Logger.LogError("Ruht roh!. Exception: {Message}", e.Message);
                return NotFound(e.Message);
            }
        }
    }
}
