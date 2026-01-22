using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using ATS.API.Data;
using ATS.API.Models;
using ATS.API.DTOs;

namespace ATS.API.Services
{
    // Notification Service Interface
    public interface INotificationService
    {
        Task SendApplicationConfirmationAsync(Application application);
        Task SendStatusUpdateEmailAsync(Application application);
        Task<NotificationSettings> GetNotificationSettingsAsync(int userId);
        Task<NotificationSettings> UpdateNotificationSettingsAsync(int userId, UpdateNotificationSettingsDto dto);
    }

    // Notification Service Implementation
    public class NotificationService : INotificationService
    {
        private readonly AtsDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            AtsDbContext context,
            IConfiguration configuration,
            ILogger<NotificationService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendApplicationConfirmationAsync(Application application)
        {
            try
            {
                var applicant = await _context.Applicants.FindAsync(application.ApplicantId);
                var jobPosting = await _context.JobPostings.FindAsync(application.JobPostingId);

                if (applicant == null || jobPosting == null)
                    return;

                var subject = $"Application Confirmation - {jobPosting.Title}";
                var body = $@"
                    <html>
                    <body>
                        <h2>Thank you for your application!</h2>
                        <p>Dear {applicant.FirstName} {applicant.LastName},</p>
                        <p>We have received your application for the position of <strong>{jobPosting.Title}</strong>.</p>
                        <p>Our team will review your application and get back to you soon.</p>
                        <p>Application Details:</p>
                        <ul>
                            <li>Position: {jobPosting.Title}</li>
                            <li>Department: {jobPosting.Department}</li>
                            <li>Applied Date: {application.AppliedAt:yyyy-MM-dd}</li>
                        </ul>
                        <p>Best regards,<br/>HR Team</p>
                    </body>
                    </html>
                ";

                await SendEmailAsync(applicant.Email, subject, body);
                _logger.LogInformation("Sent confirmation email to {Email}", applicant.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending application confirmation email");
            }
        }

        public async Task SendStatusUpdateEmailAsync(Application application)
        {
            try
            {
                var applicant = await _context.Applicants.FindAsync(application.ApplicantId);
                var jobPosting = await _context.JobPostings.FindAsync(application.JobPostingId);

                if (applicant == null || jobPosting == null)
                    return;

                // Check notification settings
                var hrUsers = await _context.Users
                    .Where(u => u.Role == UserRole.HRUser || u.Role == UserRole.HRAdmin)
                    .ToListAsync();

                foreach (var user in hrUsers)
                {
                    var settings = await GetNotificationSettingsAsync(user.Id);
                    if (settings?.EmailOnStatusChange == true)
                    {
                        var subject = $"Application Status Updated - {jobPosting.Title}";
                        var body = $@"
                            <html>
                            <body>
                                <h2>Application Status Updated</h2>
                                <p>Dear {user.FirstName},</p>
                                <p>The application status for <strong>{applicant.FirstName} {applicant.LastName}</strong> has been updated.</p>
                                <p>Details:</p>
                                <ul>
                                    <li>Position: {jobPosting.Title}</li>
                                    <li>Candidate: {applicant.FirstName} {applicant.LastName}</li>
                                    <li>New Status: {application.Status}</li>
                                    <li>Updated: {application.StatusUpdatedAt:yyyy-MM-dd HH:mm}</li>
                                </ul>
                            </body>
                            </html>
                        ";

                        await SendEmailAsync(user.Email, subject, body);
                    }
                }

                // Send email to applicant
                var applicantSubject = $"Application Status Update - {jobPosting.Title}";
                var applicantBody = $@"
                    <html>
                    <body>
                        <h2>Application Status Update</h2>
                        <p>Dear {applicant.FirstName} {applicant.LastName},</p>
                        <p>Your application for the position of <strong>{jobPosting.Title}</strong> has been updated.</p>
                        <p>Current Status: <strong>{application.Status}</strong></p>
                        {(application.Status == ApplicationStatus.Shortlisted ? 
                            "<p>Congratulations! You have been shortlisted for this position. We will contact you soon for the next steps.</p>" : "")}
                        {(application.Status == ApplicationStatus.Interviewing ? 
                            "<p>We would like to invite you for an interview. Our team will contact you shortly to schedule a time.</p>" : "")}
                        {(application.Status == ApplicationStatus.Rejected ? 
                            "<p>Thank you for your interest. Unfortunately, we have decided to move forward with other candidates at this time.</p>" : "")}
                        <p>Best regards,<br/>HR Team</p>
                    </body>
                    </html>
                ";

                await SendEmailAsync(applicant.Email, applicantSubject, applicantBody);
                _logger.LogInformation("Sent status update email to {Email}", applicant.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending status update email");
            }
        }

        public async Task<NotificationSettings> GetNotificationSettingsAsync(int userId)
        {
            var settings = await _context.NotificationSettings
                .FirstOrDefaultAsync(n => n.UserId == userId);

            if (settings == null)
            {
                // Create default settings
                settings = new NotificationSettings
                {
                    UserId = userId,
                    EmailOnNewApplication = true,
                    EmailOnStatusChange = true,
                    EmailOnShortlist = true
                };

                _context.NotificationSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            return settings;
        }

        public async Task<NotificationSettings> UpdateNotificationSettingsAsync(
            int userId, 
            UpdateNotificationSettingsDto dto)
        {
            var settings = await GetNotificationSettingsAsync(userId);

            if (dto.EmailOnNewApplication.HasValue)
                settings.EmailOnNewApplication = dto.EmailOnNewApplication.Value;
            
            if (dto.EmailOnStatusChange.HasValue)
                settings.EmailOnStatusChange = dto.EmailOnStatusChange.Value;
            
            if (dto.EmailOnShortlist.HasValue)
                settings.EmailOnShortlist = dto.EmailOnShortlist.Value;

            await _context.SaveChangesAsync();
            return settings;
        }

        private async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var smtpHost = _configuration["Smtp:Host"];
                var smtpPort = int.Parse(_configuration["Smtp:Port"] ?? "587");
                var smtpUsername = _configuration["Smtp:Username"];
                var smtpPassword = _configuration["Smtp:Password"];
                var fromEmail = _configuration["Smtp:FromEmail"];

                using var smtpClient = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(to);

                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {To}", to);
                throw;
            }
        }
    }

