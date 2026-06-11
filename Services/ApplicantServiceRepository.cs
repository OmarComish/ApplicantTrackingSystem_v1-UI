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
    public class ApplicantServiceRepository: IApplicantService
    {
         private readonly AtsDbContext _context;
         private readonly IMapper _mapper;
        public ApplicantServiceRepository(AtsDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<ResponseDto> CreateApplicationAsync(CreateApplicationDto1 dto)
        {
           var response = new ResponseDto{Status = "error", Message ="An error occurred submitting your application"};
            
            // Check if applicant exists
           var applicant = await _context.Applicants.FirstOrDefaultAsync(a => a.UserId == dto.Id);

           if(applicant == null) 
           {
             response.Message = $"No record found for your profile. Please add your profile first";
             return response;
           }
            var application = new Application
            {
                JobPostingId = dto.JobPostingId,
                ApplicantId = applicant.Id,
                CoverLetter = dto.CoverLetter,
                Status = ApplicationStatus.New,
                AppliedAt = DateTime.UtcNow,
                //Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.Applications.Add(application);
            await _context.SaveChangesAsync();

            response.Status ="success"; 
            response.Message = "Job application submitted successfully";
            //response.Payload = await GetApplicationByIdAsync(application.Id);

            return  response; //await GetApplicationByIdAsync(application.Id);
        }
        private async Task<Application> GetApplicationByIdAsync(int id)
        {
            return await _context.Applications
                .Include(a => a.Applicant)
                .Include(a => a.JobPosting)
                .Include(a => a.StatusHistory)
                .FirstOrDefaultAsync(a => a.Id == id);
        }
        public async Task<IEnumerable<ApplicationResponseDto>> GetJobApplicationByIdAsync(int userId)
        {
            
                var query = from application in _context.Applications
                          join applicant in _context.Applicants 
                          on application.ApplicantId equals applicant.UserId
                          where applicant.UserId == userId
                            select new ApplicationResponseDto
                            {
                                Id = application.Id,
                                Status = application.Status.ToString(),
                                JobTitle = application.JobPosting.Title,
                                Location = application.JobPosting.Location,
                                AppliedDate = application.AppliedAt,
                                Company = application.JobPosting.Company.Name
                            };

                           return await query.ToListAsync();

                /*return await _context.Applications
                .Join(_context.Applicants,
                       application =>application.ApplicantId,
                       applicant =>applicant.UserId,
                       (application, applicant)=>new {application, applicant})
                    .Where(joined => joined.applicant.UserId == userId)
                    .Select(joined => new ApplicationResponseDto
                    {
                        Id = joined.application.Id,
                        Status = joined.application.Status.ToString(),
                        JobTitle = joined.application.JobPosting.Title,
                        Location = joined.application.JobPosting.Location,
                        AppliedDate = joined.application.AppliedAt,
                        Company = joined.application.JobPosting.Company.Name // Navigate through JobPosting to Company
                    }).ToListAsync();*/
        }
        public async Task<IEnumerable<ApplicantDto>> GetApplicantsListingsAsync()
        {
           
                return  await _context.Applications
                .Where(a => a.JobPosting.Status == JobStatus.Open)
                .Include(a => a.Applicant)
                .Include(a => a.JobPosting)
                .Select(a => new ApplicantDto
                {
                    Id = a.Id,
                    JobTitle = a.JobPosting.Title,
                    Name = $"{a.Applicant.FirstName} {a.Applicant.LastName}",
                    Education = a.Applicant.EducationLevel.ToString(),
                    Experience = a.Applicant.YearsOfExperience,
                    Status = a.Status.ToString(),
                    Score = (decimal)_context.ApplicantScores
                     .Where( s =>s.ApplicantId == a.ApplicantId)
                     .OrderByDescending(s =>s.CreatedAt)
                     .Select(s =>s.Score)
                     .FirstOrDefault(),
                     Reasoning = _context.ApplicantScores
                        .Where(s =>s.ApplicantId == a.ApplicantId)
                        .OrderByDescending(s =>s.CreatedAt)
                        .Select(s =>s.Reasoning)
                        .FirstOrDefault()
                }).ToListAsync();

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
        public async Task <ApplicantInfoDto> CreateApplicant(CreateApplicantDto createApplicantDto)
        {
            
                if(createApplicantDto == null)
                  throw new ArgumentNullException(nameof(createApplicantDto));

                // Check if applicant exists
                var applicant = await _context.Applicants
                   .FirstOrDefaultAsync(a => a.UserId == createApplicantDto.UserId);

                if (applicant == null)
                {
                    applicant = new Applicant
                    {
                        UserId = createApplicantDto.UserId,
                        FirstName = createApplicantDto.FirstName,
                        LastName = createApplicantDto.LastName,
                        Email = createApplicantDto.Email,
                        PhoneNumber = createApplicantDto.PhoneNumber,
                        EducationLevel =createApplicantDto.EducationLevel,
                        YearsOfExperience = createApplicantDto.YearsOfExperience,
                        Skills = createApplicantDto.Skills,
                        DateOfBirth = DateTime.SpecifyKind(createApplicantDto.DateofBirth,DateTimeKind.Utc),
                        ResumeUrl = "No uploads yet",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "Admin"
                    };

                     _context.Applicants.Add(applicant);
                    await _context.SaveChangesAsync();
                    return _mapper.Map<ApplicantInfoDto>(applicant);
                }
    
           return _mapper.Map<ApplicantInfoDto>(applicant);
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
        public async Task<IEnumerable<ApplicantDto>> GetAllAsync()
        {
            var result = new List<ApplicantDto>();
            
            return result;
        }
        public async Task<ApplicantInfoDto> GetApplicantByIdAsync(Guid Id)
        {
            var response = await _context.Applicants.Where(u =>u.UserId == Id).FirstOrDefaultAsync();

            if(response == null)
              return null;

            return _mapper.Map<ApplicantInfoDto>(response);
        }
    }
}