using ATS.API.DTOs;
using ATS.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATS.API.Controllers;
[ApiController]
[Route("api/[controller]/")]
public class AuthController: ControllerBase
{
    private readonly IAuthentication _authservice;
    public AuthController(IAuthentication authservice)
    {
        _authservice = authservice;
    }
    [HttpPost("Signin")]
    public async Task<ActionResult<AuthResponseDto>> SignIn([FromBody] LogInRequestDto request)
    {
        var response = new AuthResponseDto{Status ="error", Message = BadRequest("Invalid login details").ToString()};
        if(request != null) 
        {
             response = await _authservice.AuthenticateUser(request);
        }
       
        return Ok(response);
    }
    [HttpGet("GetUserDetails")]
    public async Task<IActionResult> UserDetails(string email)
    {
        var response = await _authservice.GetUser(email);
        return Ok(response);
    }
    //[Authorize(Policy = "ApiScope")]
    [HttpPost("registerUser")]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterUserDto dto)
    {
        var response = new ResponseDto {Status="error", Message=$"{BadRequest("Failed to register user")}"};
        if(!ModelState.IsValid)
           return Ok(response);
        response = await _authservice.RegisterUser(dto);
        return Ok(response);

    }
}