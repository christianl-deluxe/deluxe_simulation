using System;
using System.Threading;
using System.Threading.Tasks;
using Data.HumanResources.Models;

namespace Data.HumanResources.Repositories
{
    public interface ICompanyInfoRepository
    {
        Task<CompanyInformation> GetCompanyById(long companyId, CancellationToken token);
        Task<CompanyInformation> GetCompanyByName(string companyName, CancellationToken token);

        Task<CompanyInformation> CreateCompany(CompanyInformation company, CancellationToken token);

        Task UpdateCompany(CompanyInformation company, CancellationToken token);

        Task RemoveCompany(long id, CancellationToken token);
    }

    public class CompanyInfoRepository : ICompanyInfoRepository
    {
        public async Task<CompanyInformation> GetCompanyById(long companyId, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public async Task<CompanyInformation> GetCompanyByName(string companyName, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public async Task<CompanyInformation> CreateCompany(CompanyInformation company, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateCompany(CompanyInformation company, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public async Task RemoveCompany(long id, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
