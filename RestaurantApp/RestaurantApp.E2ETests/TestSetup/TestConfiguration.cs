namespace RestaurantApp.E2ETests.TestSetup;

public static class TestConfiguration
{
    public static string BaseUrl => 
        Environment.GetEnvironmentVariable("TEST_BASE_URL") ?? "http://localhost:3000";
    
    public static float DefaultTimeout => 
        float.TryParse(Environment.GetEnvironmentVariable("TEST_DEFAULT_TIMEOUT"), out var timeout) 
            ? timeout 
            : 30000;
    
    public static float NavigationTimeout => 
        float.TryParse(Environment.GetEnvironmentVariable("TEST_NAVIGATION_TIMEOUT"), out var timeout) 
            ? timeout 
            : 60000;

    public static bool RecordVideo => 
        bool.TryParse(Environment.GetEnvironmentVariable("TEST_RECORD_VIDEO"), out var record) && record;

    public static string Browser => 
        Environment.GetEnvironmentVariable("TEST_BROWSER") ?? "chromium";

    public static bool Headless => 
        !bool.TryParse(Environment.GetEnvironmentVariable("TEST_HEADLESS"), out var headless) || headless;
}