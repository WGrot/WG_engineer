using RestaurantApp.Api.Models.DTOs;
using RestaurantApp.Shared.Models;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Shared.Common;


namespace RestaurantApp.Api.Services;

public class RestaurantService : IRestaurantService
{
    private readonly ApiDbContext _context;
    private readonly ILogger<RestaurantService> _logger;
    private readonly IStorageService _storageService;

    public RestaurantService(ApiDbContext context, ILogger<RestaurantService> logger, IStorageService storageService)
    {
        _context = context;
        _logger = logger;
        _storageService = storageService;
    }

    public async Task<Result<IEnumerable<Restaurant>>> GetAllAsync()
    {
        var restaurants = await _context.Restaurants
            .Include(r => r.Menu)
            .ThenInclude(m => m.Items)
            .Include(r => r.OpeningHours)
            .ToListAsync();

        return Result<IEnumerable<Restaurant>>.Success(restaurants);
    }

    public async Task<Result<Restaurant>> GetByIdAsync(int id)
    {
        var restaurant = await _context.Restaurants
            .Include(r => r.Menu)
            .ThenInclude(m => m.Items)
            .Include(r => r.OpeningHours)
            .Include(r => r.Settings)
            .FirstOrDefaultAsync(r => r.Id == id);

        return restaurant == null
            ? Result<Restaurant>.NotFound("Restaurant not found")
            : Result<Restaurant>.Success(restaurant);
    }

