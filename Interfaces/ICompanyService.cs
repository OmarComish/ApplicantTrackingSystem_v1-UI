using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ATS.API.DTOs;

namespace ATS.API.Interfaces
{
    public interface ICompanyService
    {
        Task<List<CompanyDto>> GetAllCompanies();
        Task<CompanyDto> CreateAsync(CreateCompanyDto dto);
        Task<IEnumerable<CompanyDto>> GetAllAsync();
        Task<CompanyDto> GetByIdAsync(int id);
        Task<IEnumerable<CompanyDto>> GetByIndustryAsync(int industryId);
        Task<CompanyDto> UpdateAsync(int id, CreateCompanyDto dto);
    }
}