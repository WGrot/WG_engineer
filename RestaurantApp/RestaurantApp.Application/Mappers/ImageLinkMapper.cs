using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.DTOs.Images;

namespace RestaurantApp.Application.Mappers;

public static class ImageLinkMapper
{
    public static ImageLinkDto ToDto(this ImageLink imageLink)
    {
        return new ImageLinkDto
        {
            Id = imageLink.Id,
            Url = imageLink.Url,
            ThumbnailUrl = imageLink.ThumbnailUrl,
            SortOrder = imageLink.SortOrder,
            OriginalFileName = imageLink.OriginalFileName
        };

    }
    
    public static List<ImageLinkDto> ToDtoList(this IEnumerable<ImageLink> entities)
    {
        return entities.Select(e => e.ToDto()).ToList();
    }
}