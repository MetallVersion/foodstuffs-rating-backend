using System;
using System.Security.Cryptography;

namespace FoodstuffsRating.Services.Auth
{
    public interface IRefreshTokenProvider
    {
        string GenerateRefreshToken();
    }

    public class RefreshTokenProvider : IRefreshTokenProvider
    {
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[256];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            return Convert.ToBase64String(randomNumber);
        }
    }
}