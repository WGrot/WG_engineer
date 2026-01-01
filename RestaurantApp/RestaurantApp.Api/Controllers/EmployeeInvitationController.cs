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
    public async Task<IActionResult> CreateInvitation([FromBody] CreateInvitationDto dto, CancellationToken ct)
    {
        var result = await _invitationService.CreateInvitationAsync(dto, ct);
        return result.ToActionResult();
    }


    [HttpPost("accept")]
    public async Task<IActionResult> AcceptInvitation([FromBody] TokenRequest request, CancellationToken ct)
    {
        var result = await _invitationService.AcceptInvitationAsync(request.Token, ct);

        return result.ToActionResult();
    }


    [HttpPost("reject")]
    public async Task<IActionResult> RejectInvitation([FromBody] TokenRequest request, CancellationToken ct)
    {
        var result = await _invitationService.RejectInvitationAsync(request.Token, ct);
        return result.ToActionResult();
    }
}