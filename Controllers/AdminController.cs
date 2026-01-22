using Microsoft.AspNetCore.Mvc;
using ATS.API.Models;
using ATS.API.Services;
using ATS.API.DTOs;

namespace ATS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;

        public AdminController(
            IAnalyticsService analyticsService,
            IUserService userService,
            INotificationService notificationService)
        {
            _analyticsService = analyticsService;
            _userService = userService;
            _notificationService = notificationService;
        }

        // US-3.1: View analytics
        [HttpGet("analytics")]
        public async Task<ActionResult<AnalyticsDto>> GetAnalytics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var analytics = await _analyticsService.GetRecruitmentAnalyticsAsync(startDate, endDate);
            return Ok(analytics);
        }

        [HttpGet("analytics/jobs")]
        public async Task<ActionResult<JobAnalyticsDto>> GetJobAnalytics()
        {
            var analytics = await _analyticsService.GetJobAnalyticsAsync();
            return Ok(analytics);
        }

        [HttpGet("analytics/applicants")]
        public async Task<ActionResult<ApplicantAnalyticsDto>> GetApplicantAnalytics()
        {
            var analytics = await _analyticsService.GetApplicantAnalyticsAsync();
            return Ok(analytics);
        }

        [HttpGet("analytics/shortlist")]
        public async Task<ActionResult<ShortlistAnalyticsDto>> GetShortlistAnalytics()
        {
            var analytics = await _analyticsService.GetShortlistAnalyticsAsync();
            return Ok(analytics);
        }

        // US-3.2: Manage users
        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("users/{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            
            if (user == null)
                return NotFound($"User with ID {id} not found.");

            return Ok(user);
        }

        [HttpPost("users")]
        public async Task<ActionResult<User>> CreateUser([FromBody] CreateUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userService.CreateUserAsync(dto);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        [HttpPut("users/{id}")]
        public async Task<ActionResult<User>> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userService.UpdateUserAsync(id, dto);
            
            if (user == null)
                return NotFound($"User with ID {id} not found.");

            return Ok(user);
        }

        [HttpDelete("users/{id}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteUserAsync(id);
            
            if (!result)
                return NotFound($"User with ID {id} not found.");

            return NoContent();
        }

        [HttpPost("users/{id}/activate")]
        public async Task<ActionResult<User>> ActivateUser(int id)
        {
            var user = await _userService.ActivateUserAsync(id);
            
            if (user == null)
                return NotFound($"User with ID {id} not found.");

            return Ok(user);
        }

        [HttpPost("users/{id}/deactivate")]
        public async Task<ActionResult<User>> DeactivateUser(int id)
        {
            var user = await _userService.DeactivateUserAsync(id);
            
            if (user == null)
                return NotFound($"User with ID {id} not found.");

            return Ok(user);
        }

        // US-3.3: Export reports
        [HttpGet("export/applications")]
        public async Task<ActionResult> ExportApplications(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string format = "csv")
        {
            var data = await _analyticsService.ExportApplicationsAsync(startDate, endDate);
            
            if (format.ToLower() == "csv")
            {
                var csv = _analyticsService.ConvertToCsv(data);
                return File(csv, "text/csv", $"applications_{DateTime.UtcNow:yyyyMMdd}.csv");
            }
            
            return Ok(data);
        }

        [HttpGet("export/shortlist")]
        public async Task<ActionResult> ExportShortlist(
            [FromQuery] int? jobPostingId = null,
            [FromQuery] string format = "csv")
        {
            var data = await _analyticsService.ExportShortlistAsync(jobPostingId);
            
            if (format.ToLower() == "csv")
            {
                var csv = _analyticsService.ConvertToCsv(data);
                return File(csv, "text/csv", $"shortlist_{DateTime.UtcNow:yyyyMMdd}.csv");
            }
            
            return Ok(data);
        }

        // US-4.3: Manage email notification settings
        [HttpGet("notifications/settings/{userId}")]
        public async Task<ActionResult<NotificationSettings>> GetNotificationSettings(int userId)
        {
            var settings = await _notificationService.GetNotificationSettingsAsync(userId);
            
            if (settings == null)
                return NotFound($"Notification settings for user {userId} not found.");

            return Ok(settings);
        }

        [HttpPut("notifications/settings/{userId}")]
        public async Task<ActionResult<NotificationSettings>> UpdateNotificationSettings(
            int userId,
            [FromBody] UpdateNotificationSettingsDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var settings = await _notificationService.UpdateNotificationSettingsAsync(userId, dto);
            return Ok(settings);
        }
    }
}
