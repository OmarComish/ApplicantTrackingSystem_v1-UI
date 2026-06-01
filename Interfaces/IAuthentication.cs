using ATS.API.DTOs;

namespace ATS.API.Interfaces;
public interface IAuthentication
{
    Task<AuthResponseDto> AuthenticateUser(LogInRequestDto dto);
    Task<ResponseDto> RegisterUser(RegisterUserDto dto);
    Task<ResponseDto> GetUser(string email);
}