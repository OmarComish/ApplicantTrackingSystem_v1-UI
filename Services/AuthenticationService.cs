using System.Reflection.Metadata.Ecma335;
using ATS.API.DTOs;
using ATS.API.Interfaces;

namespace ATS.API.Services;
public class AuthenticationService : IAuthentication
{
    private readonly IApiService _apiservice;
    private readonly IConfiguration _config;
    public AuthenticationService(IApiService apiservice, IConfiguration config)
    {
        _apiservice = apiservice;
        _config = config;
    }
    public async Task<AuthResponseDto> AuthenticateUser(LogInRequestDto dto)
    {
        
        Console.WriteLine("Requesting signin to identity server...");

        var userdetails = await _apiservice.SignInAsync(dto);

        if(userdetails == null)
        {
            return new AuthResponseDto
            {
                Status ="error",
                Message ="Invalid username or password",
            };
        } 

        Console.WriteLine("Signin successful...");
        var tokenRequest = new TokenRequestDto
        {
            GrantType = _config["IdentityService:grant_type"],
            Username = dto.Username,
            Password = dto.Password,
            ClientId = _config["IdentityService:client_id"],
            ClientSecret = _config["IdentityService:client_secret"],
            Scope = _config["IdentityService:scope"],
        };
     
        Console.WriteLine($"Logging tokenRequest Object...");
        Console.WriteLine($"{_config["IdentityService:grant_type"]}");
        Console.WriteLine($"{_config["IdentityService:client_id"]}");
        Console.WriteLine($"{_config["IdentityService:client_secret"]}");
        Console.WriteLine($"{_config["IdentityService:scope"]}");
        Console.WriteLine("Requesting token from identity server...");

        var tokenResponse = await _apiservice.GetTokenAsync(tokenRequest);

        Console.WriteLine();
        Console.WriteLine("=== RESULT ===");

        if(tokenResponse == null)
        {
            return new AuthResponseDto
            {
                Status = "error",
                Message = "Token endpoint returned null"
            };
        }

        if(!string.IsNullOrEmpty(tokenResponse.AccessToken))
        {

            return new AuthResponseDto
            {
                Status ="success",
                Message ="Successfully authenticated profile ",
                Token = tokenResponse.AccessToken,
                Email = userdetails.Email,
                Roles = userdetails.Roles,
                UserId = userdetails.UserId,
                Username = userdetails.Username,
       
            };
            
        }

        return new AuthResponseDto
        {
            Status ="error",
            Message = tokenResponse.ErrorDescription?? "Token request failed"
        };

        /*if (tokenResponse != null)
        {
            if (!string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                //var profiledetails = await GetProfile(userlogin.Email.ToLower());

                response.Status = "success";
                response.Message = $"Successfully authenticated profile for {dto.Username}";
                response.Token = tokenResponse.AccessToken;
                //response.UserId = profiledetails.Id;
                //response._Guid = profiledetails._GUID;
                //response.Username = dto.Username;
                //response.RoleId = profiledetails.RoleId;

                Console.WriteLine("Token obtained successfully!");
                Console.WriteLine($"Access Token: {tokenResponse.AccessToken[..Math.Min(50, tokenResponse.AccessToken.Length)]}...");
                Console.WriteLine($"Token Type: {tokenResponse.TokenType}");
                Console.WriteLine($"Expires In: {tokenResponse.ExpiresIn} seconds");
                Console.WriteLine($"Scope: {tokenResponse.Scope}");


                //log the success authentication and token issued activity
                //await LogProfileLoginActivity(userlogin.Username, tokenResponse.AccessToken);

                if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
                {
                    Console.WriteLine($"Refresh Token: {tokenResponse.RefreshToken[..Math.Min(50, tokenResponse.RefreshToken.Length)]}...");
                }
            }
            else if (!string.IsNullOrEmpty(tokenResponse.Error))
            {
                Console.WriteLine($"Error: {tokenResponse.Error}");
                Console.WriteLine($"Description: {tokenResponse.ErrorDescription}");
                response.Message = $"Description: {tokenResponse.ErrorDescription}";
            }
        }
        else
        {
            response.Message = "✗ Failed to obtain token - null response.";
        }*/
        //return response;

    }

    public async Task<ResponseDto> RegisterUser(RegisterUserDto dto)
    {
        var response = new ResponseDto {Message = "An error occurred. We failed to register user", Status ="error"};
        if(dto == null) 
           return response;
        response = await _apiservice.RegisterUserAsync(dto);
        return response;
    }
    public async Task<ResponseDto> GetUser(string email)
    {
        var response = new ResponseDto {Message = "An error occurred. We failed to register user", 
        Status ="error"};
        if(email==null)
          return response;
        response.Status ="success";
        response.Message ="Fetch request successful";
        response.Payload = await _apiservice.GetUserDetails(email);
        return response;
    }
}