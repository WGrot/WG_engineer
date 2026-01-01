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

    public async Task<Result<ImageUploadResult>> UploadProfilePhotoAsync(int restaurantId, Stream fileStream, string fileName, CancellationToken ct)
    {
        if (!await AuthorizePermissionAsync(restaurantId, ct))
            return Result<ImageUploadResult>.Forbidden("You dont have permission to edit this restaurant.");
        
        return await _inner.UploadProfilePhotoAsync(restaurantId, fileStream, fileName, ct);
    }

    public async Task<Result<List<ImageUploadResult>>> UploadGalleryPhotosAsync(int restaurantId, IEnumerable<ImageFileDto> images, CancellationToken ct)
    {
        if (!await AuthorizePermissionAsync(restaurantId, ct))
            return Result<List<ImageUploadResult>>.Forbidden("You dont have permission to edit this restaurant.");
        
        return await _inner.UploadGalleryPhotosAsync(restaurantId, images, ct);
    }

    public async Task<Result> DeleteProfilePhotoAsync(int restaurantId, CancellationToken ct)
    {
        if (!await AuthorizePermissionAsync(restaurantId, ct))
            return Result.Forbidden("You dont have permission to edit this restaurant.");
        
        return await _inner.DeleteProfilePhotoAsync(restaurantId, ct);
    }

    public async Task<Result> DeleteGalleryPhotoAsync(int restaurantId, int photoIndex, CancellationToken ct)
    {
        if (!await AuthorizePermissionAsync(restaurantId, ct))
            return Result.Forbidden("You dont have permission to edit this restaurant.");
        
        return await _inner.DeleteGalleryPhotoAsync(restaurantId, photoIndex, ct);
    }
    
    private async Task<bool> AuthorizePermissionAsync(int restaurantId, CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrEmpty(userId))
            return false;

        return await _authorizationChecker.HasPermissionInRestaurantAsync(userId, restaurantId, PermissionType.ManageRestaurant);
    }
}