    public async Task<Result<IEnumerable<Restaurant>>> SearchAsync(string? name, string? address)
    {
        var query = _context.Restaurants
            .Include(r => r.Menu)
            .Include(r => r.OpeningHours)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(r => r.Name.ToLower().Contains(name.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(address))
        {
            query = query.Where(r => r.Address.ToLower().Contains(address.ToLower()));
        }

        var restaurants = await query.ToListAsync();

        return Result<IEnumerable<Restaurant>>.Success(restaurants);
    }

    public async Task<Result<IEnumerable<Table>>> GetTablesAsync(int restaurantId)
    {
        var restaurantExists = await _context.Restaurants.AnyAsync(r => r.Id == restaurantId);
        if (!restaurantExists)
        {
            return Result<IEnumerable<Table>>.NotFound($"Cannot find restaurant with ID {restaurantId}");
        }

        var tables = await _context.Tables
            .Where(t => t.RestaurantId == restaurantId)
            .Include(t => t.Seats)
            .ToListAsync();

        return Result<IEnumerable<Table>>.Success(tables);
    }

    public async Task<Result<IEnumerable<Restaurant>>> GetOpenNowAsync()
    {
        var now = TimeOnly.FromDateTime(DateTime.Now);
        var currentDay = DateTime.Now.DayOfWeek;

        // Optymalizacja - filtrowanie na poziomie bazy danych
        var openRestaurants = await _context.Restaurants
            .Include(r => r.OpeningHours)
            .Include(r => r.Menu)
            .Where(r => r.OpeningHours.Any(oh =>
                oh.DayOfWeek == currentDay &&
                oh.OpenTime <= now &&
                oh.CloseTime >= now &&
                !oh.IsClosed))
            .ToListAsync();

        return Result<IEnumerable<Restaurant>>.Success(openRestaurants);
    }

    public async Task<Result<OpenStatusDto>> CheckIfOpenAsync(int restaurantId, TimeOnly? time = null,
        DayOfWeek? dayOfWeek = null)
    {
        var restaurant = await _context.Restaurants
            .Include(r => r.OpeningHours)
            .FirstOrDefaultAsync(r => r.Id == restaurantId);

        if (restaurant == null)
        {
            throw new KeyNotFoundException($"Restaurant with ID {restaurantId} not found.");
        }

        var checkTime = time ?? TimeOnly.FromDateTime(DateTime.Now);
        var checkDay = dayOfWeek ?? DateTime.Now.DayOfWeek;

        var openingHours = restaurant.OpeningHours?.FirstOrDefault(oh => oh.DayOfWeek == checkDay);

        if (openingHours == null)
        {
            OpenStatusDto status = new OpenStatusDto
            {
                IsOpen = false,
                Message = "No opening hours defined for this day",
                DayOfWeek = checkDay.ToString(),
                CheckedTime = checkTime.ToString("HH:mm")
            };
            return Result<OpenStatusDto>.Success(status);
        }

        return Result<OpenStatusDto>.Success(
            new OpenStatusDto
            {
                IsOpen = openingHours.IsOpenAt(checkTime),
                DayOfWeek = checkDay.ToString(),
                CheckedTime = checkTime.ToString("HH:mm"),
                OpenTime = openingHours.OpenTime.ToString("HH:mm"),
                CloseTime = openingHours.CloseTime.ToString("HH:mm"),
                IsClosed = openingHours.IsClosed
            }
        );
    }

    public async Task<Result<Restaurant>> CreateAsync(RestaurantDto restaurantDto)
    {
        _logger.LogInformation("Creating new restaurant: {RestaurantName}", restaurantDto.Name);

        Result validationResult = await ValidateRestaurantUniquenessAsync(restaurantDto.Name, restaurantDto.Address);
        // Walidacja biznesowa
        if(validationResult.IsFailure)
        {
            return Result<Restaurant>.Failure("A restaurant with the same name and address already exists.");
        }

        var restaurant = new Restaurant
        {
            Name = restaurantDto.Name,
            Address = restaurantDto.Address
        };

        // Add opening hours if provided
        if (restaurantDto.OpeningHours?.Any() == true)
        {
            restaurant.OpeningHours = MapOpeningHours(restaurantDto.OpeningHours);
        }

        _context.Restaurants.Add(restaurant);
        await _context.SaveChangesAsync();

        return Result<Restaurant>.Success(restaurant);
    }

    public async Task<Result> UpdateAsync(int id, RestaurantDto restaurantDto)
    {
        var existingRestaurant = await _context.Restaurants
            .Include(r => r.OpeningHours)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (existingRestaurant == null)
        {
            return Result.NotFound($"Restaurant with ID {id} not found.");
        }

        // Walidacja biznesowa
        if (existingRestaurant.Name != restaurantDto.Name || existingRestaurant.Address != restaurantDto.Address)
        {
            Result validationResult = await ValidateRestaurantUniquenessAsync(restaurantDto.Name, restaurantDto.Address);
            
            if(validationResult.IsFailure)
            {
                return Result<Restaurant>.Failure("A restaurant with the same name and address already exists.");
            }
        }

        // Update basic properties
        existingRestaurant.Name = restaurantDto.Name;
        existingRestaurant.Address = restaurantDto.Address;

        // Update opening hours if provided
        if (restaurantDto.OpeningHours != null)
        {
            await UpdateRestaurantOpeningHoursAsync(existingRestaurant, restaurantDto.OpeningHours);
        }

        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> UpdateAddressAsync(int id, string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return Result.Failure("Address cannot be empty");
        }

        var restaurant = await _context.Restaurants.FindAsync(id);
        if (restaurant == null)
        {
            return Result.NotFound($"Restaurant with ID {id} not found.");;
        }

        restaurant.Address = address;
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> UpdateNameAsync(int id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure("Name cannot be empty");
        }

        var restaurant = await _context.Restaurants.FindAsync(id);
        if (restaurant == null)
        {
            return Result.NotFound($"Restaurant with ID {id} not found.");
        }

        restaurant.Name = name;
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> UpdateOpeningHoursAsync(int id, List<OpeningHoursDto> openingHours)
    {
        var restaurant = await _context.Restaurants
            .Include(r => r.OpeningHours)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (restaurant == null)
        {
            return Result.NotFound($"Restaurant with ID {id} not found.");
        }

        await UpdateRestaurantOpeningHoursAsync(restaurant, openingHours);
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var restaurant = await _context.Restaurants
            .Include(r => r.Menu)
            .Include(r => r.OpeningHours)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (restaurant == null)
        {
            return Result.NotFound($"Restaurant with ID {id} not found.");
        }

        // Walidacja biznesowa przed usunięciem
        var validationResult = await ValidateRestaurantDeletionAsync(id, restaurant);
        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        _context.Restaurants.Remove(restaurant);
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    // ========== Metody pomocnicze (prywatne) ==========

    private async Task<Result> ValidateRestaurantUniquenessAsync(string name, string address, int? excludeId = null)
    {
        var query = _context.Restaurants
            .Where(r => r.Name == name && r.Address == address);

        if (excludeId.HasValue)
        {
            query = query.Where(r => r.Id != excludeId.Value);
        }

        if (await query.AnyAsync())
        {
            return Result.Failure($"Restaurant with name '{name}' and address '{address}' already exists.");
        }
        return Result.Success();
    }

    private async Task<Result> ValidateRestaurantDeletionAsync(int id, Restaurant restaurant)
    {
        // Check if restaurant has any tables
        var hasTables = await _context.Tables.AnyAsync(t => t.RestaurantId == id);
        if (hasTables)
        {
            return Result.Failure($"Cannot delete restaurant with tables.");
        }

        // Check if restaurant has active menu items
        if (restaurant.Menu != null)
        {
            var hasActiveMenu = await _context.Menus
                .Where(m => m.RestaurantId == id && m.IsActive)
                .AnyAsync();

            if (hasActiveMenu)
            {
                return Result.Failure($"Cannot delete restaurant with active menu.");
            }
        }

        return Result.Success();
    }

    private async Task UpdateRestaurantOpeningHoursAsync(Restaurant restaurant, List<OpeningHoursDto> newHours)
    {
        // Remove existing opening hours
        if (restaurant.OpeningHours != null)
        {
            _context.OpeningHours.RemoveRange(restaurant.OpeningHours);
        }

        // Add new opening hours
        restaurant.OpeningHours = MapOpeningHours(newHours, restaurant.Id);

        // Explicit tracking for EF Core
        await Task.CompletedTask;
    }
    
    public async Task<Result<ImageUploadResult>> UploadRestaurantProfilePhoto(IFormFile file, int restaurantId)
    {
        try
        {
            // Sprawdź uprawnienia i pobierz restaurację
            var restaurant = await _context.Restaurants
                .Include(r => r.Settings)
                .FirstOrDefaultAsync(r => r.Id == restaurantId);
            if (restaurant == null)
            {
                return Result<ImageUploadResult>.NotFound("Restaurant not found");
            }
            
            // Usuń stare logo jeśli istnieje
            if (!string.IsNullOrEmpty(restaurant.profileUrl))
            {
                await DeleteOldImage(restaurant.profileUrl, "logo");
            }

            // Upload nowego logo
            var stream = file.OpenReadStream();
            var uploadResult = await _storageService.UploadImageAsync(
                stream,
                file.FileName,
                ImageType.RestaurantProfile,
                restaurantId,
                generateThumbnail: true
            );

            // Zaktualizuj URL w bazie danych
            restaurant.profileUrl = uploadResult.OriginalUrl;
            restaurant.profileThumbnailUrl = uploadResult.ThumbnailUrl;

            
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Logo uploaded successfully for restaurant {restaurantId}");

            return Result<ImageUploadResult>.Success(uploadResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error uploading logo for restaurant {restaurantId}");
            return Result<ImageUploadResult>.Failure("An error occurred while uploading the logo.");
        }
    }

    public async Task<Result<List<ImageUploadResult>>> UploadRestaurantPhotos(List<IFormFile> imageList, int id)
    {
        try
        {
            List <ImageUploadResult> uploadResults = new List<ImageUploadResult>();
            // Sprawdź uprawnienia i pobierz restaurację
            var restaurant = await _context.Restaurants
                .Include(r => r.Settings)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (restaurant == null)
            {
                return Result<List<ImageUploadResult>>.NotFound("Restaurant not found");
            }

            if (restaurant.photosUrls == null)
            {
                restaurant.photosUrls = new List<string>();
            }
            if (restaurant.photosThumbnailsUrls == null)
            {
                restaurant.photosThumbnailsUrls = new List<string>();
            }

            foreach(var image in imageList)
            {
                // Upload nowego logo
                var stream = image.OpenReadStream();
                var uploadResult = await _storageService.UploadImageAsync(
                    stream,
                    image.FileName,
                    ImageType.RestaurantPhotos,
                    id,
                    generateThumbnail: true
                );
                restaurant.photosUrls.Add(uploadResult.OriginalUrl);
                restaurant.photosThumbnailsUrls.Add(uploadResult.ThumbnailUrl);
                uploadResults.Add(uploadResult);
            }
            
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Logo uploaded successfully for restaurant {id}");

            return Result<List<ImageUploadResult>>.Success(uploadResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error uploading logo for restaurant {id}");
            return Result<List<ImageUploadResult>>.Failure("An error occurred while uploading the logo.");
        }
    }


    public async Task<Result> DeleteRestaurantImage(int restaurantId, string imageUrl, ImageType imageType)
    {
        try
        {
            var restaurant = await _context.Restaurants
                .Include(r => r.Settings)
                .FirstOrDefaultAsync(r => r.Id == restaurantId);
            if (restaurant == null)
            {
                return Result.Failure("Restaurant not found.");
            }

            // Usuń z storage
            string bucketName = "images";
            var deleted = await _storageService.DeleteImageWithThumbnailAsync(imageUrl, bucketName);
            
            if (!deleted)
            {
                return Result.Failure("Failed to delete image from storage.");
            }

            // Zaktualizuj bazę danych w zależności od typu
            switch (imageType)
            {
                case ImageType.RestaurantProfile:
                    restaurant.profileUrl = null;
                    restaurant.profileThumbnailUrl = null;
                    break;
            }
            
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Image deleted successfully for restaurant {restaurantId}");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting image for restaurant {restaurantId}");
            return Result.Failure("An error occurred while deleting the image.");
        }
    }
    
    private async Task DeleteOldImage(string imageUrl, string imageType)
    {
        try
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            var bucketName = "images";
            await _storageService.DeleteImageWithThumbnailAsync(imageUrl, bucketName);
            
            _logger.LogInformation($"Deleted old {imageType}: {imageUrl}");
        }
        catch (Exception ex)
        {
            // Nie przerywaj procesu jeśli usunięcie starego zdjęcia się nie powiodło
            _logger.LogWarning(ex, $"Failed to delete old {imageType}: {imageUrl}");
        }
    }

    private List<OpeningHours> MapOpeningHours(List<OpeningHoursDto> dtos, int? restaurantId = null)
    {
        return dtos.Select(oh => new OpeningHours
        {
            DayOfWeek = (DayOfWeek)oh.DayOfWeek,
            OpenTime = TimeOnly.Parse(oh.OpenTime),
            CloseTime = TimeOnly.Parse(oh.CloseTime),
            IsClosed = oh.IsClosed,
            RestaurantId = restaurantId ?? 0
        }).ToList();
    }
}