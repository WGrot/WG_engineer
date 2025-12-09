using Microsoft.Extensions.Logging;
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

    public async Task<Result<IEnumerable<MenuItemTagDto>>> GetTagsAsync(int? restaurantId = null)
    {
        var tags = await _tagRepository.GetAllAsync(restaurantId);
        return Result<IEnumerable<MenuItemTagDto>>.Success(tags.ToDto());
    }

    public async Task<Result<MenuItemTagDto?>> GetTagByIdAsync(int id)
    {
        var tag = await _tagRepository.GetByIdAsync(id);
        return Result<MenuItemTagDto?>.Success(tag!.ToDto());
    }

    public async Task<Result<MenuItemTagDto>> CreateTagAsync(CreateMenuItemTagDto dto)
    {
        var newTag = dto.ToEntity();

        await _tagRepository.AddAsync(newTag);
        await _tagRepository.SaveChangesAsync();

        return Result<MenuItemTagDto>.Success(newTag.ToDto());
    }

    public async Task<Result<MenuItemTagDto>> UpdateTagAsync(int id, MenuItemTagDto dto)
    {
        var existingTag = await _tagRepository.GetByIdAsync(id);

        existingTag!.UpdateFromDto(dto);

        _tagRepository.Update(existingTag);
        await _tagRepository.SaveChangesAsync();

        return Result<MenuItemTagDto>.Success(existingTag.ToDto());
    }

    public async Task<Result> DeleteTagAsync(int id)
    {
        var tag = await _tagRepository.GetByIdAsync(id);

        _tagRepository.Delete(tag!);
        await _tagRepository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<bool>> TagExistsAsync(int id)
    {
        var exists = await _tagRepository.ExistsAsync(id);
        return Result<bool>.Success(exists);
    }
}