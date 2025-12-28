using OtpNet;

namespace RestaurantApp.E2ETests.Helpers;

public static class TestDataFactory
{
    private static readonly Random Random = new();

    





    #region User Generation
    
    public static TestUser GenerateUser()
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var randomSuffix = Random.Next(1000, 9999);
        
        var password = $"TestPass{randomSuffix}!";
        
        return new TestUser
        {
            FirstName = "Test",
            LastName = "User",
            Email = $"testuser@example.com",
            Password = password,
            ConfirmPassword = password
        };
    }
    
    public static TestUser GenerateUser(string firstName, string lastName)
    {
        var user = GenerateUser();
        user.FirstName = firstName;
        user.LastName = lastName;
        return user;
    }

    #endregion

    #region Predefined Test Users
    
    public static UserCredentials GetValidUserCredentials()
    {

        var email = "jan@kowalski.com";
        var password = "123123123";

        if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
        {
            return new UserCredentials { Email = email, Password = password };
        }
        
        return new UserCredentials
        {
            Email = email,
            Password = password
        };
    }
    
    public static UserCredentials GetMultiRestaurantEmployeeCredentials()
    {
        var email = "jan@kowalski.com";
        var password = "123123123";

        if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
        {
            return new UserCredentials { Email = email, Password = password };
        }
        
        return new UserCredentials
        {
            Email = email,
            Password = password
        };
    }
    
    
    public static UserCredentials GetSingleRestaurantEmployeeCredentials()
    {
        var email = "jan@nowak.com";
        var password = "123123123";

        if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
        {
            return new UserCredentials { Email = email, Password = password };
        }
        
        return new UserCredentials
        {
            Email = email,
            Password = password
        };
    }


    #endregion

    #region TOTP Generation

    public static UserCredentials GetUserWith2FACredentials()
    {
        var email = "2fa@2fa.com";
        var password = "123123123";
        var totpSecret = "2JNP4PTFM6IL2HNDCEJEXHSA2WHWWQKC";

        if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
        {
            return new UserCredentials 
            { 
                Email = email, 
                Password = password,
                TotpSecret = totpSecret
            };
        }

        // Default 2FA test user
        return new UserCredentials
        {
            Email = "test2fa@example.com",
            Password = "TestPassword123!",
            TotpSecret = "JBSWY3DPEHPK3PXP" // Example base32 secret
        };
    }
    
    
    public static string GenerateValidTotpCode(string? base32Secret)
    {
        if (string.IsNullOrEmpty(base32Secret))
        {
            throw new ArgumentException("TOTP secret is required", nameof(base32Secret));
        }

        var secretBytes = Base32Encoding.ToBytes(base32Secret);
        var totp = new Totp(secretBytes);
        return totp.ComputeTotp();
    }

    #endregion
    
    
    public static TestRestaurant GenerateRestaurant()
    {
        return new TestRestaurant
        {
            Name = $"Test Restaurant {Guid.NewGuid().ToString("N")[..8]}",
            Street = "Nowy Świat 0",
            City = "Warszawa",
            PostalCode = "00-046",
            Country = "Polska"
        };
    }
}


public static class TestHelpers
{
    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
    
    public static string RandomEmail()
    {
        return $"{RandomString(10)}@example.com";
    }
}