using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Mappers;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.OpeningHours;

namespace RestaurantApp.Application.Services;

public class RestaurantOpeningHoursService : IRestaurantOpeningHoursService
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IOpeningHoursRepository _openingHoursRepository;

    public RestaurantOpeningHoursService(
        IRestaurantRepository restaurantRepository,
        IOpeningHoursRepository openingHoursRepository)
    {
        _restaurantRepository = restaurantRepository;
        _openingHoursRepository = openingHoursRepository;
    }

    public async Task<Result<OpenStatusDto>> CheckIfOpenAsync(
        int restaurantId,
        TimeOnly? time = null,
        DayOfWeek? dayOfWeek = null)
    {
        var restaurant = await _restaurantRepository.GetByIdWithDetailsAsync(restaurantId);

        if (restaurant == null)
            return Result<OpenStatusDto>.NotFound($"Restaurant with ID {restaurantId} not found.");

        var checkTime = time ?? TimeOnly.FromDateTime(DateTime.Now);
        var checkDay = dayOfWeek ?? DateTime.Now.DayOfWeek;

        var openingHours = restaurant.OpeningHours?.FirstOrDefault(oh => oh.DayOfWeek == checkDay);

        if (openingHours == null)
        {
            return Result<OpenStatusDto>.Success(new OpenStatusDto
            {
                IsOpen = false,
                Message = "No opening hours defined for this day",
                DayOfWeek = checkDay.ToString(),
                CheckedTime = checkTime.ToString("HH:mm")
            });
        }

        return Result<OpenStatusDto>.Success(new OpenStatusDto
        {
            IsOpen = openingHours.IsOpenAt(checkTime),
            DayOfWeek = checkDay.ToString(),
            CheckedTime = checkTime.ToString("HH:mm"),
            OpenTime = openingHours.OpenTime.ToString("HH:mm"),
            CloseTime = openingHours.CloseTime.ToString("HH:mm"),
            IsClosed = openingHours.IsClosed
        });
    }

    public async Task<Result> UpdateOpeningHoursAsync(int id, List<OpeningHoursDto> openingHours)
    {
        var restaurant = await _restaurantRepository.GetByIdWithDetailsAsync(id);

        if (restaurant == null)
            return Result.NotFound($"Restaurant with ID {id} not found.");

        if (restaurant.OpeningHours != null)
        {
            _openingHoursRepository.RemoveRange(restaurant.OpeningHours);
        }

        restaurant.OpeningHours = openingHours.ToEntityList();
        
        _restaurantRepository.Update(restaurant);
        await _restaurantRepository.SaveChangesAsync();

        return Result.Success();
    }
}