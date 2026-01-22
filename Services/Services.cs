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
                SalaryMax = dto.SalaryMax,
                Status = JobStatus.Open,
                CreatedByUserId = dto.CreatedByUserId,
                CreatedAt = DateTime.UtcNow
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

        public async Task<IEnumerable<JobPosting>> GetAllJobPostingsAsync(
            JobStatus? status = null, 
            string department = null)
        {
            var query = _context.JobPostings.AsQueryable();

            if (status.HasValue)
                query = query.Where(j => j.Status == status.Value);

            if (!string.IsNullOrEmpty(department))
                query = query.Where(j => j.Department == department);

            return await query.OrderByDescending(j => j.CreatedAt).ToListAsync();
        }
    }

    // Applicant Service Interface
    public interface IApplicantService
    {
        Task<Application> CreateApplicationAsync(CreateApplicationDto dto);
        Task<Application> GetApplicationByIdAsync(int id);
        Task<IEnumerable<ApplicationDetailsDto>> GetApplicantsByJobPostingAsync(int jobPostingId);
        Task<IEnumerable<ApplicationDetailsDto>> FilterByEducationAsync(int jobPostingId, EducationLevel educationLevel);
        Task<Application> UpdateApplicationStatusAsync(int applicationId, ApplicationStatus status, int userId, string comments);
        Task ImportCandidateAsync(ExternalCandidateDto candidate);
    }

    // Applicant Service Implementation
    public class ApplicantService : IApplicantService
    {
        private readonly AtsDbContext _context;

        public ApplicantService(AtsDbContext context)
        {
            _context = context;
        }

        public async Task<Application> CreateApplicationAsync(CreateApplicationDto dto)
        {
            // Check if applicant exists
            var applicant = await _context.Applicants
                .FirstOrDefaultAsync(a => a.Email == dto.Email);

            if (applicant == null)
            {
                applicant = new Applicant
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    ResumeUrl = dto.ResumeUrl,
                    EducationLevel = dto.EducationLevel,
                    YearsOfExperience = dto.YearsOfExperience,
                    Skills = dto.Skills
                };

                _context.Applicants.Add(applicant);
                await _context.SaveChangesAsync();
            }

            var application = new Application
            {
                JobPostingId = dto.JobPostingId,
                ApplicantId = applicant.Id,
                CoverLetter = dto.CoverLetter,
                Status = ApplicationStatus.New,
                AppliedAt = DateTime.UtcNow
            };

            _context.Applications.Add(application);
            await _context.SaveChangesAsync();

            return await GetApplicationByIdAsync(application.Id);
        }

        public async Task<Application> GetApplicationByIdAsync(int id)
        {
            return await _context.Applications
                .Include(a => a.Applicant)
                .Include(a => a.JobPosting)
                .Include(a => a.StatusHistory)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<ApplicationDetailsDto>> GetApplicantsByJobPostingAsync(int jobPostingId)
        {
            return await _context.Applications
                .Where(a => a.JobPostingId == jobPostingId)
                .Include(a => a.Applicant)
                .Select(a => new ApplicationDetailsDto
                {
                    ApplicationId = a.Id,
                    ApplicantId = a.ApplicantId,
                    FirstName = a.Applicant.FirstName,
                    LastName = a.Applicant.LastName,
                    Email = a.Applicant.Email,
                    EducationLevel = a.Applicant.EducationLevel,
                    YearsOfExperience = a.Applicant.YearsOfExperience,
                    Status = a.Status,
                    IsShortlisted = a.IsShortlisted,
                    ShortlistRank = a.ShortlistRank,
                    AppliedAt = a.AppliedAt
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ApplicationDetailsDto>> FilterByEducationAsync(
            int jobPostingId, 
            EducationLevel educationLevel)
        {
            return await _context.Applications
                .Where(a => a.JobPostingId == jobPostingId && a.Applicant.EducationLevel >= educationLevel)
                .Include(a => a.Applicant)
                .Select(a => new ApplicationDetailsDto
                {
                    ApplicationId = a.Id,
                    ApplicantId = a.ApplicantId,
                    FirstName = a.Applicant.FirstName,
                    LastName = a.Applicant.LastName,
                    Email = a.Applicant.Email,
                    EducationLevel = a.Applicant.EducationLevel,
                    YearsOfExperience = a.Applicant.YearsOfExperience,
                    Status = a.Status,
                    IsShortlisted = a.IsShortlisted,
                    AppliedAt = a.AppliedAt
                })
                .ToListAsync();
        }

        public async Task<Application> UpdateApplicationStatusAsync(
            int applicationId, 
            ApplicationStatus status, 
            int userId, 
            string comments)
        {
            var application = await GetApplicationByIdAsync(applicationId);
            
            if (application == null)
                return null;

            var oldStatus = application.Status;
            application.Status = status;
            application.StatusUpdatedAt = DateTime.UtcNow;

            // Track status history
            var history = new ApplicationStatusHistory
            {
                ApplicationId = applicationId,
                FromStatus = oldStatus,
                ToStatus = status,
                ChangedByUserId = userId,
                Comments = comments,
                ChangedAt = DateTime.UtcNow
            };

            _context.ApplicationStatusHistories.Add(history);
            await _context.SaveChangesAsync();

            return application;
        }

        public async Task ImportCandidateAsync(ExternalCandidateDto candidate)
        {
            var existingApplicant = await _context.Applicants
                .FirstOrDefaultAsync(a => a.Email == candidate.Email);

            if (existingApplicant != null)
                return;

            var applicant = new Applicant
            {
                FirstName = candidate.FirstName,
                LastName = candidate.LastName,
                Email = candidate.Email,
                PhoneNumber = candidate.PhoneNumber,
                ResumeUrl = candidate.ResumeUrl,
                EducationLevel = candidate.EducationLevel,
                YearsOfExperience = candidate.YearsOfExperience,
                Skills = candidate.Skills
            };

            _context.Applicants.Add(applicant);
            await _context.SaveChangesAsync();
        }
    }
}
