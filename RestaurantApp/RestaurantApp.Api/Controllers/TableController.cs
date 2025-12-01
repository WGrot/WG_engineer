using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Shared.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.CustomHandlers.Authorization.ResourceBased.Table;

using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Tables;

namespace RestaurantApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableController : ControllerBase
    {
        private readonly ITableService _tableService;
        private readonly ITableAvailabilityService _tableAvailabilityService;
        private readonly IAuthorizationService _authorizationService;

        public TableController(ITableService tableService, IAuthorizationService authorizationService, ITableAvailabilityService tableAvailabilityService)
        {
            _tableService = tableService;
            _authorizationService = authorizationService;
            _tableAvailabilityService = tableAvailabilityService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTables()
            => (await _tableService.GetAllTablesAsync()).ToActionResult();

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTable(int id)
            => (await _tableService.GetTableByIdAsync(id)).ToActionResult();

        [HttpGet("restaurant/{restaurantId}")]
        public async Task<IActionResult> GetTablesByRestaurant(int restaurantId)
            => (await _tableService.GetTablesByRestaurantAsync(restaurantId)).ToActionResult();

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableTables([FromQuery] int? minCapacity = null)
            => (await _tableService.GetAvailableTablesAsync(minCapacity)).ToActionResult();

        [HttpPost]
        public async Task<IActionResult> CreateTable([FromBody] CreateTableDto dto)
        {
            var authResult = await _authorizationService.AuthorizeAsync(
                User, 
                null,
                new ManageTableRequirement(restaurantId: dto.RestaurantId)); 

            if (!authResult.Succeeded)
                return Forbid();
            return (await _tableService.CreateTableAsync(dto)).ToActionResult();
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTable(int id, [FromBody] UpdateTableDto dto)
        {
            var authResult = await _authorizationService.AuthorizeAsync(
                User, 
                null,
                new ManageTableRequirement(id)); 

            if (!authResult.Succeeded)
                return Forbid();
            return (await _tableService.UpdateTableAsync(id, dto)).ToActionResult();

        }

        [HttpPatch("{id}/capacity")]
        public async Task<IActionResult> UpdateTableCapacity(int id, [FromBody] int capacity)
        {
            var authResult = await _authorizationService.AuthorizeAsync(
                User, 
                null,
                new ManageTableRequirement(id)); 

            if (!authResult.Succeeded)
                return Forbid();
            return (await _tableService.UpdateTableCapacityAsync(id, capacity)).ToActionResult();
        }

        [HttpGet("{id}/check-availability")]
        public async Task<IActionResult> CheckTableAvailability(
            int id,
            [FromQuery] DateTime date,
            [FromQuery] TimeOnly startTime,
            [FromQuery] TimeOnly endTime)
        {
            return (await _tableAvailabilityService.CheckTableAvailabilityAsync(id, date, startTime, endTime))
                .ToActionResult();
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTable(int id)
        {
            var authResult = await _authorizationService.AuthorizeAsync(
                User, 
                null,
                new ManageTableRequirement(id)); 

            if (!authResult.Succeeded)
                return Forbid();
            return (await _tableService.DeleteTableAsync(id)).ToActionResult();
        }
        
        [HttpGet("{id}/availability-map")]
        public async Task<IActionResult> GetAvaibilityMap(int id, [FromQuery] DateTime date)
        {
            return (await _tableAvailabilityService.GetTableAvailabilityMapAsync(id, date)).ToActionResult();
        }
    }
}