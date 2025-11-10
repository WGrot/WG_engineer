using Microsoft.AspNetCore.Authorization;

namespace RestaurantApp.Api.CustomHandlers.Authorization.NewDirectory1;

public class ManageCategoryRequirement: IAuthorizationRequirement
{
    public int CategoryId { get; }

    public ManageCategoryRequirement(int categoryId)
    {
        CategoryId = categoryId;
    }
}