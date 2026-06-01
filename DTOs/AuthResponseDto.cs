
namespace ATS.API.DTOs;
public class AuthResponseDto
{
    public string Status { get; set; }
    public string Message   { get; set; }
    public string UserId    { get; set; }
    public string Username  { get; set; }
    public string Email     { get; set; }
    public string  Token { get; set; }
    public IList<string> Roles { get; set; }
   
}