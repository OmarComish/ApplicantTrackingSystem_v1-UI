using Microsoft.AspNetCore.Mvc;
using ATS.API.Models;
using ATS.API.Services;
using ATS.API.DTOs;

namespace ATS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicantsController : ControllerBase
    {
        private readonly IApplicantService _applicantService;
        private readonly IShortlistingService _shortlistingService;
        private readonly INotificationService _notificationService;
        private readonly IMergeClient _mergeClient;

        public ApplicantsController(
            IApplicantService applicantService,
            IShortlistingService shortlistingService,
            INotificationService notificationService,
            IMergeClient mergeClient)
        {
            _applicantService = applicantService;
            _shortlistingService = shortlistingService;
            _notificationService = notificationService;
            _mergeClient = mergeClient;
        }

        // US-2.1: View applicants for a specific job posting
        [HttpGet("job/{jobPostingId}")]
        public async Task<ActionResult<IEnumerable<ApplicationDetailsDto>>> GetApplicantsByJobPosting(int jobPostingId)
        {
            var applicants = await _applicantService.GetApplicantsByJobPostingAsync(jobPostingId);
            return Ok(applicants);
        }

        // US-2.2: Filter applicants by education level
        [HttpGet("job/{jobPostingId}/filter/education")]
        public async Task<ActionResult<IEnumerable<ApplicationDetailsDto>>> FilterByEducation(
            int jobPostingId,
            [FromQuery] EducationLevel educationLevel)
        {
            var applicants = await _applicantService.FilterByEducationAsync(jobPostingId, educationLevel);
            return Ok(applicants);
        }

        // US-2.3: Rank applicants based on years of experience
        [HttpGet("job/{jobPostingId}/rank")]
        public async Task<ActionResult<IEnumerable<ApplicationDetailsDto>>> RankByExperience(int jobPostingId)
        {
            var rankedApplicants = await _shortlistingService.RankByExperienceAsync(jobPostingId);
            return Ok(rankedApplicants);
        }

        // US-2.4: View shortlisted applicants
        [HttpGet("job/{jobPostingId}/shortlisted")]
        public async Task<ActionResult<IEnumerable<ApplicationDetailsDto>>> GetShortlistedApplicants(int jobPostingId)
        {
            var shortlisted = await _shortlistingService.GetShortlistedApplicantsAsync(jobPostingId);
            return Ok(shortlisted);
        }

        // Auto-shortlist based on criteria
        [HttpPost("job/{jobPostingId}/auto-shortlist")]
        public async Task<ActionResult<ShortlistResultDto>> AutoShortlist(
            int jobPostingId,
            [FromBody] ShortlistCriteriaDto criteria)
        {
            var result = await _shortlistingService.AutoShortlistAsync(jobPostingId, criteria);
            return Ok(result);
        }

        // US-4.1: Update applicant status
        [HttpPatch("{applicationId}/status")]
        public async Task<ActionResult<Application>> UpdateApplicationStatus(
            int applicationId,
            [FromBody] UpdateStatusDto dto)
        {
            var application = await _applicantService.UpdateApplicationStatusAsync(
                applicationId, 
                dto.Status, 
                dto.UserId, 
                dto.Comments);

            if (application == null)
                return NotFound($"Application with ID {applicationId} not found.");

            // US-4.2: Send email notification to applicant
            await _notificationService.SendStatusUpdateEmailAsync(application);

            // Sync with Merge.dev for ATS integration
            await _mergeClient.UpdateApplicationStatusAsync(application);

            return Ok(application);
        }

        // Create application (for applicant submission)
        [HttpPost("apply")]
        public async Task<ActionResult<Application>> SubmitApplication([FromBody] CreateApplicationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var application = await _applicantService.CreateApplicationAsync(dto);

            // Sync with external ATS via Merge.dev
            await _mergeClient.CreateApplicationAsync(application);

            // Send confirmation email
            await _notificationService.SendApplicationConfirmationAsync(application);

            return CreatedAtAction(nameof(GetApplication), new { id = application.Id }, application);
        }

        [HttpGet("{applicationId}")]
        public async Task<ActionResult<Application>> GetApplication(int applicationId)
        {
            var application = await _applicantService.GetApplicationByIdAsync(applicationId);
            
            if (application == null)
                return NotFound($"Application with ID {applicationId} not found.");

            return Ok(application);
        }

        // Bulk import candidates from external ATS
        [HttpPost("import")]
        public async Task<ActionResult> ImportCandidates([FromBody] ImportCandidatesDto dto)
        {
            var candidates = await _mergeClient.GetCandidatesAsync(dto.IntegrationId);
            
            foreach (var candidate in candidates)
            {
                await _applicantService.ImportCandidateAsync(candidate);
            }

            return Ok(new { imported = candidates.Count(), message = "Candidates imported successfully" });
        }
    }
}
