using System.Net.Http.Json;
using RestaurantApp.Blazor.Services.Interfaces;
using RestaurantApp.Shared.DTOs.Restaurant;

namespace RestaurantApp.Blazor.Services;

public class RestaurantServie: IRestaurantService
{
    private readonly HttpClient Http;
    private readonly JwtTokenParser _jwtTokenParser;
    
    public RestaurantServie(JwtTokenParser jwtTokenParser, HttpClient http)
    {
        _jwtTokenParser = jwtTokenParser;
        Http = http;
    }
    
    
    public async Task<List<(int Id, string Name)>> GetRestaurantNames()
    {
        string querryString = "";
        var idList = await _jwtTokenParser.GetAllUserRestaurantIds();
        foreach (var id in idList)
        {
            querryString = querryString + "ids=" + id + "&";
        }
        
        List<RestaurantDto> responseData= new();
        responseData = await Http.GetFromJsonAsync<List<RestaurantDto>>($"api/restaurant/names?{querryString}");
        
        List<(int Id, string Name)> userRestaurantNames = new();
        
        foreach (var restaurant in responseData)
        {
            userRestaurantNames.Add((restaurant.Id, restaurant.Name ?? ""));
        }
        
        return userRestaurantNames;
    }
}