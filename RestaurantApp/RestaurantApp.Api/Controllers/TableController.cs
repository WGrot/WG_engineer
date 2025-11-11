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
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Tables;

namespace RestaurantApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableController : ControllerBase
    {
        private readonly ITableService _tableService;
        private readonly IAuthorizationService _authorizationService;

        public TableController(ITableService tableService, IAuthorizationService authorizationService)
        {
            _tableService = tableService;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTables()
            => (await _tableService.GetTablesAsync()).ToActionResult();

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
            return (await _tableService.GetTableAvailabilityMapAsync(id, date)).ToActionResult();
        }
    }
}