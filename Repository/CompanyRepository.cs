using System.Data;
using Dapper;
using DapperCrud.Context;
using DapperCrud.Contracts;
using DapperCrud.DTO;
using DapperCrud.Entities;
using NuGet.Protocol.Plugins;

namespace DapperCrud.Repository;
public class CompanyRepository : ICompanyRepository
{
    private readonly DapperContext _context;
    public CompanyRepository(DapperContext context)
    {
        _context = context;
    }
    public async Task<Company> createCompany(companyForDTOCreation company)
    {
        var query = "INSERT INTO Companies (Name,Address,Country) VALUES (@Name,@Address,@Country)  " +
                    "SELECT CAST(SCOPE_IDENTITY() AS INT)";
        var parameters = new DynamicParameters();
        parameters.Add("Name",company.Name,DbType.String);
        parameters.Add("Address",company.Address,DbType.String);
        parameters.Add("Country",company.Country,DbType.String);
        using (var connection = _context.CreateConnection())
        {
            var id = await connection.QuerySingleAsync<int>(query, parameters);
            var createdCompany = new Company()
            {
                Id = id,
                Name = company.Name,
                Address = company.Address,
                country = company.Country
            };
            return createdCompany;
        }
    }
    public async  Task updateCompanyRecords(int id,DTOUpdaterCompany company)
    {
        var query = "UPDATE Companies SET Name = @Name,Address = @Address,Country = @Country WHERE Id=@Id";
        var param = new DynamicParameters();
        param.Add("Id",id,DbType.Int32);
        param.Add("Name",company.Name,DbType.String);
        param.Add("Address",company.Address,DbType.String);
        param.Add("Country",company.Country,DbType.String);
        using (var connection = _context.CreateConnection())
        {
            await connection.ExecuteAsync(query, param);
        }
    }
    public async  Task deleteCompanyRecords(int id)
    {
        var query = "DELETE FROM Companies WHERE Id = @Id";
        using (var connection = _context.CreateConnection())
        {
            await connection.ExecuteAsync(query,new {id});
        }
    }

    public async Task<IEnumerable<Company>> getCompanies()
    {
        var query = "SELECT Id,Name as companyName,Address,country FROM Companies";
        using (var connection = _context.CreateConnection())
        {
            var companies = await connection.QueryAsync<Company>(query);
            return companies.ToList();
        }
    }
    public async Task<Company> getCompanyById(int Id)
    {
        var query = "SELECT * FROM Companies WHERE Id = @Id";
        using (var connection =  _context.CreateConnection())
        {
            var company = await connection.QuerySingleOrDefaultAsync<Company>(query, new
            {
                Id
            });
            return company;
        }
    }

    public async Task<Company> getCompanyByEmployeeId(int id)
    {
        var procedureName = "USP_showCompanyByEmployeeId";
        var param = new DynamicParameters();
        param.Add("Id",id,DbType.Int32,ParameterDirection.Input);
        using (var connection = _context.CreateConnection())
        {
            var dbValue = await connection.QueryFirstOrDefaultAsync<Company>(
                procedureName, param, commandType: CommandType.StoredProcedure
            );
            return dbValue;
        }
    }

    public async Task<Company?> getMultipleResultSet(int id)
    {
        var query = "SELECT * FROM Companies WHERE Id=@Id" +
                    " SELECT * FROM Employees WHERE CompanyId = @Id ";
        using(var connection = _context.CreateConnection())
        using (var multi = await connection.QueryMultipleAsync(query, new { id }))
        {
            var resultSet = await multi.ReadSingleOrDefaultAsync<Company>();
            if (resultSet != null)
                resultSet.Employees = (await multi.ReadAsync<Employee>()).ToList();
            return resultSet;
        }
    }
          // Need to Learn properly about Multiple mapping in Dapper
    public async  Task<List<Company>> multipleMapping()
    {
        var query = " SELECT * FROM Companies CMP JOIN Employees EMP ON CMP.Id = EMP.CompanyId ";
        using (var connection = _context.CreateConnection())
        {
            var compDict = new Dictionary<int, Company>();
            var companies = await connection.QueryAsync<Company, Employee, Company>(
                query, (company, employee) =>
                {
                    if (!compDict.TryGetValue(company.Id, out var currentCompany))
                    {
                        currentCompany = company;
                        compDict.Add(currentCompany.Id, currentCompany);
                    }
                    currentCompany.Employees.Add(employee);
                    return currentCompany;
                }
            );
            return companies.Distinct().ToList();
        }
    }
    public async Task CreateMultipleCompanies(List<companyForDTOCreation> companies)
    {
        var query = "INSERT INTO Companies (Name, Address, Country) VALUES (@Name, @Address, @Country)";
        using (var connection = _context.CreateConnection())
        {
            connection.Open();
            using (var transaction = connection.BeginTransaction())
            {
                foreach (var company in companies)
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("Name", company.Name, DbType.String);
                    parameters.Add("Address", company.Address, DbType.String);
                    parameters.Add("Country", company.Country, DbType.String);
                    await connection.ExecuteAsync(query, parameters, transaction: transaction);
                }
                transaction.Commit();
            }
        }
    }
}