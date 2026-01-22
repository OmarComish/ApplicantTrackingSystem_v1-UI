using Microsoft.AspNetCore.Mvc;
using ATS.API.Models;
using ATS.API.Services;
using ATS.API.DTOs;

namespace ATS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobPostingsController : ControllerBase
    {
        private readonly IJobPostingService _jobPostingService;
        private readonly IOpenCatsClient _openCatsClient;

        public JobPostingsController(
            IJobPostingService jobPostingService,
            IOpenCatsClient openCatsClient)
        {
            _jobPostingService = jobPostingService;
            _openCatsClient = openCatsClient;
        }

        // US-1.1: Create a job posting
        [HttpPost]
        public async Task<ActionResult<JobPosting>> CreateJobPosting([FromBody] CreateJobPostingDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var jobPosting = await _jobPostingService.CreateJobPostingAsync(dto);
            
            // Sync with OpenCATS
            await _openCatsClient.CreateJobOrderAsync(jobPosting);
            
            return CreatedAtAction(nameof(GetJobPosting), new { id = jobPosting.Id }, jobPosting);
        }

        // US-1.2: Edit an existing job posting
        [HttpPut("{id}")]
        public async Task<ActionResult<JobPosting>> UpdateJobPosting(int id, [FromBody] UpdateJobPostingDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var jobPosting = await _jobPostingService.UpdateJobPostingAsync(id, dto);
            
            if (jobPosting == null)
                return NotFound($"Job posting with ID {id} not found.");

            // Sync with OpenCATS
            await _openCatsClient.UpdateJobOrderAsync(jobPosting);

            return Ok(jobPosting);
        }

        // US-1.3: Delete or close a job posting
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteJobPosting(int id)
        {
            var result = await _jobPostingService.DeleteJobPostingAsync(id);
            
            if (!result)
                return NotFound($"Job posting with ID {id} not found.");

            return NoContent();
        }

        [HttpPost("{id}/close")]
        public async Task<ActionResult<JobPosting>> CloseJobPosting(int id)
        {
            var jobPosting = await _jobPostingService.CloseJobPostingAsync(id);
            
            if (jobPosting == null)
                return NotFound($"Job posting with ID {id} not found.");

            return Ok(jobPosting);
        }

        // US-1.4: View all job postings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<JobPosting>>> GetAllJobPostings(
            [FromQuery] JobStatus? status = null,
            [FromQuery] string department = null)
        {
            var jobPostings = await _jobPostingService.GetAllJobPostingsAsync(status, department);
            return Ok(jobPostings);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<JobPosting>> GetJobPosting(int id)
        {
            var jobPosting = await _jobPostingService.GetJobPostingByIdAsync(id);
            
            if (jobPosting == null)
                return NotFound($"Job posting with ID {id} not found.");

            return Ok(jobPosting);
        }

        // Sync job postings with external systems
        [HttpPost("sync")]
        public async Task<ActionResult> SyncJobPostings()
        {
            var jobPostings = await _jobPostingService.GetAllJobPostingsAsync();
            
            foreach (var job in jobPostings.Where(j => j.Status == JobStatus.Open))
            {
                await _openCatsClient.SyncJobOrderAsync(job);
            }

            return Ok(new { message = "Job postings synced successfully" });
        }
    }
}
