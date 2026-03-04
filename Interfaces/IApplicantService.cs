using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ATS.API.DTOs;
using ATS.API.Models;

namespace ATS.API.Interfaces
{
    public interface IApplicantService
    {
        Task<Application> CreateApplicationAsync(CreateApplicationDto dto);
        Task<Application> GetApplicationByIdAsync(int id);
        Task<IEnumerable<ApplicationDetailsDto>> GetApplicantsByJobPostingAsync(int jobPostingId);
        Task<IEnumerable<ApplicationDetailsDto>> FilterByEducationAsync(int jobPostingId, EducationLevel educationLevel);
        Task<Application> UpdateApplicationStatusAsync(int applicationId, ApplicationStatus status, int userId, string comments);
        Task ImportCandidateAsync(ExternalCandidateDto candidate);
        Task <ApplicantDto> CreateApplicant(CreateApplicantDto createApplicantDto);
        Task<IEnumerable<ApplicantDto>> GetAllAsync();
        Task<Application> GetJobApplicationByIdAsync(int id);
    }
}