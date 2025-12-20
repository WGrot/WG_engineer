using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RestaurantApp.Api.Common;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Shared.DTOs.Tables;

namespace RestaurantApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableController : ControllerBase
    {
        private readonly ITableService _tableService;
        private readonly ITableAvailabilityService _tableAvailabilityService;


        public TableController(ITableService tableService, ITableAvailabilityService tableAvailabilityService)
        {
            _tableService = tableService;

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
            return (await _tableService.CreateTableAsync(dto)).ToActionResult();
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTable(int id, [FromBody] UpdateTableDto dto)
        {
            return (await _tableService.UpdateTableAsync(id, dto)).ToActionResult();

        }

        [HttpPatch("{id}/capacity")]
        public async Task<IActionResult> UpdateTableCapacity(int id, [FromBody] int capacity)
        {
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
            return (await _tableService.DeleteTableAsync(id)).ToActionResult();
        }
        
        [HttpGet("{id}/availability-map")]
        public async Task<IActionResult> GetAvaibilityMap(int id, [FromQuery] DateTime date)
        {
            return (await _tableAvailabilityService.GetTableAvailabilityMapAsync(id, date)).ToActionResult();
        }
    }
}