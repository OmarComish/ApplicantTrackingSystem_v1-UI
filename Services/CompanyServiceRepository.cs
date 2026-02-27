using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ATS.API.Data;
using ATS.API.DTOs;
using ATS.API.Interfaces;
using ATS.API.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ATS.API.Services
{
    public class CompanyServiceRepository: ICompanyService
    {
        private readonly AtsDbContext _context;
        private readonly IMapper  _mapper;
        public CompanyServiceRepository(AtsDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<List<CompanyDto>> GetAllCompanies()
        {
            var result = new List<CompanyDto>();
            return result;
        }
        public async Task<CompanyDto> CreateAsync(CreateCompanyDto dto)
        {
            var company = _mapper.Map<Company>(dto);
            company.CreatedAt = DateTime.UtcNow;
            company.UpdatedAt = DateTime.UtcNow;

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            // Reload with navigation property to return full DTO
            var created = await _context.Companies
                .Include(c => c.Industry)
                .FirstOrDefaultAsync(c => c.Id == company.Id);

            return _mapper.Map<CompanyDto>(created);
        }
        public async Task<IEnumerable<CompanyDto>> GetAllAsync()
        {
            var companies = await _context.Companies
                .Include(c => c.Industry)               // eager-load related industry                    
                .OrderBy(c => c.Name)
                .ToListAsync();

            return _mapper.Map<IEnumerable<CompanyDto>>(companies);
        }
        public async Task<CompanyDto> GetByIdAsync(int id)
        {
            var company = await _context.Companies
                .Include(c => c.Industry)
                .FirstOrDefaultAsync(c => c.Id == id);

            return company is null ? null : _mapper.Map<CompanyDto>(company);
        }

        public async Task<IEnumerable<CompanyDto>> GetByIndustryAsync(int industryId)
        {
            var companies = await _context.Companies
                .Include(c => c.Industry)
                .Where(c => c.IndustryId == industryId)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return _mapper.Map<IEnumerable<CompanyDto>>(companies);
        }
        public async Task<CompanyDto> UpdateAsync(int id, CreateCompanyDto dto)
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Id == id);

            if (company is null) return null;

            // Map only the updatable fields from the DTO onto the tracked entity
            _mapper.Map(dto, company);
            company.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Re-query with navigation property instead of explicit load
            var updated = await _context.Companies
                .Include(c => c.Industry)
                .FirstOrDefaultAsync(c => c.Id == id);

            return _mapper.Map<CompanyDto>(company);
        }
    }
}