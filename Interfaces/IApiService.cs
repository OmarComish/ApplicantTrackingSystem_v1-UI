using ATS.API.DTOs;

namespace ATS.API.Interfaces;
public interface IApiService
{
    Task<TokenResponseDto> GetTokenAsync(TokenRequestDto request);
    Task<ResponseDto> RegisterUserAsync(RegisterUserDto request);
    Task<AuthResponseDto> SignInAsync(LogInRequestDto request);
    Task<UserDetailsDto> GetUserDetails(string email);
}