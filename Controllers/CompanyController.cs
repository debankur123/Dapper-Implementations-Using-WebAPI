using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DapperCrud.Contracts;
using DapperCrud.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DapperCrud.Controllers
{
    [Route("api/company")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyRepository _compRepo;

        public CompanyController(ICompanyRepository compRepo)
        {
            _compRepo = compRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetCompanies()
        {
            var companies = await _compRepo.getCompanies();
            return Ok(companies);
        }
        [HttpGet("{id}",Name = "CompanyById")]
        public async Task<IActionResult> getCompanyById(int id)
        {
            var dbCompany = await _compRepo.getCompanyById(id);
            if (dbCompany == null)
                return NotFound();
            return Ok(dbCompany);
        }
        [HttpPost]
        public async Task<IActionResult> CreateCompany([FromBody] companyForDTOCreation company)
        {
            var createdCompany = await _compRepo.createCompany(company);
            return CreatedAtRoute("CompanyById", new { id = createdCompany.Id }, createdCompany);
        }
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateCompanyRecords(int id,[FromBody] DTOUpdaterCompany company)
        {
            var dbCompany = await _compRepo.getCompanyById(id);
            if (dbCompany == null)
                return NotFound();
            await _compRepo.updateCompanyRecords(id, company);
            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompanyRecords(int id)
        {
            var deleteId = await _compRepo.getCompanyById(id);
            if (deleteId == null)
                return NotFound();
            await _compRepo.deleteCompanyRecords(id);
            return NoContent();
        }

        [HttpGet("ByEmployeeId/{Id}")]
        public async Task<IActionResult> GetCompanyByEmployeeId(int id)
        {
            var result = await _compRepo.getCompanyById(id);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpGet("{id}/MultipleResult")]
        public async Task<IActionResult> GetMultipleRecords(int id)
        {
            var result = await _compRepo.getMultipleResultSet(id);
            if (result == null)
                return NotFound();
            return Ok(result);
        }
        [HttpGet("MultipleMapping")]
        public async Task<IActionResult> GetMultipleMapping()
        {
            var companies = await _compRepo.multipleMapping();
            return Ok(companies);
        }
    }
}
