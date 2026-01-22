using System.Net.Http.Json;
using System.Text.Json;
using ATS.API.Models;
using ATS.API.DTOs;

namespace ATS.API.Services
{
    // OpenCATS Client Interface
    public interface IOpenCatsClient
    {
        Task<bool> CreateJobOrderAsync(JobPosting jobPosting);
        Task<bool> UpdateJobOrderAsync(JobPosting jobPosting);
        Task<bool> SyncJobOrderAsync(JobPosting jobPosting);
        Task<IEnumerable<ExternalCandidateDto>> GetCandidatesAsync();
    }

    // OpenCATS Client Implementation
    public class OpenCatsClient : IOpenCatsClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OpenCatsClient> _logger;

        public OpenCatsClient(HttpClient httpClient, ILogger<OpenCatsClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<bool> CreateJobOrderAsync(JobPosting jobPosting)
        {
            try
            {
                var payload = new
                {
                    title = jobPosting.Title,
                    description = jobPosting.Description,
                    department = jobPosting.Department,
                    location = jobPosting.Location,
                    requirements = jobPosting.Requirements,
                    status = jobPosting.Status.ToString()
                };

                var response = await _httpClient.PostAsJsonAsync("/api/job-orders", payload);
                response.EnsureSuccessStatusCode();
                
                _logger.LogInformation("Created job order in OpenCATS: {JobId}", jobPosting.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating job order in OpenCATS");
                return false;
            }
        }

        public async Task<bool> UpdateJobOrderAsync(JobPosting jobPosting)
        {
            try
            {
                var payload = new
                {
                    title = jobPosting.Title,
                    description = jobPosting.Description,
                    department = jobPosting.Department,
                    location = jobPosting.Location,
                    requirements = jobPosting.Requirements,
                    status = jobPosting.Status.ToString()
                };

                var response = await _httpClient.PutAsJsonAsync($"/api/job-orders/{jobPosting.Id}", payload);
                response.EnsureSuccessStatusCode();
                
                _logger.LogInformation("Updated job order in OpenCATS: {JobId}", jobPosting.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating job order in OpenCATS");
                return false;
            }
        }

        public async Task<bool> SyncJobOrderAsync(JobPosting jobPosting)
        {
            try
            {
                // Check if job exists in OpenCATS
                var checkResponse = await _httpClient.GetAsync($"/api/job-orders/{jobPosting.Id}");
                
                if (checkResponse.IsSuccessStatusCode)
                {
                    return await UpdateJobOrderAsync(jobPosting);
                }
                else
                {
                    return await CreateJobOrderAsync(jobPosting);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing job order with OpenCATS");
                return false;
            }
        }

        public async Task<IEnumerable<ExternalCandidateDto>> GetCandidatesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/candidates");
                response.EnsureSuccessStatusCode();
                
                var candidates = await response.Content.ReadFromJsonAsync<List<ExternalCandidateDto>>();
                return candidates ?? new List<ExternalCandidateDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching candidates from OpenCATS");
                return new List<ExternalCandidateDto>();
            }
        }
    }

    // Apideck Client Interface (Unified API for HR integrations)
    public interface IApideckClient
    {
        Task<bool> SyncApplicantAsync(Applicant applicant);
        Task<IEnumerable<ExternalCandidateDto>> GetApplicantsAsync(string connectionId);
    }

    // Apideck Client Implementation
    public class ApideckClient : IApideckClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApideckClient> _logger;

        public ApideckClient(HttpClient httpClient, ILogger<ApideckClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<bool> SyncApplicantAsync(Applicant applicant)
        {
            try
            {
                var payload = new
                {
                    first_name = applicant.FirstName,
                    last_name = applicant.LastName,
                    email = applicant.Email,
                    phone_number = applicant.PhoneNumber,
                    resume_url = applicant.ResumeUrl,
                    education_level = applicant.EducationLevel.ToString(),
                    years_of_experience = applicant.YearsOfExperience
                };

                var response = await _httpClient.PostAsJsonAsync("/ats/applicants", payload);
                response.EnsureSuccessStatusCode();
                
                _logger.LogInformation("Synced applicant to Apideck: {ApplicantId}", applicant.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing applicant to Apideck");
                return false;
            }
        }

        public async Task<IEnumerable<ExternalCandidateDto>> GetApplicantsAsync(string connectionId)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Add("x-apideck-consumer-id", connectionId);
                
                var response = await _httpClient.GetAsync("/ats/applicants");
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<ApideckResponse>();
                return result?.Data ?? new List<ExternalCandidateDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching applicants from Apideck");
                return new List<ExternalCandidateDto>();
            }
        }

        private class ApideckResponse
        {
            public List<ExternalCandidateDto> Data { get; set; }
        }
    }

