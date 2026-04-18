using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PeakWise.Application.Interfaces;
using StackExchange.Redis;

namespace PeakWise.Application.Features
{
    public class OtpService : IOtpService
    {
        private readonly IDatabase _redis;
        private readonly ILogger<OtpService> _logger;

        public OtpService(IConnectionMultiplexer redis, ILogger<OtpService> logger)
        {
            _redis = redis.GetDatabase();
            _logger = logger;
        }

        public async Task<string> GenerateAndStoreOtpAsync(string userId)
        {
            var otp = GenerateOtp();

            // Use a short expiration appropriate for OTPs (example: 5 minutes)
            var expiry = TimeSpan.FromMinutes(5);
            bool success = await _redis.StringSetAsync($"otp:{userId}", otp, expiry);
            if (success)
                _logger.LogInformation("OTP generated and stored for UserId: {UserId}. Expiry: {Expiry}", userId, expiry);
            else
                _logger.LogWarning("Failed to store OTP in Redis for UserId: {UserId}", userId);

            return otp;
        }

        public async Task<bool> ValidateOtpAsync(string userId, string otp)
        {
            var storedOtp = await _redis.StringGetAsync($"otp:{userId}");

            if (storedOtp.IsNullOrEmpty)
            {
                _logger.LogWarning("OTP validation failed: No OTP found or expired for UserId: {UserId}", userId);
                return false;
            }

            bool isValid = storedOtp == otp;

            if (isValid)
            {
                await _redis.KeyDeleteAsync($"otp:{userId}");
                _logger.LogInformation("OTP validated successfully for UserId: {UserId}", userId);
            }
            else
            {
                _logger.LogWarning("OTP validation failed: Invalid OTP for UserId: {UserId}", userId);
            }

            return isValid;
        }

        private string GenerateOtp()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            uint raw = BitConverter.ToUInt32(bytes, 0);
            uint otp = raw % 1_000_000; // 0..999999
            return otp.ToString("D6");
        }
    }
}
