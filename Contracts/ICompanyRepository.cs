using DapperCrud.DTO;
using DapperCrud.Entities;

namespace DapperCrud.Contracts;

public interface ICompanyRepository
{
    public Task<IEnumerable<Company>> getCompanies();
    public Task<Company> getCompanyById(int Id);
    public Task<Company> createCompany(companyForDTOCreation company);
    public Task updateCompanyRecords(int id,DTOUpdaterCompany company);
    public Task deleteCompanyRecords(int id);
    public Task<Company> getCompanyByEmployeeId(int id);
    public Task<Company?> getMultipleResultSet(int id);
    public Task<List<Company>> multipleMapping();

}