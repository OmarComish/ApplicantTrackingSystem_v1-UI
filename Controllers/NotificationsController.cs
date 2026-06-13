using ATS.API.Interfaces;
using ATS.API.Models;
using ATS.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace ATS.API.Controllers;
[ApiController]
[Route("api/[controller]")]
public class NotificationsController: ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly IApplicantService _applicantService;
    public NotificationsController(INotificationService notify, IApplicantService applicantservice)
    {
        _notificationService = notify;
        _applicantService = applicantservice;
    }
    [HttpGet("{applicationId}")]
    public async Task<ActionResult<Application>> SendNotification(int applicationId)
    {
    
           var application = await _applicantService.GetApplicationByIdAsync(applicationId);
           if(application == null)
             return NotFound($"Application with ID {applicationId} not found.");
           
           await _notificationService.SendApplicationConfirmationAsync(application);

           return Ok(application);
        
    }
}