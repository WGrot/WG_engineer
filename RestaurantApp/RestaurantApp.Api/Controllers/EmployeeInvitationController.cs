using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Shared.DTOs.Employees;

namespace RestaurantApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeeInvitationsController : ControllerBase
{
    private readonly IEmployeeInvitationService _invitationService;

    public EmployeeInvitationsController(IEmployeeInvitationService invitationService)
    {
        _invitationService = invitationService;
    }


    [HttpPost]
    public async Task<IActionResult> CreateInvitation([FromBody] CreateInvitationDto dto)
    {
        var result = await _invitationService.CreateInvitationAsync(dto);
        return result.ToActionResult();
    }


    [HttpDelete("{id:int}")]
    public async Task<IActionResult> CancelInvitation(int id)
    {
        return (await _invitationService.CancelInvitationAsync(id)).ToActionResult();
    }


    [HttpPost("accept")]
    public async Task<IActionResult> AcceptInvitation([FromBody] TokenRequest request)
    {
        var result = await _invitationService.AcceptInvitationAsync(request.Token);

        return result.ToActionResult();
    }


    [HttpPost("reject")]
    public async Task<IActionResult> RejectInvitation([FromBody] TokenRequest request)
    {
        var result = await _invitationService.RejectInvitationAsync(request.Token);
        return result.ToActionResult();
    }
}