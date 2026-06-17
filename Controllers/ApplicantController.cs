using Microsoft.AspNetCore.Mvc;
using ATS.API.Models;
using ATS.API.Services;
using ATS.API.DTOs;
using ATS.API.Interfaces;

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
        private readonly IRankingService _rankingservice;

        public ApplicantsController(
            IApplicantService applicantService,
            IShortlistingService shortlistingService,
            INotificationService notificationService,
            IMergeClient mergeClient, IRankingService rankingservice)
        {
            _applicantService = applicantService;
            _shortlistingService = shortlistingService;
            _notificationService = notificationService;
            _mergeClient = mergeClient;
            _rankingservice = rankingservice;
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

        [HttpGet("AllApplicantListings")]
        public async Task<ActionResult<IEnumerable<ApplicantDto>>> GetAllApplicants()
        {
            var applicants = await _applicantService.GetApplicantsListingsAsync();
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
            dto.UserId = 1;
            dto.Comments = $"Status changed to {dto.Status}";

            var application = await _applicantService.UpdateApplicationStatusAsync(
                applicationId, 
                dto.Status, 
                dto.UserId, 
                dto.Comments);

            if (application == null)
                return NotFound($"Application with ID {applicationId} not found.");

            // US-4.2: Send email notification to applicant
            //await _notificationService.SendStatusUpdateEmailAsync(application);

            // Sync with Merge.dev for ATS integration
            //await _mergeClient.UpdateApplicationStatusAsync(application);

            return Ok(application);
        }

        // Create application (for applicant submission)
        [HttpPost("apply")]
        public async Task<ActionResult<ResponseDto>> SubmitApplication([FromBody] CreateApplicationDto1 dto)
        {
            foreach (var kvp in ModelState)
            {
                Console.WriteLine($"Key: {kvp.Key}");

                foreach (var error in kvp.Value.Errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }
            }
            var response = new ResponseDto {Status ="error", Message = "We could not submit the application"};
            if (!ModelState.IsValid)
            {
                response.Message =$"{BadRequest(ModelState).ToString()}. We could not submit the application";
                return response;
            }
                

            response = await _applicantService.CreateApplicationAsync(dto);

            // Sync with external ATS via Merge.dev
            //await _mergeClient.CreateApplicationAsync(application);

            // Send confirmation email
            //if(response.Status=="success")
               //await _notificationService.SendApplicationConfirmationAsync((Application)response.Payload);

            return response;//CreatedAtAction(nameof(GetApplication), new { id = application.Id }, application);
        }
        

        [HttpGet("{applicationId}")]
        public async Task<ActionResult<ApplicationResponseDto>> GetApplication(int applicationId)
        {
            //var application = await _applicantService.GetApplicationByIdAsync(applicationId);
            var applications = await _applicantService.GetJobApplicationByIdAsync(applicationId);
            
            if (applications == null)
                return NotFound($"Application with ID {applicationId} not found.");

            return Ok(applications);
        }

        [HttpGet("jobsapplied/{UserId}")]
        public async Task<ActionResult<IEnumerable<ApplicationResponseDto>>> GetJobApplication(Guid UserId)
        {
            var applications = await _applicantService.GetApplicationByUserIdAsync(UserId);
             
             if (applications == null)
                return NotFound($"Application with ID {UserId} not found.");

            return Ok(applications);
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

        //Applicant Ranking
        [HttpGet("RankApplicants")]
        public async Task<ActionResult> RankApplicants()
        {
            //var rankingServicce = new ApplicantRankingService();
            //Optional: Train model with historical data
            //var historicaldata =LoadHistoricalData();
            //_rankingservice.TrainModel(historicaldata);

            //Rank new Applicants
            var jobdescription = @"
                Senior Software Engineer

                We're looking for a Senior Software Engineer with:
                - 5+ years experience with C# and .NET
                - Experieince with Azure and cloud services
                - Knowledge of microservices architecture
                - Bachelor's degree in Computer Science
                - Strong SQL and REST API skills
            ";
            var applicants = new List<ResumeData>();
            /*{
                new ResumeData {Id = ,
                ApplicantName = "Aggie Gwata", 
                Text="Software Engineer with 7 years experience. Expert in C#, .NET, Azure, SQL ..."},
                new ResumeData {Id = 2,
                ApplicantName = "Comish Omar", 
                Text="Full Stack Developer, 3 years experience with JavaScript, React, Node.js..."}
                
            };*/
            var rankedApplicants =await _rankingservice.RankApplicants(jobdescription, applicants);
            foreach(var applicant in rankedApplicants)
            {
                Console.WriteLine($"Applicant: {applicant.ApplicantId}");
                Console.WriteLine($"Score: {applicant.Score:F2}");
                Console.WriteLine($"Matched Skills: {string.Join(", ", applicant.MatchedSkills)}");
                Console.WriteLine($"Missing Skills: {string.Join(", ", applicant.MissingSkills)}");
                Console.WriteLine($"Reasoning: {applicant.Reasoning}");
                Console.WriteLine(new string('-', 50));
            }

            return Ok(rankedApplicants);
        }

         [HttpGet("AutoRankApplicants")]
        public async Task<ActionResult> AutoRankApplicants()
        {
            var response = new ResponseDto {Status ="success", Message="Auto ranking initiated successfully"};
            await _shortlistingService.AutoRankApplicants();
            return Ok(response);
        }
        // US: Upload resume/CV file
        [HttpPost("upload-resume")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ResumeUploadResultDto>> UploadResume(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file was provided.");

              // Validate file type — only PDF, DOC, DOCX allowed
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
               return BadRequest($"Invalid file type '{extension}'. Only PDF, DOC, and DOCX are accepted.");

             // Validate file size — max 5 MB
            const long maxFileSizeBytes = 5 * 1024 * 1024;
            if (file.Length > maxFileSizeBytes)
                return BadRequest("File size exceeds the 5 MB limit.");

            // Generate a unique file name to prevent collisions
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";

             // Define upload path (adjust folder as needed, or swap for cloud storage)
            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "resumes");
            Directory.CreateDirectory(uploadFolder); // ensure folder exists

             var filePath = Path.Combine(uploadFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Build a URL the client can store and reference later
            var resumeUrl = $"/resumes/{uniqueFileName}";

            return Ok(new ResumeUploadResultDto
            {
                ResumeUrl = resumeUrl,
                FileName = file.FileName,
                FileSizeBytes = file.Length
            });
        }
        [HttpPost("CreateApplicant")]
        public async Task<ActionResult<ApplicantInfoDto>> Create(CreateApplicantDto dto)
        {
            if(dto == null) return BadRequest("Null or invald data. Failed to save to database") ;

            var result = await _applicantService.CreateApplicant(dto);
            return result; //CreatedAtAction(nameof(GetApplication), new { applicationId= result.Id }, result);
        }
        [HttpGet("GetApplicantData/{UserId}")]
        public async Task<ActionResult<ApplicantInfoDto>> GetApplicantInfo(Guid UserId)
        {
            var response = await _applicantService.GetApplicantByIdAsync(UserId);
            return response;
        }
    }
}