    // Knit Client Interface (API integration platform)
    public interface IKnitClient
    {
        Task<bool> CreateIntegrationAsync(string provider, Dictionary<string, string> credentials);
        Task<IEnumerable<ExternalCandidateDto>> FetchCandidatesAsync(string integrationId);
    }

    // Knit Client Implementation
    public class KnitClient : IKnitClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<KnitClient> _logger;

        public KnitClient(HttpClient httpClient, ILogger<KnitClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<bool> CreateIntegrationAsync(string provider, Dictionary<string, string> credentials)
        {
            try
            {
                var payload = new
                {
                    provider = provider,
                    credentials = credentials
                };

                var response = await _httpClient.PostAsJsonAsync("/integrations", payload);
                response.EnsureSuccessStatusCode();
                
                _logger.LogInformation("Created Knit integration: {Provider}", provider);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Knit integration");
                return false;
            }
        }

        public async Task<IEnumerable<ExternalCandidateDto>> FetchCandidatesAsync(string integrationId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/integrations/{integrationId}/candidates");
                response.EnsureSuccessStatusCode();
                
                var candidates = await response.Content.ReadFromJsonAsync<List<ExternalCandidateDto>>();
                return candidates ?? new List<ExternalCandidateDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching candidates from Knit");
                return new List<ExternalCandidateDto>();
            }
        }
    }

    // Merge.dev Client Interface (Unified ATS API)
    public interface IMergeClient
    {
        Task<bool> CreateApplicationAsync(Application application);
        Task<bool> UpdateApplicationStatusAsync(Application application);
        Task<IEnumerable<ExternalCandidateDto>> GetCandidatesAsync(string accountToken);
        Task<bool> SyncJobPostingAsync(JobPosting jobPosting);
    }

    // Merge.dev Client Implementation
    public class MergeClient : IMergeClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MergeClient> _logger;

        public MergeClient(HttpClient httpClient, ILogger<MergeClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<bool> CreateApplicationAsync(Application application)
        {
            try
            {
                var payload = new
                {
                    candidate = new
                    {
                        first_name = application.Applicant.FirstName,
                        last_name = application.Applicant.LastName,
                        email = application.Applicant.Email,
                        phone_number = application.Applicant.PhoneNumber
                    },
                    job = application.JobPostingId.ToString(),
                    applied_at = application.AppliedAt,
                    status = application.Status.ToString()
                };

                var response = await _httpClient.PostAsJsonAsync("/ats/v1/applications", payload);
                response.EnsureSuccessStatusCode();
                
                _logger.LogInformation("Created application in Merge: {ApplicationId}", application.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating application in Merge");
                return false;
            }
        }

        public async Task<bool> UpdateApplicationStatusAsync(Application application)
        {
            try
            {
                var payload = new
                {
                    current_stage = application.Status.ToString(),
                    reject_reason = application.Status == ApplicationStatus.Rejected 
                        ? application.Notes 
                        : null
                };

                var response = await _httpClient.PatchAsJsonAsync(
                    $"/ats/v1/applications/{application.Id}", 
                    payload);
                response.EnsureSuccessStatusCode();
                
                _logger.LogInformation("Updated application status in Merge: {ApplicationId}", application.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating application status in Merge");
                return false;
            }
        }

        public async Task<IEnumerable<ExternalCandidateDto>> GetCandidatesAsync(string accountToken)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Add("X-Account-Token", accountToken);
                
                var response = await _httpClient.GetAsync("/ats/v1/candidates");
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<MergeResponse>();
                return result?.Results ?? new List<ExternalCandidateDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching candidates from Merge");
                return new List<ExternalCandidateDto>();
            }
        }

        public async Task<bool> SyncJobPostingAsync(JobPosting jobPosting)
        {
            try
            {
                var payload = new
                {
                    name = jobPosting.Title,
                    description = jobPosting.Description,
                    departments = new[] { jobPosting.Department },
                    offices = new[] { jobPosting.Location },
                    status = jobPosting.Status.ToString()
                };

                var response = await _httpClient.PostAsJsonAsync("/ats/v1/jobs", payload);
                response.EnsureSuccessStatusCode();
                
                _logger.LogInformation("Synced job posting to Merge: {JobId}", jobPosting.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing job posting to Merge");
                return false;
            }
        }

        private class MergeResponse
        {
            public List<ExternalCandidateDto> Results { get; set; }
        }
    }
}
