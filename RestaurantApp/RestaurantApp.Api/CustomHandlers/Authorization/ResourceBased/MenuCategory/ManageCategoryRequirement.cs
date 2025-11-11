using Microsoft.AspNetCore.Authorization;

namespace RestaurantApp.Api.CustomHandlers.Authorization.ResourceBased.MenuCategory;

public class ManageCategoryRequirement: IAuthorizationRequirement
{
    public int CategoryId { get; }

    public ManageCategoryRequirement(int categoryId)
    {
        CategoryId = categoryId;
    }
}