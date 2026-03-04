using Microsoft.EntityFrameworkCore;
using ATS.API.Data;
using ATS.API.Models;
using ATS.API.DTOs;

namespace ATS.API.Services
{
    // Job Posting Service Interface
    public interface IJobPostingService
    {
        Task<JobPosting> CreateJobPostingAsync(CreateJobPostingDto dto);
        Task<JobPosting> UpdateJobPostingAsync(int id, UpdateJobPostingDto dto);
        Task<bool> DeleteJobPostingAsync(int id);
        Task<JobPosting> CloseJobPostingAsync(int id);
        Task<JobPosting> GetJobPostingByIdAsync(int id);
        Task<IEnumerable<JobPosting>> GetAllJobPostingsAsync(JobStatus? status = null, string department = null);
    }

    // Job Posting Service Implementation
    public class JobPostingService : IJobPostingService
    {
        private readonly AtsDbContext _context;

        public JobPostingService(AtsDbContext context)
        {
            _context = context;
        }

        public async Task<JobPosting> CreateJobPostingAsync(CreateJobPostingDto dto)
        {
            var jobPosting = new JobPosting
            {
                Title = dto.Title,
                Description = dto.Description,
                Department = dto.Department,
                Location = dto.Location,
                Requirements = dto.Requirements,
                SalaryMin = dto.SalaryMin,
                Responsibilities = dto.Responsibilities,
                CompanyId = 2,
                SalaryMax = dto.SalaryMax,
                Status = JobStatus.Open,
                CreatedBy = dto.CreatedByUserId,
                CreatedAt = DateTime.UtcNow,
                Featured = dto.Featured,
                Type = dto.Type==0? JobType.Contract: dto.Type,
            };

            _context.JobPostings.Add(jobPosting);
            await _context.SaveChangesAsync();

            return jobPosting;
        }
        public async Task<JobPosting> UpdateJobPostingAsync(int id, UpdateJobPostingDto dto)
        {
            var jobPosting = await _context.JobPostings.FindAsync(id);
            
            if (jobPosting == null)
                return null;

            if (!string.IsNullOrEmpty(dto.Title))
                jobPosting.Title = dto.Title;
            if (!string.IsNullOrEmpty(dto.Description))
                jobPosting.Description = dto.Description;
            if (!string.IsNullOrEmpty(dto.Department))
                jobPosting.Department = dto.Department;
            if (!string.IsNullOrEmpty(dto.Location))
                jobPosting.Location = dto.Location;
            if (!string.IsNullOrEmpty(dto.Requirements))
                jobPosting.Requirements = dto.Requirements;
            if (dto.SalaryMin.HasValue)
                jobPosting.SalaryMin = dto.SalaryMin;
            if (dto.SalaryMax.HasValue)
                jobPosting.SalaryMax = dto.SalaryMax;

            jobPosting.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return jobPosting;
        }
        public async Task<bool> DeleteJobPostingAsync(int id)
        {
            var jobPosting = await _context.JobPostings.FindAsync(id);
            
            if (jobPosting == null)
                return false;

            _context.JobPostings.Remove(jobPosting);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<JobPosting> CloseJobPostingAsync(int id)
        {
            var jobPosting = await _context.JobPostings.FindAsync(id);
            
            if (jobPosting == null)
                return null;

            jobPosting.Status = JobStatus.Closed;
            jobPosting.ClosedAt = DateTime.UtcNow;
            jobPosting.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return jobPosting;
        }
        public async Task<JobPosting> GetJobPostingByIdAsync(int id)
        {
            return await _context.JobPostings
                .Include(j => j.Applications)
                    .ThenInclude(a => a.Applicant)
                .FirstOrDefaultAsync(j => j.Id == id);
        }

        public async Task<IEnumerable<JobPosting>> GetAllJobPostingsAsync(JobStatus? status = null, string department = null)
        {
            var query = _context.JobPostings.AsQueryable();

            if (status.HasValue)
                query = query.Where(j => j.Status == status.Value);

            if (!string.IsNullOrEmpty(department))
                query = query.Where(j => j.Department == department);

            return await query.OrderByDescending(j => j.CreatedAt).ToListAsync();
        }
    }



}
