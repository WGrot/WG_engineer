using Microsoft.AspNetCore.Authorization;

namespace RestaurantApp.Api.CustomHandlers.Authorization;

public class SameUserRequirement: IAuthorizationRequirement
{

    public string ParameterName { get; }
    
    public SameUserRequirement(string parameterName = "userId")
    {
        ParameterName = parameterName;
    }
}