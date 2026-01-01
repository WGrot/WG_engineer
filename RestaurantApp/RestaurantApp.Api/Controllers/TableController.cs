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
        public async Task<IActionResult> GetTables(CancellationToken ct)
            => (await _tableService.GetAllTablesAsync(ct)).ToActionResult();

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTable(int id, CancellationToken ct)
            => (await _tableService.GetTableByIdAsync(id, ct)).ToActionResult();

        [HttpGet("restaurant/{restaurantId}")]
        public async Task<IActionResult> GetTablesByRestaurant(int restaurantId, CancellationToken ct)
            => (await _tableService.GetTablesByRestaurantAsync(restaurantId, ct)).ToActionResult();

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableTables(CancellationToken ct, [FromQuery] int? minCapacity = null)
            => (await _tableService.GetAvailableTablesAsync(minCapacity, ct)).ToActionResult();

        [HttpPost]
        public async Task<IActionResult> CreateTable([FromBody] CreateTableDto dto, CancellationToken ct)
        {
            return (await _tableService.CreateTableAsync(dto, ct)).ToActionResult();
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTable(int id, [FromBody] UpdateTableDto dto, CancellationToken ct)
        {
            return (await _tableService.UpdateTableAsync(id, dto, ct)).ToActionResult();

        }

        [HttpPatch("{id}/capacity")]
        public async Task<IActionResult> UpdateTableCapacity(int id, [FromBody] int capacity, CancellationToken ct)
        {
            return (await _tableService.UpdateTableCapacityAsync(id, capacity, ct)).ToActionResult();
        }

        [HttpGet("{id}/check-availability")]
        public async Task<IActionResult> CheckTableAvailability(
            int id,
            CancellationToken ct,
            [FromQuery] DateTime date,
            [FromQuery] TimeOnly startTime,
            [FromQuery] TimeOnly endTime)
        {
            return (await _tableAvailabilityService.CheckTableAvailabilityAsync(id, date, startTime, endTime, ct))
                .ToActionResult();
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTable(int id, CancellationToken ct)
        {
            return (await _tableService.DeleteTableAsync(id, ct)).ToActionResult();
        }
        
        [HttpGet("{id}/availability-map")]
        public async Task<IActionResult> GetAvaibilityMap(int id, [FromQuery] DateTime date, CancellationToken ct)
        {
            return (await _tableAvailabilityService.GetTableAvailabilityMapAsync(id, date, ct)).ToActionResult();
        }
    }
}