    // User Service Interface
    public interface IUserService
    {
        Task<User> CreateUserAsync(CreateUserDto dto);
        Task<User> UpdateUserAsync(int id, UpdateUserDto dto);
        Task<bool> DeleteUserAsync(int id);
        Task<User> GetUserByIdAsync(int id);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> ActivateUserAsync(int id);
        Task<User> DeactivateUserAsync(int id);
    }

    // User Service Implementation
    public class UserService : IUserService
    {
        private readonly AtsDbContext _context;

        public UserService(AtsDbContext context)
        {
            _context = context;
        }

        public async Task<User> CreateUserAsync(CreateUserDto dto)
        {
            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Role = dto.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User> UpdateUserAsync(int id, UpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            
            if (user == null)
                return null;

            if (!string.IsNullOrEmpty(dto.FirstName))
                user.FirstName = dto.FirstName;
            
            if (!string.IsNullOrEmpty(dto.LastName))
                user.LastName = dto.LastName;
            
            if (!string.IsNullOrEmpty(dto.Email))
                user.Email = dto.Email;
            
            if (dto.Role.HasValue)
                user.Role = dto.Role.Value;
            
            if (dto.IsActive.HasValue)
                user.IsActive = dto.IsActive.Value;

            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            
            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();
        }

        public async Task<User> ActivateUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            
            if (user == null)
                return null;

            user.IsActive = true;
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> DeactivateUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            
            if (user == null)
                return null;

            user.IsActive = false;
            await _context.SaveChangesAsync();
            return user;
        }
    }
}
