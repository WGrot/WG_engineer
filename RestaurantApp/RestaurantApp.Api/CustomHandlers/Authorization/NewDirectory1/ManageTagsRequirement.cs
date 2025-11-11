using Microsoft.AspNetCore.Authorization;

namespace RestaurantApp.Api.CustomHandlers.Authorization.NewDirectory1;

public class ManageTagsRequirement: IAuthorizationRequirement
{
    public int TagId { get; }

    public ManageTagsRequirement(int categoryId)
    {
        TagId = categoryId;
    }
}