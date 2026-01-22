using Microsoft.EntityFrameworkCore;
using ATS.API.Data;
using ATS.API.Models;
using ATS.API.DTOs;

namespace ATS.API.Services
{
    // Shortlisting Service Interface
    public interface IShortlistingService
    {
        Task<IEnumerable<ApplicationDetailsDto>> RankByExperienceAsync(int jobPostingId);
        Task<IEnumerable<ApplicationDetailsDto>> GetShortlistedApplicantsAsync(int jobPostingId);
        Task<ShortlistResultDto> AutoShortlistAsync(int jobPostingId, ShortlistCriteriaDto criteria);
    }

    // Shortlisting Service Implementation
    public class ShortlistingService : IShortlistingService
    {
        private readonly AtsDbContext _context;

        public ShortlistingService(AtsDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ApplicationDetailsDto>> RankByExperienceAsync(int jobPostingId)
        {
            var applications = await _context.Applications
                .Where(a => a.JobPostingId == jobPostingId)
                .Include(a => a.Applicant)
                .OrderByDescending(a => a.Applicant.YearsOfExperience)
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

            // Assign ranks
            for (int i = 0; i < applications.Count; i++)
            {
                applications[i].ShortlistRank = i + 1;
            }

            return applications;
        }

        public async Task<IEnumerable<ApplicationDetailsDto>> GetShortlistedApplicantsAsync(int jobPostingId)
        {
            return await _context.Applications
                .Where(a => a.JobPostingId == jobPostingId && a.IsShortlisted)
                .Include(a => a.Applicant)
                .OrderBy(a => a.ShortlistRank)
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

        public async Task<ShortlistResultDto> AutoShortlistAsync(
            int jobPostingId, 
            ShortlistCriteriaDto criteria)
        {
            var query = _context.Applications
                .Where(a => a.JobPostingId == jobPostingId)
                .Include(a => a.Applicant)
                .AsQueryable();

            // Apply filters
            if (criteria.MinEducationLevel.HasValue)
            {
                query = query.Where(a => a.Applicant.EducationLevel >= criteria.MinEducationLevel.Value);
            }

            if (criteria.MinYearsOfExperience.HasValue)
            {
                query = query.Where(a => a.Applicant.YearsOfExperience >= criteria.MinYearsOfExperience.Value);
            }

            // Rank by experience
            var candidates = await query
                .OrderByDescending(a => a.Applicant.YearsOfExperience)
                .ThenByDescending(a => a.Applicant.EducationLevel)
                .ToListAsync();

            // Select top N candidates
            var topCount = criteria.TopCount ?? 10;
            var shortlisted = candidates.Take(topCount).ToList();

            // Update shortlist status
            foreach (var (application, index) in shortlisted.Select((app, idx) => (app, idx)))
            {
                application.IsShortlisted = true;
                application.ShortlistRank = index + 1;
                application.Status = ApplicationStatus.Shortlisted;
                application.StatusUpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return new ShortlistResultDto
            {
                TotalApplicants = candidates.Count,
                ShortlistedCount = shortlisted.Count,
                Shortlisted = shortlisted.Select(a => new ApplicationDetailsDto
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
                    ShortlistRank = a.ShortlistRank
                }).ToList()
            };
        }
    }

    // Analytics Service Interface
    public interface IAnalyticsService
    {
        Task<AnalyticsDto> GetRecruitmentAnalyticsAsync(DateTime? startDate, DateTime? endDate);
        Task<JobAnalyticsDto> GetJobAnalyticsAsync();
        Task<ApplicantAnalyticsDto> GetApplicantAnalyticsAsync();
        Task<ShortlistAnalyticsDto> GetShortlistAnalyticsAsync();
        Task<IEnumerable<object>> ExportApplicationsAsync(DateTime? startDate, DateTime? endDate);
        Task<IEnumerable<object>> ExportShortlistAsync(int? jobPostingId);
        byte[] ConvertToCsv(IEnumerable<object> data);
    }

    // Analytics Service Implementation
    public class AnalyticsService : IAnalyticsService
    {
        private readonly AtsDbContext _context;

        public AnalyticsService(AtsDbContext context)
        {
            _context = context;
        }

        public async Task<AnalyticsDto> GetRecruitmentAnalyticsAsync(
            DateTime? startDate, 
            DateTime? endDate)
        {
            startDate ??= DateTime.UtcNow.AddMonths(-1);
            endDate ??= DateTime.UtcNow;

            var jobAnalytics = await GetJobAnalyticsAsync();
            var applicantAnalytics = await GetApplicantAnalyticsAsync();
            var shortlistAnalytics = await GetShortlistAnalyticsAsync();

            return new AnalyticsDto
            {
                JobAnalytics = jobAnalytics,
                ApplicantAnalytics = applicantAnalytics,
                ShortlistAnalytics = shortlistAnalytics,
                StartDate = startDate.Value,
                EndDate = endDate.Value
            };
        }

        public async Task<JobAnalyticsDto> GetJobAnalyticsAsync()
        {
            var totalJobs = await _context.JobPostings.CountAsync();
            var openJobs = await _context.JobPostings.CountAsync(j => j.Status == JobStatus.Open);
            var closedJobs = await _context.JobPostings.CountAsync(j => j.Status == JobStatus.Closed);

            return new JobAnalyticsDto
            {
                TotalJobs = totalJobs,
                OpenJobs = openJobs,
                ClosedJobs = closedJobs,
                DraftJobs = await _context.JobPostings.CountAsync(j => j.Status == JobStatus.Draft)
            };
        }

        public async Task<ApplicantAnalyticsDto> GetApplicantAnalyticsAsync()
        {
            var totalApplicants = await _context.Applicants.CountAsync();
            var totalApplications = await _context.Applications.CountAsync();
            
            var statusBreakdown = await _context.Applications
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status.ToString(), x => x.Count);

            return new ApplicantAnalyticsDto
            {
                TotalApplicants = totalApplicants,
                TotalApplications = totalApplications,
                StatusBreakdown = statusBreakdown,
                AverageApplicationsPerJob = totalApplications > 0 
                    ? (double)totalApplications / await _context.JobPostings.CountAsync(j => j.Status == JobStatus.Open)
                    : 0
            };
        }

        public async Task<ShortlistAnalyticsDto> GetShortlistAnalyticsAsync()
        {
            var totalShortlisted = await _context.Applications.CountAsync(a => a.IsShortlisted);
            var totalApplications = await _context.Applications.CountAsync();

            return new ShortlistAnalyticsDto
            {
                TotalShortlisted = totalShortlisted,
                ShortlistRate = totalApplications > 0 
                    ? (double)totalShortlisted / totalApplications * 100 
                    : 0,
                AverageShortlistPerJob = await _context.Applications
                    .Where(a => a.IsShortlisted)
                    .GroupBy(a => a.JobPostingId)
                    .AverageAsync(g => (double?)g.Count()) ?? 0
            };
        }

        public async Task<IEnumerable<object>> ExportApplicationsAsync(
            DateTime? startDate, 
            DateTime? endDate)
        {
            var query = _context.Applications
                .Include(a => a.Applicant)
                .Include(a => a.JobPosting)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(a => a.AppliedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.AppliedAt <= endDate.Value);

            return await query
                .Select(a => new
                {
                    ApplicationId = a.Id,
                    JobTitle = a.JobPosting.Title,
                    ApplicantName = $"{a.Applicant.FirstName} {a.Applicant.LastName}",
                    Email = a.Applicant.Email,
                    Education = a.Applicant.EducationLevel.ToString(),
                    Experience = a.Applicant.YearsOfExperience,
                    Status = a.Status.ToString(),
                    IsShortlisted = a.IsShortlisted,
                    AppliedDate = a.AppliedAt
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> ExportShortlistAsync(int? jobPostingId)
        {
            var query = _context.Applications
                .Where(a => a.IsShortlisted)
                .Include(a => a.Applicant)
                .Include(a => a.JobPosting)
                .AsQueryable();

            if (jobPostingId.HasValue)
                query = query.Where(a => a.JobPostingId == jobPostingId.Value);

            return await query
                .OrderBy(a => a.ShortlistRank)
                .Select(a => new
                {
                    Rank = a.ShortlistRank,
                    JobTitle = a.JobPosting.Title,
                    ApplicantName = $"{a.Applicant.FirstName} {a.Applicant.LastName}",
                    Email = a.Applicant.Email,
                    Education = a.Applicant.EducationLevel.ToString(),
                    Experience = a.Applicant.YearsOfExperience,
                    Status = a.Status.ToString()
                })
                .ToListAsync();
        }

        public byte[] ConvertToCsv(IEnumerable<object> data)
        {
            var csv = new System.Text.StringBuilder();
            
            if (!data.Any())
                return System.Text.Encoding.UTF8.GetBytes(csv.ToString());

            // Get headers
            var properties = data.First().GetType().GetProperties();
            csv.AppendLine(string.Join(",", properties.Select(p => p.Name)));

            // Add rows
            foreach (var item in data)
            {
                var values = properties.Select(p => 
                {
                    var value = p.GetValue(item)?.ToString() ?? "";
                    return value.Contains(",") ? $"\"{value}\"" : value;
                });
                csv.AppendLine(string.Join(",", values));
            }

            return System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        }
    }
}
