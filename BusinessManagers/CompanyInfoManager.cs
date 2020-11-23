using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Models;
using Data.HumanResources.Repositories;
using Microsoft.Extensions.Logging;

namespace BusinessManagers
{
    public interface ICompanyInfoManager
    {
        Task<CompanyModel> GetCompanyByName(string companyName, CancellationToken token);

        Task<CompanyModel> CreateCompany(CompanyModel company, CancellationToken token);

        Task UpdateCompany(CompanyModel company, CancellationToken token);
    }

    public class CompanyInfoManager : ICompanyInfoManager
    {
        private readonly ILogger<CompanyInfoManager> Logger;
        private readonly ICompanyInfoRepository Repository;

        public CompanyInfoManager(ILogger<CompanyInfoManager> logger, ICompanyInfoRepository repository)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<CompanyModel> GetCompanyByName(string companyName, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public async Task<CompanyModel> CreateCompany(CompanyModel company, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateCompany(CompanyModel company, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
