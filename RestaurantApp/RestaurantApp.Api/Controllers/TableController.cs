using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Shared.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.Models.DTOs;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Shared.DTOs;

namespace RestaurantApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableController : ControllerBase
    {
        private readonly ITableService _tableService;

        public TableController(ITableService tableService)
        {
            _tableService = tableService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTables()
            => (await _tableService.GetTablesAsync()).ToActionResult(this);

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTable(int id)
            => (await _tableService.GetTableByIdAsync(id)).ToActionResult(this);

        [HttpGet("restaurant/{restaurantId}")]
        public async Task<IActionResult> GetTablesByRestaurant(int restaurantId)
            => (await _tableService.GetTablesByRestaurantAsync(restaurantId)).ToActionResult(this);

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableTables([FromQuery] int? minCapacity = null)
            => (await _tableService.GetAvailableTablesAsync(minCapacity)).ToActionResult(this);

        [HttpPost]
        public async Task<IActionResult> CreateTable([FromBody] CreateTableDto dto)
            => (await _tableService.CreateTableAsync(dto)).ToActionResult(this);

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTable(int id, [FromBody] UpdateTableDto dto)
            => (await _tableService.UpdateTableAsync(id, dto)).ToActionResult(this);

        [HttpPatch("{id}/capacity")]
        public async Task<IActionResult> UpdateTableCapacity(int id, [FromBody] int capacity)
            => (await _tableService.UpdateTableCapacityAsync(id, capacity)).ToActionResult(this);

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTable(int id)
            => (await _tableService.DeleteTableAsync(id)).ToActionResult(this);
    }
}