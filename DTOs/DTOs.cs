using System.ComponentModel.DataAnnotations;
using ATS.API.Models;

namespace ATS.API.DTOs
{
    // Job Posting DTOs
    public class CreateJobPostingDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }
        
        [Required]
        public string Description { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Department { get; set; }
        
        [MaxLength(100)]
        public string Location { get; set; }
        
        [Required]
        public string Requirements { get; set; }
        
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        
        [Required]
        public int CreatedByUserId { get; set; }
    }

    public class UpdateJobPostingDto
    {
        [MaxLength(200)]
        public string Title { get; set; }
        
        public string Description { get; set; }
        
        [MaxLength(100)]
        public string Department { get; set; }
        
        [MaxLength(100)]
        public string Location { get; set; }
        
        public string Requirements { get; set; }
        
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
    }

    // Application DTOs
    public class CreateApplicationDto
    {
        [Required]
        public int JobPostingId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Phone]
        public string PhoneNumber { get; set; }
        
        [Required]
        public string ResumeUrl { get; set; }
        
        [Required]
        public EducationLevel EducationLevel { get; set; }
        
        [Required]
        [Range(0, 50)]
        public int YearsOfExperience { get; set; }
        
        public string Skills { get; set; }
        
        public string CoverLetter { get; set; }
    }

    public class ApplicationDetailsDto
    {
        public int ApplicationId { get; set; }
        public int ApplicantId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public EducationLevel EducationLevel { get; set; }
        public int YearsOfExperience { get; set; }
        public ApplicationStatus Status { get; set; }
        public bool IsShortlisted { get; set; }
        public int? ShortlistRank { get; set; }
        public DateTime AppliedAt { get; set; }
    }

    public class UpdateStatusDto
    {
        [Required]
        public ApplicationStatus Status { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        public string Comments { get; set; }
    }

    // Shortlisting DTOs
    public class ShortlistCriteriaDto
    {
        public EducationLevel? MinEducationLevel { get; set; }
        public int? MinYearsOfExperience { get; set; }
        public int? TopCount { get; set; } = 10;
    }

    public class ShortlistResultDto
    {
        public int TotalApplicants { get; set; }
        public int ShortlistedCount { get; set; }
        public List<ApplicationDetailsDto> Shortlisted { get; set; }
    }

    // Analytics DTOs
    public class AnalyticsDto
    {
        public JobAnalyticsDto JobAnalytics { get; set; }
        public ApplicantAnalyticsDto ApplicantAnalytics { get; set; }
        public ShortlistAnalyticsDto ShortlistAnalytics { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class JobAnalyticsDto
    {
        public int TotalJobs { get; set; }
        public int OpenJobs { get; set; }
        public int ClosedJobs { get; set; }
        public int DraftJobs { get; set; }
    }

    public class ApplicantAnalyticsDto
    {
        public int TotalApplicants { get; set; }
        public int TotalApplications { get; set; }
        public Dictionary<string, int> StatusBreakdown { get; set; }
        public double AverageApplicationsPerJob { get; set; }
    }

    public class ShortlistAnalyticsDto
    {
        public int TotalShortlisted { get; set; }
        public double ShortlistRate { get; set; }
        public double AverageShortlistPerJob { get; set; }
    }

    // User DTOs
    public class CreateUserDto
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        public UserRole Role { get; set; }
    }

    public class UpdateUserDto
    {
        [MaxLength(100)]
        public string FirstName { get; set; }
        
        [MaxLength(100)]
        public string LastName { get; set; }
        
        [EmailAddress]
        public string Email { get; set; }
        
        public UserRole? Role { get; set; }
        
        public bool? IsActive { get; set; }
    }

    // Notification DTOs
    public class UpdateNotificationSettingsDto
    {
        public bool? EmailOnNewApplication { get; set; }
        public bool? EmailOnStatusChange { get; set; }
        public bool? EmailOnShortlist { get; set; }
    }

    // External Integration DTOs
    public class ExternalCandidateDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string ResumeUrl { get; set; }
        public EducationLevel EducationLevel { get; set; }
        public int YearsOfExperience { get; set; }
        public string Skills { get; set; }
    }

    public class ImportCandidatesDto
    {
        [Required]
        public string IntegrationId { get; set; }
    }
}
