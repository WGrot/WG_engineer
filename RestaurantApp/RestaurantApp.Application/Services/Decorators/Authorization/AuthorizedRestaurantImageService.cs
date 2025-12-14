using RestaurantApp.Application.Interfaces;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Domain.Enums;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Images;

namespace RestaurantApp.Application.Services.Decorators.Authorization;

public class AuthorizedRestaurantImageService : IRestaurantImageService
{
    private readonly IRestaurantImageService _inner;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthorizationChecker _authorizationChecker;

    public AuthorizedRestaurantImageService(
        IRestaurantImageService inner,
        ICurrentUserService currentUser,
        IAuthorizationChecker authorizationChecker)
    {
        _inner = inner;
        _currentUser = currentUser;
        _authorizationChecker = authorizationChecker;
    }

    public async Task<Result<ImageUploadResult>> UploadProfilePhotoAsync(int restaurantId, Stream fileStream, string fileName)
    {
        if (!await AuthorizePermissionAsync(restaurantId))
            return Result<ImageUploadResult>.Forbidden("You dont have permission to edit this restaurant.");
        
        return await _inner.UploadProfilePhotoAsync(restaurantId, fileStream, fileName);
    }

    public async Task<Result<List<ImageUploadResult>>> UploadGalleryPhotosAsync(int restaurantId, IEnumerable<ImageFileDto> images)
    {
        if (!await AuthorizePermissionAsync(restaurantId))
            return Result<List<ImageUploadResult>>.Forbidden("You dont have permission to edit this restaurant.");
        
        return await _inner.UploadGalleryPhotosAsync(restaurantId, images);
    }

    public async Task<Result> DeleteProfilePhotoAsync(int restaurantId)
    {
        if (!await AuthorizePermissionAsync(restaurantId))
            return Result.Forbidden("You dont have permission to edit this restaurant.");
        
        return await _inner.DeleteProfilePhotoAsync(restaurantId);
    }

    public async Task<Result> DeleteGalleryPhotoAsync(int restaurantId, int photoIndex)
    {
        if (!await AuthorizePermissionAsync(restaurantId))
            return Result.Forbidden("You dont have permission to edit this restaurant.");
        
        return await _inner.DeleteGalleryPhotoAsync(restaurantId, photoIndex);
    }
    
    private async Task<bool> AuthorizePermissionAsync(int restaurantId)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrEmpty(userId))
            return false;

        return await _authorizationChecker.HasPermissionInRestaurantAsync(userId, restaurantId, PermissionType.ManageRestaurant);
    }
}