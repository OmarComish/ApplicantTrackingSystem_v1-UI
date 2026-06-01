using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using ATS.API.DTOs;
using ATS.API.Interfaces;
using Newtonsoft.Json;

namespace ATS.API.Services;
public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    public ApiService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }
    public async Task<TokenResponseDto> GetTokenAsync(TokenRequestDto request)
    {
        Console.WriteLine($"Token request getting this far?:");
         var handler = new HttpClientHandler
        {
            AllowAutoRedirect = false
        };

        using var httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        httpClient.DefaultRequestHeaders.Clear();

        var tokenEndpoint = $"{_config["IdentityService:identityServerUrl"].TrimEnd('/')}/connect/token";
        Console.WriteLine($"---------url: {tokenEndpoint}");
        var formData = new Dictionary<string, string>
        {
            { "grant_type", request.GrantType },
            { "username", request.Username },
            { "password", request.Password },
            { "client_id", request.ClientId },
            { "client_secret", request.ClientSecret },
            { "scope", request.Scope }
        };
        Console.WriteLine($"Token request:");
        foreach (var kvp in formData)
        {
            Console.WriteLine($"{kvp.Key} = {kvp.Value}");
        }
        var content = new FormUrlEncodedContent(formData);

        var response = await httpClient.PostAsync(tokenEndpoint, content);
        var responseContent = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"Status: {response.StatusCode}");
        Console.WriteLine($"Response: {responseContent}");

        if (!response.IsSuccessStatusCode)
        {
            return new TokenResponseDto
            {
                Error = response.StatusCode.ToString(),
                ErrorDescription = responseContent
            };
        }


        return System.Text.Json.JsonSerializer.Deserialize<TokenResponseDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        //return JsonConvert.DeserializeObject<TokenResponseDto>(responseContent);
        /*{
            PropertyNameCaseInsensitive = true
        });*/

    }

    public async Task<ResponseDto> RegisterUserAsync(RegisterUserDto request)
    {
        var response = new ResponseDto{Status="error", Message="Failed to register user account"};
        
        _httpClient.BaseAddress = new Uri(_config["Endpoint:BASEUri"]);
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //serialize request data to JSON
        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage result = await _httpClient.PostAsync(_config["Endpoint:RegisterUser"],content);
        if(result.IsSuccessStatusCode)
        {
            //read and return the response as a string
            var responsecontent = await result.Content.ReadAsStringAsync();
            response = JsonConvert.DeserializeObject<ResponseDto>(responsecontent);
        }
        else
        {
            //handle errors
            throw new Exception($"Request failed with status code {result.StatusCode}");
        }
        return response;
    }
    public async Task<AuthResponseDto> SignInAsync(LogInRequestDto request)
    {
        var response = new AuthResponseDto {Status ="error", Message ="Invalid username or password. Failed to login"};

        var userdetails = await GetUserDetails(request.Username);
        if(userdetails == null)
           return response;

        request.Username = userdetails?.NormalizedUserName;
        
        //_httpClient.BaseAddress = new Uri(_config["IdentityService:identityServerUrl"]);
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //serialize request data to json
        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        //send post request
        var url = $"{_config["IdentityService:identityServerUrl"]}{_config["Endpoint:UserLogin"]}"; 

        HttpResponseMessage result = await _httpClient.PostAsync(url, content);
        if(result.IsSuccessStatusCode)
        {
            var responsecontent = await result.Content.ReadAsStringAsync();
            response = JsonConvert.DeserializeObject<AuthResponseDto>(responsecontent);
            return response;
        }
        else
        {
            throw new Exception($"Request failed with status code {result.StatusCode}");
        }
    }
    public async Task<UserDetailsDto> GetUserDetails(string email)
    {
        // Set up HttpClient base address and accept header
        //_httpClient.BaseAddress = new Uri(_config["IdentityService:identityServerUrl"]);
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Build URL with email query parameter (encode to handle special characters)
        string url = $"{_config["IdentityService:identityServerUrl"]}{_config["Endpoint:GetUserDetails"]}{Uri.EscapeDataString(email)}";

        // Send GET request
        var httpResponse = await _httpClient.GetAsync(url);

        if (httpResponse.IsSuccessStatusCode)
        {
            // Read and deserialize the JSON response to UserDetailsDto
            var json = await httpResponse.Content.ReadAsStringAsync();
            var userDetails = JsonConvert.DeserializeObject<UserDetailsDto>(json);
            return userDetails;
        }
        else
        {
            // Handle error cases - throw an exception with details
            throw new HttpRequestException(
                $"Failed to get user details for email '{email}'. Status code: {httpResponse.StatusCode}");
        }
    }
}