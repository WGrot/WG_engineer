using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Mappers;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Menu.Tags;

namespace RestaurantApp.Application.Services;


public class MenuItemTagService : IMenuItemTagService
{
    private readonly IMenuItemTagRepository _tagRepository;

    public MenuItemTagService(IMenuItemTagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<Result<IEnumerable<MenuItemTagDto>>> GetTagsAsync(CancellationToken ct, int? restaurantId = null)
    {
        var tags = await _tagRepository.GetAllAsync(ct, restaurantId);
        return Result<IEnumerable<MenuItemTagDto>>.Success(tags.ToDto());
    }

    public async Task<Result<MenuItemTagDto?>> GetTagByIdAsync(int id, CancellationToken ct)
    {
        var tag = await _tagRepository.GetByIdAsync(id, ct);
        return Result<MenuItemTagDto?>.Success(tag!.ToDto());
    }

    public async Task<Result<MenuItemTagDto>> CreateTagAsync(CreateMenuItemTagDto dto, CancellationToken ct)
    {
        var newTag = dto.ToEntity();

        await _tagRepository.AddAsync(newTag, ct);
        await _tagRepository.SaveChangesAsync();

        return Result<MenuItemTagDto>.Success(newTag.ToDto());
    }

    public async Task<Result<MenuItemTagDto>> UpdateTagAsync(int id, MenuItemTagDto dto, CancellationToken ct)
    {
        var existingTag = await _tagRepository.GetByIdAsync(id, ct);

        existingTag!.UpdateFromDto(dto);

        _tagRepository.Update(existingTag!, ct);
        await _tagRepository.SaveChangesAsync();

        return Result<MenuItemTagDto>.Success(existingTag!.ToDto());
    }

    public async Task<Result> DeleteTagAsync(int id, CancellationToken ct)
    {
        var tag = await _tagRepository.GetByIdAsync(id, ct);

        _tagRepository.Delete(tag!, ct);
        await _tagRepository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<bool>> TagExistsAsync(int id, CancellationToken ct)
    {
        var exists = await _tagRepository.ExistsAsync(id, ct);
        return Result<bool>.Success(exists);
    }
}